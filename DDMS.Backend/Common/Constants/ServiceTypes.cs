namespace DDMS.Backend.Common.Constants;

/// <summary>
/// Loại dịch vụ chủ thuyền chọn khi đăng ký. Quyết định form hiển thị field nào
/// (phòng, combo, dụng cụ câu, giá nguyên ngày), nên lưu sai là form hiện sai.
/// </summary>
public static class ServiceTypes
{
    public const string Cruise = "cruise";
    public const string Dinner = "dinner";
    public const string Fishing = "fishing";
    public const string Speedboat = "speedboat";
    public const string ComplexTour = "complex_tour";

    private static readonly HashSet<string> All =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Cruise, Dinner, Fishing, Speedboat, ComplexTour,
        };

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && All.Contains(value.Trim());
}
