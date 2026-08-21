using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

/// <summary>
/// Nguồn sự thật duy nhất về giá một booking. Mọi con số đều tra từ DB;
/// đơn giá do client gửi lên bị bỏ qua hoàn toàn.
/// </summary>
public class BookingPricingService : IBookingPricingService
{
    private readonly IBookingRepository _bookings;
    private readonly IPromotionsRepository _promotions;

    public BookingPricingService(IBookingRepository bookings, IPromotionsRepository promotions)
    {
        _bookings = bookings;
        _promotions = promotions;
    }

    public async Task<BookingQuote> QuoteAsync(
        Guid scheduleId,
        PartyComposition party,
        IReadOnlyCollection<BookingLineRequest> cabins,
        IReadOnlyCollection<BookingLineRequest> services,
        string? code,
        CancellationToken ct)
    {
        if (party.Total <= 0)
            throw new AppException(ErrorCode.UncategorizedError, "Số khách phải lớn hơn 0.");

        var schedule = await _bookings.FindScheduleWithTourAsync(scheduleId, ct)
            ?? throw new AppException(ErrorCode.ScheduleNotFound, "Lịch trình tour không tồn tại.");

        var cabinLines = await PriceLinesAsync(schedule.boat_id, cabins, isCabin: true, ct);
        var serviceLines = await PriceLinesAsync(schedule.boat_id, services, isCabin: false, ct);

        var partyLines = PriceParty(schedule.tour, party);

        var quote = new BookingQuote
        {
            BasePrice = partyLines.Sum(l => l.LineTotal),
            PartyLines = partyLines,
            CabinPrice = cabinLines.Sum(l => l.UnitPrice * l.Quantity),
            ServicePrice = serviceLines.Sum(l => l.UnitPrice * l.Quantity),
            CabinLines = cabinLines,
            ServiceLines = serviceLines,
        };
        quote.Subtotal = quote.BasePrice + quote.CabinPrice + quote.ServicePrice;
        quote.TotalPrice = quote.Subtotal;

        if (string.IsNullOrWhiteSpace(code))
            return quote;

        var promotion = await ValidateAsync(code.Trim(), quote.Subtotal, schedule, ct);
        quote.PromotionId = promotion.id;
        quote.PromotionCode = promotion.code;
        quote.PromotionDescription = Describe(promotion);
        quote.DiscountAmount = ComputeDiscount(promotion, quote.Subtotal);
        quote.TotalPrice = quote.Subtotal - quote.DiscountAmount;

        return quote;
    }

    /// <summary>
    /// Tách tiền tour theo hạng vé. Người lớn luôn 100%; trẻ em và em bé theo tỉ lệ
    /// owner khai báo trên tour.
    ///
    /// Làm tròn TỪNG DÒNG rồi mới cộng, không làm tròn tổng: PayOS chỉ nhận số
    /// nguyên, nên số ghi vào đơn phải khớp chính xác số gửi lên cổng.
    /// Hạng không có khách thì không tạo dòng, để hoá đơn khỏi hiện "0 trẻ em".
    /// </summary>
    private static List<PartyPricedLine> PriceParty(tour? tour, PartyComposition party)
    {
        var tourPrice = tour?.price ?? 0m;
        var lines = new List<PartyPricedLine>(3);

        void Add(string tier, int quantity, decimal percent)
        {
            if (quantity <= 0) return;

            var unitPrice = Round(tourPrice * percent / 100m);
            lines.Add(new PartyPricedLine
            {
                Tier = tier,
                Quantity = quantity,
                UnitPrice = unitPrice,
                LineTotal = Round(tourPrice * quantity * percent / 100m),
            });
        }

        Add(PassengerTiers.Adult, party.Adults, PassengerTiers.AdultPricePercent);
        Add(PassengerTiers.Child, party.Children, tour?.child_price_percent ?? PassengerTiers.DefaultChildPricePercent);
        Add(PassengerTiers.Infant, party.Infants, tour?.infant_price_percent ?? PassengerTiers.DefaultInfantPricePercent);

        return lines;
    }

    private static decimal Round(decimal value) => Math.Round(value, 0, MidpointRounding.AwayFromZero);

