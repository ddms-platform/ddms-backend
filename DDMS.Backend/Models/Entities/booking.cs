using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class booking
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public Guid schedule_id { get; set; }

    public Guid? promotion_id { get; set; }

    public int num_people { get; set; }

    public decimal base_price { get; set; }

    public decimal cabin_price { get; set; }

    public decimal service_price { get; set; }

    public decimal discount_amount { get; set; }

    public decimal total_price { get; set; }

    public string status { get; set; } = null!;

    public string? notes { get; set; }

    public DateTime? cancelled_at { get; set; }

    public string? cancel_reason { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<booking_cabin> booking_cabins { get; set; } = new List<booking_cabin>();

    public virtual ICollection<booking_service> booking_services { get; set; } = new List<booking_service>();

    public virtual ICollection<conversation> conversations { get; set; } = new List<conversation>();

    public virtual ICollection<loyalty_point> loyalty_points { get; set; } = new List<loyalty_point>();

    public virtual payment? payment { get; set; }

    public virtual promotion? promotion { get; set; }

    public virtual review? review { get; set; }

    public virtual tour_schedule schedule { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
