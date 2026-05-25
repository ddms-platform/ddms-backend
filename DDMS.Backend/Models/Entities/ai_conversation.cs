using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class ai_conversation
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public string? title { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<ai_message> ai_messages { get; set; } = new List<ai_message>();

    public virtual user user { get; set; } = null!;
}
