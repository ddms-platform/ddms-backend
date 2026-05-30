namespace DDMS.Backend.Models.Entities;

public partial class email_verification_token
{
    public Guid id { get; set; }

    public string email { get; set; } = null!;

    public string token_hash { get; set; } = null!;

    public string purpose { get; set; } = null!;

    public DateTime expires_at { get; set; }

    public DateTime? used_at { get; set; }

    public DateTime created_at { get; set; }
}
