using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class BoatComplianceNotifier : IBoatComplianceNotifier
{
    private readonly INotificationService _notificationService;

    public BoatComplianceNotifier(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task NotifyCertificateApprovedAsync(boat_certificate certificate, CancellationToken ct = default)
    {
        var ownerId = certificate.boat?.owner_id;
        if (!ownerId.HasValue) return;

        var boatName = certificate.boat?.name ?? "phương tiện";
        var title = "Phương tiện đã được phê duyệt ✅";
        var body = $"Chúc mừng! Giấy tờ {certificate.certificate_type} của tàu {boatName} đã được kiểm duyệt thành công và sẵn sàng hoạt động trên hệ thống.";

        await _notificationService.CreateNotificationAsync(
            senderId: null,
            type: "system",
            title: title,
            body: body,
            recipientIds: new List<Guid> { ownerId.Value },
            data: null,
            ct: ct
        );
    }

    public async Task NotifyCertificateRejectedAsync(boat_certificate certificate, CancellationToken ct = default)
    {
        var ownerId = certificate.boat?.owner_id;
        if (!ownerId.HasValue) return;

        var boatName = certificate.boat?.name ?? "phương tiện";
        var reason = !string.IsNullOrWhiteSpace(certificate.rejection_reason)
            ? certificate.rejection_reason
            : "Hồ sơ chưa đạt yêu cầu";
        var title = "Yêu cầu kiểm duyệt bị từ chối ❌";
        var body = $"Yêu cầu kiểm duyệt giấy tờ {certificate.certificate_type} cho tàu {boatName} không được duyệt. Lý do: {reason}. Vui lòng cập nhật lại hồ sơ.";

        await _notificationService.CreateNotificationAsync(
            senderId: null,
            type: "system",
            title: title,
            body: body,
            recipientIds: new List<Guid> { ownerId.Value },
            data: null,
            ct: ct
        );
    }

    public async Task NotifyCertificateExpiringSoonAsync(boat_certificate certificate, CancellationToken ct = default)
    {
        var ownerId = certificate.boat?.owner_id;
        if (!ownerId.HasValue) return;

        var boatName = certificate.boat?.name ?? "phương tiện";
        var title = "Giấy tờ sắp hết hạn ⚠️";
        var body = $"Giấy tờ {certificate.certificate_type} của tàu {boatName} sắp hết hạn vào ngày {certificate.expiry_date:dd/MM/yyyy}. Vui lòng gia hạn sớm.";

        await _notificationService.CreateNotificationAsync(
            senderId: null,
            type: "system",
            title: title,
            body: body,
            recipientIds: new List<Guid> { ownerId.Value },
            data: null,
            ct: ct
        );
    }

    public async Task NotifyCertificateExpiredHiddenAsync(boat boat, CancellationToken ct = default)
    {
        if (!boat.owner_id.HasValue) return;

        var title = "Tàu tạm thời bị ẩn do giấy tờ hết hạn ⚠️";
        var body = $"Tàu {boat.name} tạm thời bị ẩn khỏi hệ thống đặt tour do có giấy tờ pháp lý đã hết hạn.";

        await _notificationService.CreateNotificationAsync(
            senderId: null,
            type: "system",
            title: title,
            body: body,
            recipientIds: new List<Guid> { boat.owner_id.Value },
            data: null,
            ct: ct
        );
    }

    public async Task NotifyCertificateLockedAsync(boat boat, CancellationToken ct = default)
    {
        if (!boat.owner_id.HasValue) return;

        var title = "Tàu đã bị khóa do quá hạn giấy tờ 🔴";
        var body = $"Tàu {boat.name} đã bị khóa tự động do quá hạn bổ sung giấy tờ kiểm định hợp lệ.";

        await _notificationService.CreateNotificationAsync(
            senderId: null,
            type: "system",
            title: title,
            body: body,
            recipientIds: new List<Guid> { boat.owner_id.Value },
            data: null,
            ct: ct
        );
    }
}
