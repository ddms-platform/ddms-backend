namespace DDMS.Backend.Models.DTOs.PublicOwners;

/// <summary>
/// Chủ thuyền đã được cảng vụ xác thực, hiển thị ở khối "Đối tác" trang chủ.
/// Mọi con số ở đây đều đếm từ DB — không có giá trị mặc định đẹp mắt nào.
/// </summary>
public class FeaturedOwnerResponse
{
    public Guid Id { get; set; }

    /// <summary>Id user chủ thuyền — dùng để lọc tour public, khác Id hồ sơ.</summary>
    public Guid UserId { get; set; }

    /// <summary>Tên doanh nghiệp, rơi về tên người dùng nếu chủ thuyền là cá nhân.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>individual | business | cooperative</summary>
    public string EntityType { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public int BoatCount { get; set; }

    public int TourCount { get; set; }

    /// <summary>Null khi chưa có đánh giá nào — frontend không hiển thị sao.</summary>
    public double? AvgRating { get; set; }

    public int ReviewCount { get; set; }

    /// <summary>Ảnh tàu thật của chủ thuyền, tối đa 4 tấm.</summary>
    public List<string> BoatImages { get; set; } = new();
}
