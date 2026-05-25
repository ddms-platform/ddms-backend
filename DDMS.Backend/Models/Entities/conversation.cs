using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class conversation
{
    public Guid id { get; set; }

    public string type { get; set; } = null!;

    public string? name { get; set; }

    public Guid? booking_id { get; set; }

    public Guid? schedule_id { get; set; }

    public Guid created_by { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual booking? booking { get; set; }

    public virtual ICollection<conversation_member> conversation_members { get; set; } = new List<conversation_member>();

    public virtual user created_byNavigation { get; set; } = null!;

    public virtual ICollection<message> messages { get; set; } = new List<message>();

    public virtual tour_schedule? schedule { get; set; }
}
