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
        /// Map tàu -> các tour public đang mở (active + lịch scheduled tương lai)
        /// trên con tàu đó. Không đếm tour nháp / hết lịch.
        /// </summary>
    Task<List<(Guid BoatId, Guid TourId)>> GetTourIdsByBoatAsync(
        IReadOnlyCollection<Guid> boatIds, CancellationToken ct);

    /// <summary>Tổng điểm và số lượt đánh giá theo tour.</summary>
    Task<List<(Guid TourId, double AvgRating, int ReviewCount)>> GetRatingsByTourAsync(
        IReadOnlyCollection<Guid> tourIds, CancellationToken ct);
}
