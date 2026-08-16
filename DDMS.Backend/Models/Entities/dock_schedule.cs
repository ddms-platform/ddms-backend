using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class dock_schedule
{
    public Guid id { get; set; }

    public Guid dock_id { get; set; }

    public Guid boat_id { get; set; }

    public Guid? schedule_id { get; set; }

    /// <summary>
    /// Khoang neo cang vu gan cho tau (A1..A16, B1..B16). NULL = chua gan.
    /// Truoc day khong luu o dau ca nen moi man tu suy ra tu vi tri trong mang,
    /// dan den admin thay A12 con owner thay A1 cho cung mot con tau.
    /// </summary>
    public string? berth_code { get; set; }

    public DateTime start_time { get; set; }

    public DateTime end_time { get; set; }

    public DateTime created_at { get; set; }

    public virtual boat boat { get; set; } = null!;

    public virtual dock dock { get; set; } = null!;

    public virtual tour_schedule? schedule { get; set; }
}
