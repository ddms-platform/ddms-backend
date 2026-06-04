namespace DDMS.Backend.Configurations;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string fromAddress { get; set; } = "noreply@ddms.local";
    public string fromName { get; set; } = "DDMS";
    public bool useSmtp { get; set; }
    public string smtpHost { get; set; } = string.Empty;
    public int smtpPort { get; set; } = 587;
    public string smtpUser { get; set; } = string.Empty;
    public string smtpPassword { get; set; } = string.Empty;
    public bool enableSsl { get; set; } = true;
}
