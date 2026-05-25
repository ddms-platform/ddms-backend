using System;
using System.Collections.Generic;

namespace BoatTour.Backend.Models.Entities;

public partial class v_unread_notification
{
    public Guid user_id { get; set; }

    public long unread_count { get; set; }
}
