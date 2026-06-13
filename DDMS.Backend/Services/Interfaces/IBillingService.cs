using DDMS.Backend.Models.DTOs.Billing;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Services.Interfaces;

public interface IBillingService
{
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid ownerId, CancellationToken ct);
    Task<PaymentInitResult> InitiatePaymentAsync(Guid ownerId, CancellationToken ct);
    Task<WebhookHandleResult> HandlePayOSWebhookAsync(Webhook webhookBody, CancellationToken ct);
}
