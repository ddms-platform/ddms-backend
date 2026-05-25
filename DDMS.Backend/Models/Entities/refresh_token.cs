using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class refresh_token
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public string token_hash { get; set; } = null!;

    public DateTime expires_at { get; set; }

    public bool revoked { get; set; }

    public DateTime? revoked_at { get; set; }

    public string? user_agent { get; set; }

    public string? ip_address { get; set; }

    public DateTime created_at { get; set; }

    public virtual user user { get; set; } = null!;
}
