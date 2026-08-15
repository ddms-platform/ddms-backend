using DDMS.Backend.Models.DTOs.PublicOwners;

namespace DDMS.Backend.Services.Interfaces;

public interface IPublicOwnersService
{
    /// <summary>
    /// Chủ thuyền đã xác thực để hiển thị ở trang chủ.
    /// Chưa có ai được duyệt thì trả về danh sách rỗng — frontend ẩn khối đi,
    /// không lấp bằng dữ liệu mẫu.
    /// </summary>
    Task<List<FeaturedOwnerResponse>> GetFeaturedAsync(int take, CancellationToken ct);
}
