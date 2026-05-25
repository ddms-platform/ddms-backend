using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class tour_schedule
{
    public Guid id { get; set; }

    public Guid tour_id { get; set; }

    public Guid? boat_id { get; set; }

    public Guid? dock_id { get; set; }

    public DateTime start_time { get; set; }

    public DateTime end_time { get; set; }

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual boat? boat { get; set; }

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    public virtual ICollection<conversation> conversations { get; set; } = new List<conversation>();

    public virtual dock? dock { get; set; }

    public virtual ICollection<dock_schedule> dock_schedules { get; set; } = new List<dock_schedule>();

    public virtual tour tour { get; set; } = null!;
}
