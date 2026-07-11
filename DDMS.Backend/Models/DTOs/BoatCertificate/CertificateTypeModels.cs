namespace DDMS.Backend.Models.DTOs.BoatCertificate;

public class CertificateTypeItem
{
    public int id { get; set; }
    public string code { get; set; } = null!;
    public string nameVi { get; set; } = null!;
    public string nameEn { get; set; } = null!;
    public string scope { get; set; } = "boat";
    public int sortOrder { get; set; }
    public bool isActive { get; set; }
}

public class CreateCertificateTypeRequest
{
    public string code { get; set; } = null!;
    public string nameVi { get; set; } = null!;
    public string nameEn { get; set; } = null!;
    public string scope { get; set; } = "boat";
    public int? sortOrder { get; set; }
    public bool isActive { get; set; } = true;
}

public class UpdateCertificateTypeRequest
{
    public string nameVi { get; set; } = null!;
    public string nameEn { get; set; } = null!;
    public string? scope { get; set; }
    public int sortOrder { get; set; }
    public bool isActive { get; set; } = true;
}
