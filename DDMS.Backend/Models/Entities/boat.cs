using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class boat
{
    public Guid id { get; set; }

    public Guid? owner_id { get; set; }

    public string name { get; set; } = null!;

    public string? type { get; set; }

    public int max_passengers { get; set; }

    public string status { get; set; } = null!;

    public decimal? length { get; set; }

    public decimal? beam { get; set; }

    public string? registration_number { get; set; }

    public string? mooring_type { get; set; }

    public DateTime? expected_docking_date { get; set; }

    public string? required_services { get; set; }

    public string? document_url { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual user? owner { get; set; }

    public virtual ICollection<boat_cabin> boat_cabins { get; set; } = new List<boat_cabin>();

    public virtual ICollection<boat_image> boat_images { get; set; } = new List<boat_image>();

    public virtual ICollection<boat_maintenance> boat_maintenances { get; set; } = new List<boat_maintenance>();

    public virtual ICollection<boat_service> boat_services { get; set; } = new List<boat_service>();

    public virtual ICollection<dock_schedule> dock_schedules { get; set; } = new List<dock_schedule>();

    public virtual ICollection<tour_schedule> tour_schedules { get; set; } = new List<tour_schedule>();

    public virtual ICollection<wishlist> wishlists { get; set; } = new List<wishlist>();

    public bool is_deleted { get; set; } = false;
}
