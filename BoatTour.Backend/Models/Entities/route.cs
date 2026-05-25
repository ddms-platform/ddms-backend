using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class route
{
    public Guid id { get; set; }

    public Guid tour_id { get; set; }

    public string? name { get; set; }

    public string start_point { get; set; } = null!;

    public string end_point { get; set; } = null!;

    public string? description { get; set; }

    public int sort_order { get; set; }

    public DateTime created_at { get; set; }

    public virtual tour tour { get; set; } = null!;
}
