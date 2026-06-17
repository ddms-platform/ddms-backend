using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class tour
{
    public Guid id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public decimal price { get; set; }

    public int duration_minutes { get; set; }

    public string? location { get; set; }
    
    public string? map_url { get; set; }

    public decimal avg_rating { get; set; }

    public int total_reviews { get; set; }

    public string status { get; set; } = null!;

    public string cancel_policy { get; set; } = null!;

    public int? cancel_hours { get; set; }

    public Guid? created_by { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual user? created_byNavigation { get; set; }

    public virtual ICollection<faq> faqs { get; set; } = new List<faq>();

    public virtual ICollection<review> reviews { get; set; } = new List<review>();

    public virtual ICollection<route> routes { get; set; } = new List<route>();

    public virtual ICollection<tour_image> tour_images { get; set; } = new List<tour_image>();

    public virtual ICollection<tour_schedule> tour_schedules { get; set; } = new List<tour_schedule>();
}
