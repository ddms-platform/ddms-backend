using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Repositories.Interfaces;

public interface IPublicOwnersRepository
{
    /// <summary>Chủ thuyền đã được cảng vụ duyệt, mới nhất trước.</summary>
    Task<List<owner_profile>> GetVerifiedProfilesAsync(int take, CancellationToken ct);

    /// <summary>Tàu còn hoạt động của các chủ thuyền này, kèm ảnh.</summary>
    Task<List<boat>> GetActiveBoatsWithImagesAsync(
        IReadOnlyCollection<Guid> ownerIds, CancellationToken ct);

    /// <summary>
    /// Map tàu -> các tour có lịch chạy trên con tàu đó.
    /// Đếm tour qua lịch trình chứ không qua tour.created_by, vì cột đó
    /// không phải lúc nào cũng được ghi.
    /// </summary>
    Task<List<(Guid BoatId, Guid TourId)>> GetTourIdsByBoatAsync(
        IReadOnlyCollection<Guid> boatIds, CancellationToken ct);

    /// <summary>Tổng điểm và số lượt đánh giá theo tour.</summary>
    Task<List<(Guid TourId, double AvgRating, int ReviewCount)>> GetRatingsByTourAsync(
        IReadOnlyCollection<Guid> tourIds, CancellationToken ct);
}
