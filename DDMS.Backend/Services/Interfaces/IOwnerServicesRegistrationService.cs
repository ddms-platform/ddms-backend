using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerServicesRegistrationService
{
    /// <summary>
    /// Đăng ký hoặc cập nhật một dịch vụ (mỗi dịch vụ là một tour) trên thuyền.
    /// Tour mới / chưa duyệt → hàng duyệt tour. Tour đang bán → phiếu sửa dịch vụ.
    /// </summary>
    Task<TourResponse> RegisterAsync(DynamicServiceRequest request, Guid userId, CancellationToken ct);

    Task<List<ServiceChangeRequestResponse>> ListChangesAsync(string? status, CancellationToken ct);

    Task<ServiceChangeRequestResponse> ApproveChangeAsync(Guid changeId, CancellationToken ct);

    Task<ServiceChangeRequestResponse> RejectChangeAsync(
        Guid changeId, string reason, CancellationToken ct);
}
