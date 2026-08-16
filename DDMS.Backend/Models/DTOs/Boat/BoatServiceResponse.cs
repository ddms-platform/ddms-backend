namespace DDMS.Backend.Models.DTOs.Boat;

public class BoatServiceResponse
{
    public Guid id { get; init; }
    public Guid boatId { get; init; }
    public string name { get; init; } = null!;
    public decimal price { get; init; }
    public string? description { get; init; }
    public string? imageUrl { get; init; }
    public List<string> imageUrls { get; init; } = [];
    public string? serviceType { get; init; }
    public bool isActive { get; init; }
    public DateTime createdAt { get; init; }
    public DateTime updatedAt { get; init; }

    // Nested collections (tour has routes/faqs; boat has cabins/combos — shared across tours on same boat)
    public List<ServiceRouteItem> routes { get; init; } = [];
    public List<ServiceFaqItem> faqs { get; init; } = [];
    public List<ServiceRoomItem> rooms { get; init; } = [];
    public List<ServiceComboItem> combos { get; init; } = [];
}

public class ServiceRouteItem
{
    public Guid id { get; init; }
    public string name { get; init; } = string.Empty;
    public string? startPoint { get; init; }
    public string? endPoint { get; init; }
    public string? description { get; init; }
}

public class ServiceFaqItem
{
    public Guid id { get; init; }
    public string question { get; init; } = string.Empty;
    public string answer { get; init; } = string.Empty;
}

public class ServiceRoomItem
{
    public Guid id { get; init; }
    public string name { get; init; } = string.Empty;
    public int capacity { get; init; }
    public decimal price { get; init; }
    public string? description { get; init; }
    public string? imageUrl { get; init; }
}

public class ServiceComboItem
{
    public Guid id { get; init; }
    public string name { get; init; } = string.Empty;
    public decimal price { get; init; }
    public string? description { get; init; }
    public string? imageUrl { get; init; }
}