    /// <summary>Gán đơn giá từ DB cho từng dòng; id không thuộc tàu của lịch trình sẽ bị từ chối.</summary>
    private async Task<List<PricedLine>> PriceLinesAsync(
        Guid? boatId, IReadOnlyCollection<BookingLineRequest> lines, bool isCabin, CancellationToken ct)
    {
        if (lines.Count == 0) return [];

        if (boatId is null)
            throw new AppException(ErrorCode.ResourceNotFound, "Lịch trình chưa gán tàu, không thể tính giá.");

        var ids = lines.Select(l => l.Id).Distinct().ToList();
        var prices = isCabin
            ? await _bookings.GetCabinPricesAsync(boatId.Value, ids, ct)
            : await _bookings.GetServicePricesAsync(boatId.Value, ids, ct);

        var priced = new List<PricedLine>(lines.Count);
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw new AppException(ErrorCode.UncategorizedError, "Số lượng phải lớn hơn 0.");

            if (!prices.TryGetValue(line.Id, out var unitPrice))
                throw new AppException(
                    ErrorCode.ResourceNotFound,
                    isCabin
                        ? "Cabin không thuộc lịch trình đã chọn."
                        : "Dịch vụ không thuộc lịch trình đã chọn.");

            priced.Add(new PricedLine { Id = line.Id, Quantity = line.Quantity, UnitPrice = unitPrice });
        }
        return priced;
    }

    /// <summary>Kiểm tra mã theo thứ tự, mỗi lý do từ chối một mã lỗi riêng để FE báo đúng nguyên nhân.</summary>
    private async Task<promotion> ValidateAsync(
        string code, decimal subtotal, tour_schedule schedule, CancellationToken ct)
    {
        var promo = await _promotions.FindByCodeWithCreatorRolesAsync(code.ToLowerInvariant(), ct)
            ?? throw new AppException(ErrorCode.PromotionNotFound, ErrorCode.Messages.PromotionNotFound);

        if (promo.is_active != true || promo.status != PromotionStatuses.Approved)
            throw new AppException(ErrorCode.PromotionInactive, ErrorCode.Messages.PromotionInactive);

        var now = DateTime.UtcNow;
        if (promo.valid_from > now)
            throw new AppException(ErrorCode.PromotionNotStarted, ErrorCode.Messages.PromotionNotStarted);

        if (promo.valid_until is not null && promo.valid_until < now)
            throw new AppException(ErrorCode.PromotionExpired, ErrorCode.Messages.PromotionExpired);

        if (promo.usage_limit is not null && promo.used_count >= promo.usage_limit)
            throw new AppException(ErrorCode.PromotionUsageExhausted, ErrorCode.Messages.PromotionUsageExhausted);

        if (subtotal < promo.min_order_value)
            throw new AppException(ErrorCode.PromotionMinOrderNotMet, ErrorCode.Messages.PromotionMinOrderNotMet);

        if (!AppliesToSchedule(promo, schedule))
            throw new AppException(
                ErrorCode.PromotionNotApplicableToTour, ErrorCode.Messages.PromotionNotApplicableToTour);

        return promo;
    }

    /// <summary>
    /// Mã do owner tạo chỉ dùng được cho tour chạy trên tàu của chính owner đó.
    /// Mã do admin tạo (hoặc không rõ người tạo) áp cho mọi tour.
    /// Lịch trình chưa gán tàu thì không xác định được owner nên từ chối mã của owner.
    /// </summary>
    private static bool AppliesToSchedule(promotion promo, tour_schedule schedule)
    {
        var creator = promo.created_byNavigation;
        if (creator is null) return true;

        var isOwnerScoped = creator.user_roles.Any(ur => ur.role.name == RoleNames.Owner);
        if (!isOwnerScoped) return true;

        return schedule.boat?.owner_id == promo.created_by;
    }

    private static decimal ComputeDiscount(promotion promo, decimal subtotal)
    {
        var raw = promo.discount_type == DiscountTypes.DbPercent
            ? subtotal * promo.discount_value / 100m
            : promo.discount_value;

        if (promo.max_discount is not null && raw > promo.max_discount)
            raw = promo.max_discount.Value;

        // Kẹp lại để tổng thanh toán không bao giờ âm.
        if (raw > subtotal) raw = subtotal;
        if (raw < 0m) raw = 0m;

        return decimal.Round(raw, 2, MidpointRounding.AwayFromZero);
    }

    private static string Describe(promotion promo) =>
        promo.discount_type == DiscountTypes.DbPercent
            ? $"Giảm {promo.discount_value:0.##}%"
            : $"Giảm {promo.discount_value:N0}đ";
}
