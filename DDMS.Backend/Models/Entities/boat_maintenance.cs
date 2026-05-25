using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class boat_maintenance
{
    public Guid id { get; set; }

    public Guid boat_id { get; set; }

    public DateTime start_time { get; set; }

    public DateTime end_time { get; set; }

    public string? reason { get; set; }

    public DateTime created_at { get; set; }

    public virtual boat boat { get; set; } = null!;
}
