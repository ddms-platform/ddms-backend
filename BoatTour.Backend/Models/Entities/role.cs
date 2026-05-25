using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class role
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<user_role> user_roles { get; set; } = new List<user_role>();
}
