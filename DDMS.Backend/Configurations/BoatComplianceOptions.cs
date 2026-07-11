using System.ComponentModel.DataAnnotations;

namespace DDMS.Backend.Configurations;

public class BoatComplianceOptions
{
    public const string SectionName = "BoatCompliance";

    [Range(1, 168, ErrorMessage = "BoatCompliance:CheckIntervalHours phải nằm trong [1, 168].")]
    public int CheckIntervalHours { get; set; } = 6;

    [Range(1, 365, ErrorMessage = "BoatCompliance:ReminderDaysBeforeExpiry phải nằm trong [1, 365].")]
    public int ReminderDaysBeforeExpiry { get; set; } = 180;

    [Range(0, 90, ErrorMessage = "BoatCompliance:GracePeriodDays phải nằm trong [0, 90].")]
    public int GracePeriodDays { get; set; } = 7;

    [Required(AllowEmptyStrings = false, ErrorMessage = "BoatCompliance:TimeZoneId là bắt buộc.")]
    public string TimeZoneId { get; set; } = "SE Asia Standard Time";
}
