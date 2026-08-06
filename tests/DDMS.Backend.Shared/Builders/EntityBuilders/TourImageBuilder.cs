using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

public class TourImageBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _tourId = TestGuids.TourId;
    private string _imageUrl = "https://example.com/tour.jpg";
    private int _sortOrder;

    public TourImageBuilder WithTourId(Guid tourId) { _tourId = tourId; return this; }
    public TourImageBuilder WithImageUrl(string imageUrl) { _imageUrl = imageUrl; return this; }
    public TourImageBuilder WithSortOrder(int sortOrder) { _sortOrder = sortOrder; return this; }

    public tour_image Build() => new()
    {
        id = _id,
        tour_id = _tourId,
        image_url = _imageUrl,
        sort_order = _sortOrder,
        created_at = DateTime.UtcNow.AddDays(-100)
    };
}
