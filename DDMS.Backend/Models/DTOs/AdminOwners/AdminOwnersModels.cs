namespace DDMS.Backend.Models.DTOs.AdminOwners;

public class VerificationItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Owner { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string License { get; set; } = null!;
    public string Submitted { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int Boats { get; set; }
    public List<VesselItem> Vessels { get; set; } = new();
}

public class VesselItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal? Length { get; set; }
    public decimal? Beam { get; set; }
    public string RegistrationNumber { get; set; } = null!;
    public string MooringType { get; set; } = null!;
    public string ExpectedDockingDate { get; set; } = null!;
    public List<string> RequiredServices { get; set; } = new();
    public List<string> DocumentUrls { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public string Status { get; set; } = null!;
}

