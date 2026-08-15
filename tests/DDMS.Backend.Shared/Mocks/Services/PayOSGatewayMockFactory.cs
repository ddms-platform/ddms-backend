using DDMS.Backend.Services.Interfaces;
using Moq;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace DDMS.Backend.Shared.Mocks.Services;

public static class PayOSGatewayMockFactory
{
    /// <summary>
    /// Mặc định: đã cấu hình, nhưng mọi webhook đều KHÔNG qua được bước xác minh chữ ký
    /// và không tra được đơn nào. Test nào cần "đã trả tiền" thì phải nói rõ ra.
    /// </summary>
    public static Mock<IPayOSGateway> Create()
    {
        var mock = new Mock<IPayOSGateway>();

        mock.SetupGet(g => g.IsConfigured).Returns(true);
        mock.Setup(g => g.VerifyWebhookAsync(It.IsAny<Webhook>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WebhookData?)null);
        mock.Setup(g => g.GetPaymentLinkAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentLink?)null);
        mock.Setup(g => g.CreatePaymentLinkAsync(It.IsAny<CreatePaymentLinkRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreatePaymentLinkResponse
            {
                CheckoutUrl = "https://pay.payos.vn/web/test",
                QrCode = "00020101021138...",
                AccountNumber = "0935566373",
                AccountName = "DDMS PORTAL",
                Bin = "970422",
                Status = PaymentLinkStatus.Pending,
            });

        return mock;
    }

    /// <summary>Webhook có chữ ký hợp lệ cho đúng mã đơn và số tiền này.</summary>
    public static Mock<IPayOSGateway> WithValidWebhook(long orderCode, long amount)
    {
        var mock = Create();
        mock.Setup(g => g.VerifyWebhookAsync(It.IsAny<Webhook>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebhookData { OrderCode = orderCode, Amount = amount });
        return mock;
    }

    /// <summary>PayOS báo đơn đang ở trạng thái <paramref name="status"/>.</summary>
    public static Mock<IPayOSGateway> WithLinkStatus(
        long orderCode, PaymentLinkStatus status, long amountPaid = 0)
    {
        var mock = Create();
        mock.Setup(g => g.GetPaymentLinkAsync(orderCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentLink
            {
                OrderCode = orderCode,
                Status = status,
                AmountPaid = amountPaid,
            });
        return mock;
    }
}
