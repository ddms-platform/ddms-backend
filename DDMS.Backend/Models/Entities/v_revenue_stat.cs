using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class v_revenue_stat
{
    public DateOnly? month { get; set; }

    public long total_payments { get; set; }

    public decimal? total_revenue { get; set; }
}
