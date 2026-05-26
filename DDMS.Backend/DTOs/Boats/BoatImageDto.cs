namespace DDMS.Backend.DTOs.Boats;

public class BoatImageDto
{
    public Guid Id { get; set; }
    public Guid BoatId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string? PublicId { get; set; }
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
}
