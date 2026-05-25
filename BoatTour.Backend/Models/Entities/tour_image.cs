using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class tour_image
{
    public Guid id { get; set; }

    public Guid tour_id { get; set; }

    public string image_url { get; set; } = null!;

    public string? public_id { get; set; }

    public string? caption { get; set; }

    public int sort_order { get; set; }

    public DateTime created_at { get; set; }

    public virtual tour tour { get; set; } = null!;
}
