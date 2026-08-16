using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class boat_cabin
{
    public Guid id { get; set; }

    public Guid boat_id { get; set; }

    /// <summary>
    /// Phòng/combo thuộc về tour nào. NULL = dùng chung cho cả con thuyền,
    /// giữ nguyên hành vi cũ cho dữ liệu đã có trước khi tách theo tour.
    /// </summary>
    public Guid? tour_id { get; set; }

    public string name { get; set; } = null!;

    public int capacity { get; set; }

    public decimal price { get; set; }

    public int total_rooms { get; set; }

    public string? description { get; set; }

    public string? image_url { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual boat boat { get; set; } = null!;

    public virtual ICollection<booking_cabin> booking_cabins { get; set; } = new List<booking_cabin>();
}
