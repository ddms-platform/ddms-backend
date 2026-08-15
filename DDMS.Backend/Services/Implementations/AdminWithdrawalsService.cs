using DDMS.Backend.Common.Constants;
using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.AdminWithdrawals;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class AdminWithdrawalsService : IAdminWithdrawalsService
{
    private readonly IAdminWithdrawalsRepository _repo;
    private readonly IWalletRepository _wallets;
    private readonly IEmailSender _email;
    private readonly INotificationService _notificationService;

    public AdminWithdrawalsService(
        IAdminWithdrawalsRepository repo,
        IWalletRepository wallets,
        IEmailSender email,
        INotificationService notificationService)
    {
        _repo = repo;
        _wallets = wallets;
        _email = email;
        _notificationService = notificationService;
    }

    public Task<List<WithdrawalItem>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);

    public Task ApproveAsync(Guid id, CancellationToken ct) =>
        UpdateStatusAsync(id, WithdrawalStatuses.Approved, refund: false, ct);

    public Task RejectAsync(Guid id, CancellationToken ct) =>
        UpdateStatusAsync(id, WithdrawalStatuses.Rejected, refund: true, ct);

    private async Task UpdateStatusAsync(Guid id, string newStatus, bool refund, CancellationToken ct)
    {
        var w = await _repo.FindWithUserAsync(id, ct)
            ?? throw new NotFoundException(ErrorCode.WithdrawalNotFound, ErrorCode.Messages.WithdrawalNotFound);

        if (w.status != WithdrawalStatuses.Pending)
            throw new AppException(ErrorCode.WithdrawalAlreadyProcessed, ErrorCode.Messages.WithdrawalAlreadyProcessed);

        w.status = newStatus;
        w.processed_at = DateTime.UtcNow;

        if (refund) await RefundAsync(w.user_id, w.amount, ct);

        await _repo.SaveChangesAsync(ct);
        await TrySendNotificationEmailAsync(w, newStatus);

        try
        {
            var code = w.id.ToString().Substring(0, 8).ToUpper();
            var isApproved = newStatus == WithdrawalStatuses.Approved;
            var title = isApproved ? "Rút tiền thành công 💰" : "Yêu cầu rút tiền bị từ chối ❌";
            var body = isApproved
                ? $"Lệnh rút tiền #{code} trị giá {w.amount:N0} đ về tài khoản {w.bank_name} ({w.account_number}) đã được xử lý thành công."
                : $"Yêu cầu rút tiền #{code} trị giá {w.amount:N0} đ đã bị từ chối. Số tiền đã được hoàn lại vào ví của bạn.";

            await _notificationService.CreateNotificationAsync(
                senderId: null,
                type: "system",
                title: title,
                body: body,
                recipientIds: new List<Guid> { w.user_id },
                data: null,
                ct: ct
            );
        }
        catch { /* best-effort */ }
    }

    private async Task RefundAsync(Guid userId, decimal amount, CancellationToken ct)
    {
        var wallet = await _wallets.FindAsync(userId, ct);
        if (wallet == null) return;
        wallet.balance += amount;
        wallet.updated_at = DateTime.UtcNow;
    }

    private async Task TrySendNotificationEmailAsync(wallet_withdrawal w, string status)
    {
        if (w.user == null) return;
        try
        {
            await _email.SendWithdrawalStatusEmailAsync(
                w.user.email,
                w.user.full_name,
                w.amount,
                w.bank_name,
                w.account_number,
                status);
        }
        catch
        {
            // best-effort: nuốt lỗi gửi mail
        }
    }
}
