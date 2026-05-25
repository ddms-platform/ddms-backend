using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class dock
{
    public Guid id { get; set; }

    public string name { get; set; } = null!;

    public string? location { get; set; }

    public int max_boats { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<dock_schedule> dock_schedules { get; set; } = new List<dock_schedule>();

    public virtual ICollection<tour_schedule> tour_schedules { get; set; } = new List<tour_schedule>();
}
