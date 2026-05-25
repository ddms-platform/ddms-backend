using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class notification
{
    public Guid id { get; set; }

    public string type { get; set; } = null!;

    public Guid? sender_id { get; set; }

    public string title { get; set; } = null!;

    public string body { get; set; } = null!;

    public string? data { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<notification_recipient> notification_recipients { get; set; } = new List<notification_recipient>();

    public virtual user? sender { get; set; }
}
