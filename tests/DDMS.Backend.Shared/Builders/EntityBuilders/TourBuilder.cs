using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

public class TourBuilder
{
    private Guid _id = TestGuids.TourId;
    private string _name = "Tour Ngắm Hoàng Hôn Sông Hàn";
    private decimal _price = 300_000m;
    private int _durationMinutes = 120;
    private string _location = "Đà Nẵng";
    private string _status = "active";
    private string _cancelPolicy = "free";
    private readonly List<tour_image> _images = new();

    public TourBuilder WithId(Guid id) { _id = id; return this; }
    public TourBuilder WithName(string name) { _name = name; return this; }
    public TourBuilder WithPrice(decimal price) { _price = price; return this; }
    public TourBuilder WithStatus(string status) { _status = status; return this; }
    public TourBuilder WithImages(params tour_image[] images) { _images.Clear(); _images.AddRange(images); return this; }

    public tour Build()
    {
        var tour = new tour
        {
            id = _id,
            name = _name,
            price = _price,
            duration_minutes = _durationMinutes,
            location = _location,
            status = _status,
            cancel_policy = _cancelPolicy,
            created_at = DateTime.UtcNow.AddDays(-200),
            updated_at = DateTime.UtcNow.AddDays(-200),
            tour_images = _images
        };

        foreach (var image in _images)
        {
            image.tour_id = tour.id;
            image.tour = tour;
        }

        return tour;
    }
}
