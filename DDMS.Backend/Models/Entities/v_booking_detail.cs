using System;
using System.Collections.Generic;

namespace DDMS.Backend.Models.Entities;

public partial class v_booking_detail
{
    public Guid booking_id { get; set; }

    public string customer_name { get; set; } = null!;

    public string customer_email { get; set; } = null!;

    public string tour_name { get; set; } = null!;

    public string? boat_name { get; set; }

    public DateTime start_time { get; set; }

    public DateTime end_time { get; set; }

    public int num_people { get; set; }

    public decimal base_price { get; set; }

    public decimal cabin_price { get; set; }

    public decimal service_price { get; set; }

    public decimal discount_amount { get; set; }

    public decimal total_price { get; set; }

    public string booking_status { get; set; } = null!;

    public string? payment_status { get; set; }

    public string? payment_method { get; set; }
}
