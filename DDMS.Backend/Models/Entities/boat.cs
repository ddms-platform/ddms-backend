using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class boat
{
    public Guid id { get; set; }

    public string name { get; set; } = null!;

    public string? type { get; set; }

    public int max_passengers { get; set; }

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<boat_cabin> boat_cabins { get; set; } = new List<boat_cabin>();

    public virtual ICollection<boat_image> boat_images { get; set; } = new List<boat_image>();

    public virtual ICollection<boat_maintenance> boat_maintenances { get; set; } = new List<boat_maintenance>();

    public virtual ICollection<boat_service> boat_services { get; set; } = new List<boat_service>();

    public virtual ICollection<dock_schedule> dock_schedules { get; set; } = new List<dock_schedule>();

    public virtual ICollection<tour_schedule> tour_schedules { get; set; } = new List<tour_schedule>();

    public virtual ICollection<wishlist> wishlists { get; set; } = new List<wishlist>();
}
