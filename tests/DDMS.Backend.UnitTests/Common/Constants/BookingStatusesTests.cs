using DDMS.Backend.Common.Constants;
using FluentAssertions;

namespace DDMS.Backend.UnitTests.Common.Constants;

/// <summary>
/// "Chiếm chỗ" quyết định một đơn có bị trừ vào phòng/ghế còn trống hay không.
/// Sót một trạng thái ở đây là bán trùng chỗ ngoài đời thật, nên khoá lại bằng test.
/// </summary>
public class BookingStatusesTests
{
    [Theory]
    [InlineData(BookingStatuses.Pending)]
    [InlineData(BookingStatuses.Confirmed)]
    [InlineData(BookingStatuses.Paid)]
    // Giữ chỗ phải chiếm chỗ, nếu không thì "giữ chỗ" chẳng giữ được gì.
    [InlineData(BookingStatuses.Holding)]
    // Khách đã lên tàu vẫn đang ngồi trên tàu — chỗ đó không được bán lại.
    [InlineData(BookingStatuses.CheckedIn)]
    public void OccupiesInventory_DungVoiDonDangChiemCho(string status) =>
        BookingStatuses.OccupiesInventory(status).Should().BeTrue();

    [Theory]
    [InlineData(BookingStatuses.Cancelled)]
    [InlineData(BookingStatuses.Completed)]
    public void OccupiesInventory_SaiVoiDonDaKetThuc(string status) =>
        BookingStatuses.OccupiesInventory(status).Should().BeFalse();

    [Fact]
    public void OccupyingStatuses_KhopVoiOccupiesInventory()
    {
        // Repository lọc bằng danh sách (EF dịch được sang SQL), service dùng hàm.
        // Hai đường phải luôn nói cùng một chuyện.
        var all = new[]
        {
            BookingStatuses.Pending, BookingStatuses.Holding, BookingStatuses.Confirmed,
            BookingStatuses.Paid, BookingStatuses.CheckedIn, BookingStatuses.Completed,
            BookingStatuses.Cancelled,
        };

        foreach (var status in all)
            BookingStatuses.OccupyingStatuses.Contains(status)
                .Should().Be(BookingStatuses.OccupiesInventory(status), $"trạng thái '{status}'");
    }
}
