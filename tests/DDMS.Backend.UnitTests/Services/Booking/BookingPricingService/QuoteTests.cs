using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Booking;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Assertions;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using DDMS.Backend.Shared.Mocks.Repositories;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Booking.BookingPricingService;

/// <summary>
/// Kiểm tra service tính giá — nguồn sự thật duy nhất về số tiền một booking.
/// Phủ: tính giá từ dữ liệu server, từng luật từ chối mã giảm giá, cách tính mức
/// giảm (percent/fixed, trần max_discount, kẹp không âm) và phạm vi mã của owner.
/// </summary>
public class QuoteTests
{
    private const decimal TourPrice = 300_000m;
    private const decimal CabinPrice = 200_000m;
    private const decimal ServicePrice = 100_000m;
    private static readonly Guid ServiceId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    private static readonly IReadOnlyCollection<BookingLineRequest> NoLines = [];

    /// <summary>Đoàn toàn người lớn — các test dưới đây không quan tâm tới hạng vé.</summary>
    private static PartyComposition Adults(int count) => new(count, 0, 0);

    /// <summary>Dựng service với một lịch trình hợp lệ và bảng giá cabin/dịch vụ cố định.</summary>
    private static (DDMS.Backend.Services.Implementations.BookingPricingService Service,
                    Mock<DDMS.Backend.Repositories.Interfaces.IPromotionsRepository> Promotions)
        Build(promotion? promo = null, Guid? boatOwnerId = null)
    {
        var bookings = BookingRepositoryMockFactory.Create();
        var promotions = PromotionsRepositoryMockFactory.Create();

        var boat = new BoatBuilder().WithOwnerId(boatOwnerId ?? TestGuids.OwnerId).Build();
        var schedule = new TourScheduleBuilder()
            .WithTour(new TourBuilder().WithPrice(TourPrice).Build())
            .WithBoat(boat)
            .Build();

        bookings.Setup(r => r.FindScheduleWithTourAsync(TestGuids.ScheduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        bookings.Setup(r => r.GetCabinPricesAsync(
                boat.id, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal> { [TestGuids.CabinId] = CabinPrice });
        bookings.Setup(r => r.GetServicePricesAsync(
                boat.id, It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, decimal> { [ServiceId] = ServicePrice });

        promotions.Setup(r => r.FindByCodeWithCreatorRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(promo);

        return (new DDMS.Backend.Services.Implementations.BookingPricingService(
            bookings.Object, promotions.Object), promotions);
    }

    /// <summary>Mã hợp lệ mặc định: giảm 10%, không trần, không giới hạn lượt, do admin tạo.</summary>
    private static promotion ValidPromo(Action<promotion>? tweak = null)
    {
        var promo = new promotion
        {
            id = TestGuids.PromotionId,
            code = "summer25",
            discount_type = DiscountTypes.DbPercent,
            discount_value = 10m,
            min_order_value = 0m,
            usage_limit = null,
            used_count = 0,
            valid_from = DateTime.UtcNow.AddDays(-1),
            valid_until = DateTime.UtcNow.AddDays(30),
            is_active = true,
            status = PromotionStatuses.Approved,
            created_by = null,
        };
        tweak?.Invoke(promo);
        return promo;
    }

    private static List<BookingLineRequest> Cabin(int qty = 1) =>
        [new() { Id = TestGuids.CabinId, Quantity = qty }];

    private static List<BookingLineRequest> Service(int qty = 1) =>
        [new() { Id = ServiceId, Quantity = qty }];

    [Fact]
    public async Task Quote_ComputesEveryComponentFromServerData()
    {
        var (service, _) = Build();

        var quote = await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(2), Cabin(), Service(), null, CancellationToken.None);

        quote.BasePrice.Should().Be(TourPrice * 2);
        quote.CabinPrice.Should().Be(CabinPrice);
        quote.ServicePrice.Should().Be(ServicePrice);
        quote.Subtotal.Should().Be(TourPrice * 2 + CabinPrice + ServicePrice);
        quote.DiscountAmount.Should().Be(0m);
        quote.TotalPrice.Should().Be(quote.Subtotal);
        quote.PromotionId.Should().BeNull();
    }

    [Fact]
    public async Task Quote_IgnoresClientPrices_UsesDbUnitPrice()
    {
        var (service, _) = Build();

        // Client chỉ nói "1 cabin này" — không có chỗ nào để khai đơn giá.
        var quote = await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), Cabin(), NoLines, null, CancellationToken.None);

        quote.CabinLines.Should().ContainSingle()
            .Which.UnitPrice.Should().Be(CabinPrice);
    }

    [Fact]
    public async Task Quote_PercentDiscount_AppliedOnSubtotal()
    {
        var (service, _) = Build(ValidPromo());

        var quote = await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), NoLines, NoLines, "SUMMER25", CancellationToken.None);

        quote.Subtotal.Should().Be(TourPrice);
        quote.DiscountAmount.Should().Be(TourPrice * 0.1m);
        quote.TotalPrice.Should().Be(TourPrice * 0.9m);
        quote.PromotionCode.Should().Be("summer25");
    }

