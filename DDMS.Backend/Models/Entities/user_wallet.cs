using System;

namespace DDMS.Backend.Models.Entities;

public partial class user_wallet
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public decimal balance { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual user user { get; set; } = null!;
}
