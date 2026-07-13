using System;

namespace DDMS.Backend.Models.Entities;

public partial class boat_certificate
{
    public Guid id { get; set; }

    public Guid boat_id { get; set; }

    public string certificate_type { get; set; } = null!;

    public string document_url { get; set; } = null!;

    public string? public_id { get; set; }

    public DateOnly expiry_date { get; set; }

    public string status { get; set; } = null!;

    public string? rejection_reason { get; set; }

    public Guid? verified_by { get; set; }

    public DateTime? verified_at { get; set; }

    public DateTime? reminder_sent_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual boat boat { get; set; } = null!;

    public virtual user? verifier { get; set; }
}
