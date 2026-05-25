using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class faq
{
    public Guid id { get; set; }

    public Guid tour_id { get; set; }

    public string question { get; set; } = null!;

    public string answer { get; set; } = null!;

    public int sort_order { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual tour tour { get; set; } = null!;
}
