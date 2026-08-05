using System;

namespace DDMS.Backend.Models.DTOs.Sos;

public class TriggerSosRequest
{
    public Guid? boat_id { get; set; }
    public decimal latitude { get; set; }
    public decimal longitude { get; set; }
    public string? note { get; set; }
}

public class SosAlertResponse
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public string? user_name { get; set; }
    public string? user_phone { get; set; }
    public Guid? boat_id { get; set; }
    public string? boat_name { get; set; }
    public string? registration_number { get; set; }
    public decimal latitude { get; set; }
    public decimal longitude { get; set; }
    public string status { get; set; } = "ACTIVE";
    public string? note { get; set; }
    public DateTime created_at { get; set; }
    public DateTime? resolved_at { get; set; }
}

public class ResolveSosRequest
{
    public string? note { get; set; }
}
