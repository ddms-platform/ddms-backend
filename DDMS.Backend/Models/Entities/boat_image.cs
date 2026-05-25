using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class boat_image
{
    public Guid id { get; set; }

    public Guid boat_id { get; set; }

    public string image_url { get; set; } = null!;

    public string? public_id { get; set; }

    public string? caption { get; set; }

    public int sort_order { get; set; }

    public DateTime created_at { get; set; }

    public virtual boat boat { get; set; } = null!;
}
