using DDMS.Backend.Models.DTOs.OwnerServices;
using DDMS.Backend.Models.DTOs.Tour;

namespace DDMS.Backend.Services.Interfaces;

public interface IOwnerServicesRegistrationService
{
    /// <summary>
    /// Đăng ký hoặc cập nhật một dịch vụ (mỗi dịch vụ là một tour) trên thuyền.
    /// </summary>
    /// <param name="userId">
    /// Người đang gọi. Thuyền phải thuộc về chính người này — trước đây hàm chỉ
    /// lấy owner_id từ con thuyền nên ai biết boatId cũng đăng ký được.
    /// </param>
    Task<TourResponse> RegisterAsync(DynamicServiceRequest request, Guid userId, CancellationToken ct);
}
