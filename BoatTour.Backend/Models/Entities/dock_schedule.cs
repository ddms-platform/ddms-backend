using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class dock_schedule
{
    public Guid id { get; set; }

    public Guid dock_id { get; set; }

    public Guid boat_id { get; set; }

    public Guid? schedule_id { get; set; }

    public DateTime start_time { get; set; }

    public DateTime end_time { get; set; }

    public DateTime created_at { get; set; }

    public virtual boat boat { get; set; } = null!;

    public virtual dock dock { get; set; } = null!;

    public virtual tour_schedule? schedule { get; set; }
}
