namespace DDMS.Backend.Models.DTOs.OwnerServices;

public class DynamicServiceRequest
{
    public Guid? id { get; set; }
    public Guid boatId { get; set; }
    public string serviceType { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public decimal basePrice { get; set; }
    public string? description { get; set; }
    public string? route { get; set; }
    public List<ServiceRoute>? routes { get; set; }
    public List<ServiceCombo>? combos { get; set; }
    public List<ServiceRoom>? rooms { get; set; }
    public List<ServiceFaq>? faqs { get; set; }
    public string? equipments { get; set; }
    public decimal? pricePerDay { get; set; }
}

public class ServiceCombo
{
    public string name { get; set; } = string.Empty;
    public decimal price { get; set; }
    public string? description { get; set; }
    public string? imageUrl { get; set; }
}

public class ServiceRoom
{
    public string name { get; set; } = string.Empty;
    public int capacity { get; set; }
    public decimal? price { get; set; }
    public string? description { get; set; }
    public string? imageUrl { get; set; }
}

public class ServiceRoute
{
    public string name { get; set; } = string.Empty;
    public string startPoint { get; set; } = string.Empty;
    public string endPoint { get; set; } = string.Empty;
    public string? description { get; set; }
}

public class ServiceFaq
{
    public string question { get; set; } = string.Empty;
    public string answer { get; set; } = string.Empty;
}
