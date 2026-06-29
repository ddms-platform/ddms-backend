using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class wishlist
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public Guid tour_id { get; set; }

    public DateTime created_at { get; set; }

    public virtual tour tour { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