    [Fact]
    public async Task Quote_PercentDiscount_CappedByMaxDiscount()
    {
        var (service, _) = Build(ValidPromo(p =>
        {
            p.discount_value = 50m;
            p.max_discount = 20_000m;
        }));

        var quote = await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), NoLines, NoLines, "SUMMER25", CancellationToken.None);

        // 50% của 300k là 150k nhưng trần chỉ 20k.
        quote.DiscountAmount.Should().Be(20_000m);
        quote.TotalPrice.Should().Be(TourPrice - 20_000m);
    }

    [Fact]
    public async Task Quote_FixedDiscountLargerThanSubtotal_ClampsToZeroTotal()
    {
        var (service, _) = Build(ValidPromo(p =>
        {
            p.discount_type = "fixed";
            p.discount_value = 999_000_000m;
        }));

        var quote = await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), NoLines, NoLines, "SUMMER25", CancellationToken.None);

        quote.DiscountAmount.Should().Be(TourPrice);
        quote.TotalPrice.Should().Be(0m);
    }

    [Theory]
    [InlineData("inactive", ErrorCode.PromotionInactive)]
    [InlineData("notApproved", ErrorCode.PromotionInactive)]
    [InlineData("notStarted", ErrorCode.PromotionNotStarted)]
    [InlineData("expired", ErrorCode.PromotionExpired)]
    [InlineData("exhausted", ErrorCode.PromotionUsageExhausted)]
    [InlineData("minOrder", ErrorCode.PromotionMinOrderNotMet)]
    public async Task Quote_RejectsInvalidPromotion_WithSpecificErrorCode(string scenario, int expectedCode)
    {
        var promo = ValidPromo(p =>
        {
            switch (scenario)
            {
                case "inactive": p.is_active = false; break;
                case "notApproved": p.status = PromotionStatuses.Pending; break;
                case "notStarted": p.valid_from = DateTime.UtcNow.AddDays(5); break;
                case "expired": p.valid_until = DateTime.UtcNow.AddDays(-1); break;
                case "exhausted": p.usage_limit = 5; p.used_count = 5; break;
                case "minOrder": p.min_order_value = TourPrice + 1m; break;
            }
        });
        var (service, _) = Build(promo);

        var act = async () => await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), NoLines, NoLines, "SUMMER25", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.ShouldBeAppException(expectedCode);
    }

    [Fact]
    public async Task Quote_UnknownCode_ThrowsNotFound()
    {
        var (service, _) = Build(promo: null);

        var act = async () => await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), NoLines, NoLines, "NOPE", CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.PromotionNotFound);
    }

    [Fact]
    public async Task Quote_OwnerCode_RejectedOnAnotherOwnersTour()
    {
        var otherOwner = TestGuids.OtherUserId;
        var ownerPromo = ValidPromo(p =>
        {
            p.created_by = otherOwner;
            p.created_byNavigation = OwnerUser(otherOwner);
        });
        // Tàu của lịch trình thuộc TestGuids.OwnerId, khác chủ mã.
        var (service, _) = Build(ownerPromo, boatOwnerId: TestGuids.OwnerId);

        var act = async () => await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), NoLines, NoLines, "SUMMER25", CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.PromotionNotApplicableToTour);
    }

    [Fact]
    public async Task Quote_OwnerCode_AcceptedOnOwnTour()
    {
        var ownerPromo = ValidPromo(p =>
        {
            p.created_by = TestGuids.OwnerId;
            p.created_byNavigation = OwnerUser(TestGuids.OwnerId);
        });
        var (service, _) = Build(ownerPromo, boatOwnerId: TestGuids.OwnerId);

        var quote = await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), NoLines, NoLines, "SUMMER25", CancellationToken.None);

        quote.DiscountAmount.Should().Be(TourPrice * 0.1m);
    }

    [Fact]
    public async Task Quote_UnknownCabinId_Rejected()
    {
        var (service, _) = Build();
        var strangerCabin = new List<BookingLineRequest>
        {
            new() { Id = Guid.NewGuid(), Quantity = 1 },
        };

        var act = async () => await service.QuoteAsync(
            TestGuids.ScheduleId, Adults(1), strangerCabin, NoLines, null, CancellationToken.None);

        (await act.Should().ThrowAsync<AppException>())
            .Which.ShouldBeAppException(ErrorCode.ResourceNotFound);
    }

    private static user OwnerUser(Guid id) => new()
    {
        id = id,
        email = $"owner-{id:N}@ddms.local",
        full_name = "Owner",
        user_roles = new List<user_role>
        {
            new() { user_id = id, role = new role { name = RoleNames.Owner } },
        },
    };
}
