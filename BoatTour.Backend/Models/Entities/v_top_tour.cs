using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class v_top_tour
{
    public Guid id { get; set; }

    public string name { get; set; } = null!;

    public decimal avg_rating { get; set; }

    public long total_bookings { get; set; }

    public decimal total_revenue { get; set; }
}
