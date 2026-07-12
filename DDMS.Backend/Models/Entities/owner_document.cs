using System;

namespace DDMS.Backend.Models.Entities;

public partial class owner_document
{
    public Guid id { get; set; }

    public Guid owner_profile_id { get; set; }

    public string document_type { get; set; } = null!;

    public string document_url { get; set; } = null!;

    public string? public_id { get; set; }

    public DateOnly? expiry_date { get; set; }

    public string? admin_note { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual owner_profile owner_profile { get; set; } = null!;
}
