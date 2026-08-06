namespace DDMS.Backend.Shared.Constants;

/// <summary>
/// Fixed GUID pool dùng chung giữa test code (C#) và test data (JSON), để cả hai
/// phía tham chiếu cùng 1 giá trị mà không cần Guid.NewGuid() (không thể biểu diễn
/// động trong JSON).
/// </summary>
public static class TestGuids
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid BookingId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ScheduleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid TourId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid BoatId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid CabinId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid PromotionId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public static readonly Guid RefreshTokenId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
}
