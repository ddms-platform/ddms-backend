using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class message
{
    public Guid id { get; set; }

    public Guid conversation_id { get; set; }

    public Guid sender_id { get; set; }

    public string body { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual conversation conversation { get; set; } = null!;

    public virtual user sender { get; set; } = null!;
}
