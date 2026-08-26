using DDMS.Backend.Models.DTOs.PublicOwners;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class PublicOwnersService : IPublicOwnersService
{
    private const int MaxImagesPerOwner = 4;

    private readonly IPublicOwnersRepository _repo;

    public PublicOwnersService(IPublicOwnersRepository repo) => _repo = repo;

    public async Task<List<FeaturedOwnerResponse>> GetFeaturedAsync(int take, CancellationToken ct)
    {
        // Lấy dư hồ sơ rồi lọc người đã có tour public — Take(take) trước sẽ
        // thiếu card nếu vài chủ thuyền verified chưa mở tour.
        var profileTake = Math.Clamp(take * 8, take, 48);
        var profiles = await _repo.GetVerifiedProfilesAsync(profileTake, ct);
        if (profiles.Count == 0) return new List<FeaturedOwnerResponse>();

        var ownerIds = profiles.Select(p => p.user_id).Distinct().ToList();
        var boats = await _repo.GetActiveBoatsWithImagesAsync(ownerIds, ct);

        var boatIds = boats.Select(b => b.id).ToList();
        var boatTours = await _repo.GetTourIdsByBoatAsync(boatIds, ct);

        var allTourIds = boatTours.Select(x => x.TourId).Distinct().ToList();
        var ratings = (await _repo.GetRatingsByTourAsync(allTourIds, ct))
            .ToDictionary(r => r.TourId, r => (r.AvgRating, r.ReviewCount));

        var boatsByOwner = boats
            .Where(b => b.owner_id != null)
            .GroupBy(b => b.owner_id!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var toursByBoat = boatTours
            .GroupBy(x => x.BoatId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.TourId).ToList());

        var result = new List<FeaturedOwnerResponse>();

        foreach (var profile in profiles)
        {
            var ownerBoats = boatsByOwner.TryGetValue(profile.user_id, out var bs)
                ? bs
                : new List<Models.Entities.boat>();

            var tourIds = ownerBoats
                .SelectMany(b => toursByBoat.TryGetValue(b.id, out var ts) ? ts : new List<Guid>())
                .Distinct()
                .ToList();

            // Trung bình có trọng số theo số lượt đánh giá, không phải trung bình
            // của các trung bình — tour 1 đánh giá không cân bằng tour 50 đánh giá.
            var rated = tourIds
                .Where(ratings.ContainsKey)
                .Select(id => ratings[id])
                .ToList();

            var reviewCount = rated.Sum(r => r.ReviewCount);
            double? avgRating = reviewCount > 0
                ? Math.Round(rated.Sum(r => r.AvgRating * r.ReviewCount) / reviewCount, 1)
                : null;

            result.Add(new FeaturedOwnerResponse
            {
                Id = profile.id,
                UserId = profile.user_id,
                Name = !string.IsNullOrWhiteSpace(profile.business_name)
                    ? profile.business_name!
                    : profile.user?.full_name ?? "Chủ thuyền",
                EntityType = profile.entity_type,
                Bio = profile.bio,
                BoatCount = ownerBoats.Count,
                TourCount = tourIds.Count,
                AvgRating = avgRating,
                ReviewCount = reviewCount,
                BoatImages = ownerBoats
                    .SelectMany(b => b.boat_images.OrderBy(i => i.sort_order))
                    .Select(i => i.image_url)
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Take(MaxImagesPerOwner)
                    .ToList(),
            });
        }

        // Chỉ hiện đối tác đã có tour khách đặt được. Chưa có tour thì không lên trang chủ.
        return result.Where(r => r.BoatCount > 0 && r.TourCount > 0).Take(take).ToList();
    }
}
