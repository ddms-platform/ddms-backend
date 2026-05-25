using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class conversation_member
{
    public Guid id { get; set; }

    public Guid conversation_id { get; set; }

    public Guid user_id { get; set; }

    public DateTime joined_at { get; set; }

    public DateTime? last_read_at { get; set; }

    public virtual conversation conversation { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
