using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class ai_message
{
    public Guid id { get; set; }

    public Guid ai_conversation_id { get; set; }

    public string role { get; set; } = null!;

    public string content { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual ai_conversation ai_conversation { get; set; } = null!;
}
