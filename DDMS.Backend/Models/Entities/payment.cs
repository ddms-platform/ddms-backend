using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class payment
{
    public Guid id { get; set; }

    public Guid booking_id { get; set; }

    public decimal amount { get; set; }

    public string method { get; set; } = null!;

    public string status { get; set; } = null!;

    public string? transaction_ref { get; set; }

    public string? gateway_response { get; set; }

    public DateTime? paid_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual booking booking { get; set; } = null!;
}
