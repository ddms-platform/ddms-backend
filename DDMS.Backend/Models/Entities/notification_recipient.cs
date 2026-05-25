using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class notification_recipient
{
    public Guid id { get; set; }

    public Guid notification_id { get; set; }

    public Guid user_id { get; set; }

    public bool is_read { get; set; }

    public DateTime? read_at { get; set; }

    public DateTime created_at { get; set; }

    public virtual notification notification { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
