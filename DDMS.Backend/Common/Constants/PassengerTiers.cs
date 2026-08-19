namespace DDMS.Backend.Common.Constants;

/// <summary>
/// Hạng vé theo độ tuổi. Khoảng tuổi cố định toàn hệ thống — ta không thu thập
/// tuổi từng khách nên đây chỉ là nhãn để khách tự khai; cho owner tuỳ chỉnh
/// khoảng tuổi sẽ chẳng kiểm chứng được bằng gì. Cái owner tuỳ chỉnh được là
/// tỉ lệ giá, lưu trên từng tour.
/// </summary>
public static class PassengerTiers
{
    public const string Adult = "adult";
    public const string Child = "child";
    public const string Infant = "infant";

    /// <summary>Tuổi nhỏ nhất để tính là người lớn.</summary>
    public const int AdultMinAge = 12;

    /// <summary>Tuổi nhỏ nhất để tính là trẻ em; dưới mức này là em bé.</summary>
    public const int ChildMinAge = 5;

    /// <summary>Người lớn luôn trả đủ giá tour — không có cột nào để đổi.</summary>
    public const decimal AdultPricePercent = 100m;

    public const decimal DefaultChildPricePercent = 50m;
    public const decimal DefaultInfantPricePercent = 0m;
}
