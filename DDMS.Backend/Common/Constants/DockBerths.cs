namespace DDMS.Backend.Common.Constants;

/// <summary>
/// Khoang neo đậu của bến: A1..A16 trên cầu tàu A, B1..B16 trên cầu tàu B.
///
/// Thứ tự trong <see cref="Ordered"/> phải khớp với thứ tự vẽ ở frontend — hai
/// hàng của mỗi cầu tàu xen kẽ nhau (A1, A9, A2, A10, ...), chứ không phải tuần
/// tự. Bến chỉ mở số khoang bằng <c>dock.max_boats</c> đầu tiên của danh sách
/// này, nên thứ tự quyết định khoang nào dùng được.
/// </summary>
public static class DockBerths
{
    private const int PerRow = 8;

    /// <summary>Toàn bộ khoang, đúng thứ tự mở dần theo sức chứa của bến.</summary>
    public static readonly IReadOnlyList<string> Ordered = Build();

    private static string[] Build()
    {
        var list = new List<string>(PerRow * 4);
        foreach (var pier in new[] { "A", "B" })
        {
            for (var i = 0; i < PerRow; i++)
            {
                list.Add($"{pier}{i + 1}");
                list.Add($"{pier}{i + PerRow + 1}");
            }
        }
        return [.. list];
    }

    /// <summary>Khoang có tồn tại trên sơ đồ bến hay không.</summary>
    public static bool IsKnown(string? code) =>
        !string.IsNullOrWhiteSpace(code) && Ordered.Contains(Normalize(code));

    /// <summary>
    /// Khoang có nằm trong sức chứa của bến hay không. Bến 10 chỗ thì chỉ
    /// 10 khoang đầu của <see cref="Ordered"/> dùng được.
    /// </summary>
    public static bool IsWithinCapacity(string? code, int maxBoats)
    {
        if (!IsKnown(code)) return false;
        var index = Ordered.ToList().IndexOf(Normalize(code)!);
        return index >= 0 && index < maxBoats;
    }

    /// <summary>Chuẩn hoá về chữ hoa, bỏ khoảng trắng. Null nếu rỗng.</summary>
    public static string? Normalize(string? code)
    {
        var trimmed = code?.Trim().ToUpperInvariant();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
