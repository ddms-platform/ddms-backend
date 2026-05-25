using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class promotion
{
    public Guid id { get; set; }

    public string code { get; set; } = null!;

    public string? description { get; set; }

    public string discount_type { get; set; } = null!;

    public decimal discount_value { get; set; }

    public decimal min_order_value { get; set; }

    public decimal? max_discount { get; set; }

    public int? usage_limit { get; set; }

    public int used_count { get; set; }

    public DateTime valid_from { get; set; }

    public DateTime? valid_until { get; set; }

    public bool? is_active { get; set; }

    public Guid? created_by { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<booking> bookings { get; set; } = new List<booking>();

    public virtual user? created_byNavigation { get; set; }
}
