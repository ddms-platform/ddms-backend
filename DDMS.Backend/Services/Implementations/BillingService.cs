using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Hubs;
using DDMS.Backend.Models.DTOs.Billing;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Services.Implementations;

public class BillingService : IBillingService
{
    private readonly IBillingRepository _repo;
    private readonly PayOSClient _payOS;
    private readonly IHubContext<BillingHub> _hub;

    public BillingService(IBillingRepository repo, PayOSClient payOS, IHubContext<BillingHub> hub)
    {
        _repo = repo;
        _payOS = payOS;
        _hub = hub;
    }

    public async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid ownerId, CancellationToken ct)
    {
        var charges = await ComputeChargesAsync(ownerId, ct);
        var payments = await _repo.GetOwnerPaymentsAsync(ownerId, ct);
        var totalPaid = payments.Where(p => p.status == "paid").Sum(p => p.amount);

        var totalOwed = charges.CommissionOwed + charges.MaintenanceOwed + charges.DockRentalOwed;

        return new FinancialSummaryResponse
        {
            TotalBookingRevenue = charges.TotalBookingRevenue,
            TotalOwed = totalOwed,
            CommissionOwed = charges.CommissionOwed,
            MaintenanceOwed = charges.MaintenanceOwed,
            DockRentalOwed = charges.DockRentalOwed,
            TotalPaid = totalPaid,
            RemainingBalance = Math.Max(0m, totalOwed - totalPaid),
            Bookings = charges.Bookings,
            Maintenances = charges.Maintenances,
            DockRentals = charges.DockRentals,
            PaymentHistory = payments.Select(MapPaymentHistory).ToList()
        };
    }

    public async Task<PaymentInitResult> InitiatePaymentAsync(Guid ownerId, CancellationToken ct)
    {
        EnsurePayOSConfigured();

        var charges = await ComputeChargesAsync(ownerId, ct);
        var totalPaid = await _repo.GetOwnerTotalPaidAsync(ownerId, ct);
        var remaining = Math.Max(0m,
            charges.CommissionOwed + charges.MaintenanceOwed + charges.DockRentalOwed - totalPaid);

        if (remaining <= 0)
            throw new AppException(ErrorCode.UncategorizedError, "Bạn không có dư nợ cần thanh toán.");

        var orderCode = (long)(DateTime.UtcNow - BillingRates.OrderCodeEpoch).TotalMilliseconds;

        try
        {
            var result = await _payOS.PaymentRequests.CreateAsync(new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (int)Math.Round(remaining),
                Description = $"Dư nợ chủ tàu {orderCode}",
                ReturnUrl = BillingRates.PayOSReturnUrl,
                CancelUrl = BillingRates.PayOSCancelUrl
            });

            await _repo.AddPaymentAsync(new owner_payment
            {
                id = Guid.NewGuid(),
                owner_id = ownerId,
                amount = remaining,
                status = "pending",
                payos_order_code = orderCode,
                description = $"Thanh toán dư nợ chủ tàu - Mã {orderCode}",
                created_at = DateTime.UtcNow
            }, ct);
            await _repo.SaveChangesAsync(ct);

            return new PaymentInitResult
            {
                CheckoutUrl = result.CheckoutUrl,
                OrderCode = orderCode,
                QrCode = result.QrCode,
                AccountNumber = result.AccountNumber,
                AccountName = result.AccountName,
                Bin = result.Bin
            };
        }
        catch (AppException) { throw; }
        catch (Exception ex)
        {
            throw new AppException(ErrorCode.UncategorizedError,
                $"Lỗi khi tạo yêu cầu thanh toán qua PayOS: {ex.Message}");
        }
    }

    public async Task<WebhookHandleResult> HandlePayOSWebhookAsync(Webhook webhookBody, CancellationToken ct)
    {
        try
        {
            var verified = await _payOS.Webhooks.VerifyAsync(webhookBody);
            if (verified == null)
                return new WebhookHandleResult("01", "Chữ ký không hợp lệ", false);

            var payment = await _repo.FindPendingPaymentByOrderCodeAsync(verified.OrderCode, ct);
            if (payment != null)
            {
                payment.status = "paid";
                payment.paid_at = DateTime.UtcNow;
                await _repo.SaveChangesAsync(ct);

                var ownerPayload = new { paymentId = payment.id, status = "paid", amount = payment.amount };
                var broadcastPayload = new { paymentId = payment.id, ownerId = payment.owner_id, status = "paid", amount = payment.amount };

                await _hub.Clients.Group(payment.owner_id.ToString())
                    .SendAsync("PaymentReceived", ownerPayload, ct);
                await _hub.Clients.All.SendAsync("PaymentReceived", broadcastPayload, ct);
            }

            return new WebhookHandleResult("00", "Thành công", true);
        }
        catch (Exception ex)
        {
            return new WebhookHandleResult("99", $"Lỗi xử lý webhook: {ex.Message}", false);
        }
    }

    private void EnsurePayOSConfigured()
    {
        if (string.IsNullOrWhiteSpace(_payOS.ClientId)
         || string.IsNullOrWhiteSpace(_payOS.ApiKey)
         || string.IsNullOrWhiteSpace(_payOS.ChecksumKey))
        {
            throw new AppException(ErrorCode.UncategorizedError,
                "Cấu hình cổng thanh toán PayOS chưa được thiết lập. Vui lòng bổ sung ClientId, ApiKey và ChecksumKey vào file appsettings.json trên server.");
        }
    }

    private async Task<ChargesSnapshot> ComputeChargesAsync(Guid ownerId, CancellationToken ct)
    {
        var bookings = await _repo.GetOwnerRevenueBookingsAsync(ownerId, ct);
        var maintenances = await _repo.GetOwnerApprovedMaintenancesAsync(ownerId, ct);
        var boats = await _repo.GetOwnerBoatsAsync(ownerId, ct);
        var schedules = await _repo.GetSchedulesForBoatsAsync(boats.Select(b => b.id).ToList(), ct);

        var dockRentals = BuildDockRentals(boats, schedules);

        return new ChargesSnapshot
        {
            TotalBookingRevenue = bookings.Sum(b => b.total_price),
            CommissionOwed = bookings.Sum(b => b.total_price * BillingRates.Commission),
            MaintenanceOwed = maintenances.Sum(m => m.port_maintenance_service?.price ?? 0m),
            DockRentalOwed = dockRentals.Sum(d => d.Amount),
            Bookings = bookings.Select(MapBooking).ToList(),
            Maintenances = maintenances.Select(MapMaintenance).ToList(),
            DockRentals = dockRentals
        };
    }

    private static List<DockRentalItem> BuildDockRentals(List<boat> boats, List<dock_schedule> schedules)
    {
        var items = new List<DockRentalItem>();
        foreach (var boat in boats)
        {
            var months = new HashSet<(int Year, int Month)>();
            foreach (var s in schedules.Where(x => x.boat_id == boat.id))
            {
                var current = new DateTime(s.start_time.Year, s.start_time.Month, 1);
                var end = new DateTime(s.end_time.Year, s.end_time.Month, 1);
                while (current <= end)
                {
                    months.Add((current.Year, current.Month));
                    current = current.AddMonths(1);
                }
            }
            foreach (var (year, month) in months.OrderBy(m => m.Year).ThenBy(m => m.Month))
            {
                items.Add(new DockRentalItem
                {
                    BoatId = boat.id,
                    BoatName = boat.name,
                    RegistrationNumber = boat.registration_number ?? "",
                    Year = year,
                    Month = month,
                    Amount = BillingRates.MonthlyDockRental
                });
            }
        }
        return items;
    }

    private static BookingRevenueItem MapBooking(booking b) => new()
    {
        BookingId = b.id,
        TourName = b.schedule.tour.name,
        CustomerName = b.user.full_name ?? "Khách hàng",
        BookingDate = b.created_at,
        TotalPrice = b.total_price,
        Status = b.status,
        Commission = b.total_price * BillingRates.Commission
    };

    private static MaintenanceFeeItem MapMaintenance(boat_maintenance m) => new()
    {
        MaintenanceId = m.id,
        BoatName = m.boat.name,
        ServiceName = m.port_maintenance_service?.name ?? m.reason ?? "Dịch vụ bảo trì",
        StartTime = m.start_time,
        EndTime = m.end_time,
        Status = m.status,
        Amount = m.port_maintenance_service?.price ?? 0m
    };

    private static PaymentHistoryItem MapPaymentHistory(owner_payment p) => new()
    {
        PaymentId = p.id,
        Amount = p.amount,
        Status = p.status,
        PayosOrderCode = p.payos_order_code,
        Description = p.description,
        CreatedAt = p.created_at,
        PaidAt = p.paid_at
    };

    private sealed class ChargesSnapshot
    {
        public decimal TotalBookingRevenue { get; init; }
        public decimal CommissionOwed { get; init; }
        public decimal MaintenanceOwed { get; init; }
        public decimal DockRentalOwed { get; init; }
        public List<BookingRevenueItem> Bookings { get; init; } = new();
        public List<MaintenanceFeeItem> Maintenances { get; init; } = new();
        public List<DockRentalItem> DockRentals { get; init; } = new();
    }
}
