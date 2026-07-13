namespace DDMS.Backend.Models.DTOs.TourSchedule;

public class CreateTourScheduleRequest
{
    public Guid tour_id { get; set; }
    public Guid? boat_id { get; set; }
    public Guid? dock_id { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
    public string status { get; set; } = "scheduled";
}

public class UpdateTourScheduleRequest : CreateTourScheduleRequest
{
}

public class TourScheduleResponse
{
    public Guid id { get; set; }
    public Guid tour_id { get; set; }
    public Guid? boat_id { get; set; }
    public string? boatName { get; set; }
    public List<string> boatImageUrls { get; set; } = new();
    public Guid? dock_id { get; set; }
    public DateTime start_time { get; set; }
    public DateTime end_time { get; set; }
    public string status { get; set; } = string.Empty;
}
