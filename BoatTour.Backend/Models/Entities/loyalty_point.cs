using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class loyalty_point
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public Guid? booking_id { get; set; }

    public int points { get; set; }

    public string type { get; set; } = null!;

    public string? note { get; set; }

    public DateTime created_at { get; set; }

    public virtual booking? booking { get; set; }

    public virtual user user { get; set; } = null!;
}
