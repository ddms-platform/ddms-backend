using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class review
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public Guid booking_id { get; set; }

    public Guid tour_id { get; set; }

    public sbyte rating { get; set; }

    public string? comment { get; set; }

    public string? image_urls { get; set; }

    public string? video_urls { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual booking booking { get; set; } = null!;

    public virtual tour tour { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
