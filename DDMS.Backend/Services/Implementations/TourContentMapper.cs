using DDMS.Backend.Models.DTOs.TourContent;
using DDMS.Backend.Models.Entities;

namespace DDMS.Backend.Services.Implementations;

internal static class TourContentMapper
{
    public static TourImageResponse Map(tour_image source) => new()
    {
        id = source.id,
        tour_id = source.tour_id,
        image_url = source.image_url,
        public_id = source.public_id,
        caption = source.caption,
        sort_order = source.sort_order
    };

    public static FaqResponse Map(faq source) => new()
    {
        id = source.id,
        tour_id = source.tour_id,
        question = source.question,
        answer = source.answer,
        sort_order = source.sort_order
    };

    public static DockScheduleResponse Map(dock_schedule source) => new()
    {
        id = source.id,
        dock_id = source.dock_id,
        boat_id = source.boat_id,
        schedule_id = source.schedule_id,
        start_time = source.start_time,
        end_time = source.end_time
    };
}
