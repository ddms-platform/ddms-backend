namespace DDMS.Backend.Models.DTOs.TourContent;

public class CreateTourImageRequest
{
    public Guid tour_id { get; set; }
    public string image_url { get; set; } = string.Empty;
    public string? public_id { get; set; }
    public string? caption { get; set; }
    public int sort_order { get; set; }
}

public class UpdateTourImageRequest : CreateTourImageRequest
{
}

public class TourImageResponse
{
    public Guid id { get; set; }
    public Guid tour_id { get; set; }
    public string image_url { get; set; } = string.Empty;
    public string? public_id { get; set; }
    public string? caption { get; set; }
    public int sort_order { get; set; }
}

public class CreateFaqRequest
{
    public Guid tour_id { get; set; }
    public string question { get; set; } = string.Empty;
    public string answer { get; set; } = string.Empty;
    public int sort_order { get; set; }
}

public class UpdateFaqRequest : CreateFaqRequest
{
}

public class FaqResponse
{
    public Guid id { get; set; }
    public Guid tour_id { get; set; }
    public string question { get; set; } = string.Empty;
    public string answer { get; set; } = string.Empty;
    public int sort_order { get; set; }
}

public class CreateDockScheduleRequest
{
    public Guid dock_id { get; set; }
    public Guid boat_id { get; set; }
    public Guid? schedule_id { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
}

public class UpdateDockScheduleRequest : CreateDockScheduleRequest
{
}

public class DockScheduleResponse
{
    public Guid id { get; set; }
    public Guid dock_id { get; set; }
    public Guid boat_id { get; set; }
    public Guid? schedule_id { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
}
