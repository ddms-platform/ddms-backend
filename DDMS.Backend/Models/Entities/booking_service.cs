using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class booking_service
{
    public Guid id { get; set; }

    public Guid booking_id { get; set; }

    public Guid service_id { get; set; }

    public int quantity { get; set; }

    public decimal unit_price { get; set; }

    public DateTime created_at { get; set; }

    public virtual booking booking { get; set; } = null!;

    public virtual boat_service service { get; set; } = null!;
}
