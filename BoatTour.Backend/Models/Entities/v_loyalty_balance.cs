using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class v_loyalty_balance
{
    public Guid user_id { get; set; }

    public decimal? total_points { get; set; }
}
