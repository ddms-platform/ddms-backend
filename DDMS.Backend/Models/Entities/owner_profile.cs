using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class owner_profile
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public string? business_name { get; set; }

    public string? bio { get; set; }

    public string? license_number { get; set; }

    public string? license_image { get; set; }

    public string? phone_business { get; set; }

    public string? address { get; set; }

    public bool is_verified { get; set; }

    public string? status { get; set; }

    public DateTime? verified_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual user user { get; set; } = null!;
}
