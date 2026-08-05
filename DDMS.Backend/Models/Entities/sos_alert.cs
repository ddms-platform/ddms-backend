using System;

namespace DDMS.Backend.Models.Entities;

public partial class sos_alert
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public Guid? boat_id { get; set; }
    public decimal latitude { get; set; }
    public decimal longitude { get; set; }
    public string status { get; set; } = "ACTIVE"; // ACTIVE, RESOLVED, CANCELLED
    public string? note { get; set; }
    public DateTime created_at { get; set; } = DateTime.UtcNow;
    public DateTime? resolved_at { get; set; }
    public Guid? resolved_by { get; set; }

    public virtual user? user { get; set; }
    public virtual boat? boat { get; set; }
}
