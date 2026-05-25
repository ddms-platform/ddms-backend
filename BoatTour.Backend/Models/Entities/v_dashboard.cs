using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class v_dashboard
{
    public long? total_completed_bookings { get; set; }

    public long? pending_bookings { get; set; }

    public decimal? total_revenue { get; set; }

    public long? total_customers { get; set; }

    public long? active_tours { get; set; }

    public long? idle_boats { get; set; }
}
