using System;

namespace DDMS.Backend.Models.Entities;

public partial class wallet_withdrawal
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public decimal amount { get; set; }

    public string bank_name { get; set; } = null!;

    public string account_number { get; set; } = null!;

    public string account_name { get; set; } = null!;

    public string status { get; set; } = null!; // pending, approved, rejected

    public DateTime created_at { get; set; }

    public DateTime? processed_at { get; set; }

    public virtual user user { get; set; } = null!;
}
