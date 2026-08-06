using System.Text.Json;

namespace DDMS.Backend.Shared.DataProviders;

/// <summary>
/// Đọc dữ liệu test (input/expected) từ file JSON trong thư mục TestData/ của
/// DDMS.Backend.UnitTests. Dùng làm nguồn cho xUnit [Theory] + [MemberData].
///
/// Quy ước: mỗi case trong JSON tương ứng 1 lớp tương đương (Equivalence Partitioning),
/// KHÔNG chứa hành vi mock của repository/service (việc đó luôn nằm trong C#, xem Shared/Mocks
/// và Shared/Builders) — JSON chỉ mang input đơn giản + kỳ vọng đầu ra.
/// </summary>
public static class JsonDataProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Load và deserialize 1 file JSON (mảng object) thành List&lt;T&gt;.
    /// relativePath tính từ thư mục output của test assembly, ví dụ:
    /// "TestData/Booking/BookingService/Cancel.json".
    /// </summary>
    public static List<T> Load<T>(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"Không tìm thấy test data '{relativePath}'. " +
                "Kiểm tra file .json đã được copy vào output directory (CopyToOutputDirectory) chưa.",
                fullPath);
        }

        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<List<T>>(json, SerializerOptions)
            ?? throw new InvalidOperationException($"Không thể deserialize test data từ '{relativePath}'.");
    }

    /// <summary>Tiện ích để dùng trực tiếp làm nguồn cho [Theory] + [MemberData].</summary>
    public static IEnumerable<object[]> LoadAsTheoryData<T>(string relativePath) =>
        Load<T>(relativePath).Select(item => new object[] { item! });

    private static string ResolvePath(string relativePath) =>
        Path.Combine(AppContext.BaseDirectory, relativePath);
}
