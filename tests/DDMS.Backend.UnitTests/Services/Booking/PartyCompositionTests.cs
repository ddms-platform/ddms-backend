using DDMS.Backend.Models.DTOs.Booking;
using FluentAssertions;

namespace DDMS.Backend.UnitTests.Services.Booking;

/// <summary>
/// Chuẩn hoá thành phần đoàn khách. Đây là chỗ duy nhất quyết định "đơn này có
/// bao nhiêu người, thuộc hạng nào", nên client cũ lẫn client mới đều phải đi qua đây.
/// </summary>
public class PartyCompositionTests
{
    [Fact]
    public void ClientCu_KhongGuiHangVe_ThiCoiTatCaLaNguoiLon()
    {
        var party = PartyComposition.FromRequest(new CreateBookingRequest { NumPeople = 3 });

        party.Adults.Should().Be(3);
        party.Children.Should().Be(0);
        party.Infants.Should().Be(0);
        party.Total.Should().Be(3);
    }

    [Fact]
    public void CoHangVe_ThiTinhLaiTongChuKhongTinSoClientGui()
    {
        // Client khai NumPeople = 99 nhưng chỉ liệt kê 4 khách. Server không tin số 99.
        var party = PartyComposition.FromRequest(new CreateBookingRequest
        {
            NumPeople = 99,
            NumAdults = 2,
            NumChildren = 1,
            NumInfants = 1,
        });

        party.Adults.Should().Be(2);
        party.Children.Should().Be(1);
        party.Infants.Should().Be(1);
        party.Total.Should().Be(4);
    }

    [Fact]
    public void ChiCoTreEm_VanHopLe()
    {
        var party = PartyComposition.FromRequest(new CreateBookingRequest { NumChildren = 2 });

        party.Adults.Should().Be(0);
        party.Total.Should().Be(2);
    }

    [Fact]
    public void DonCuTrongDb_ChuaCoHangVe_ThiDocTheoNumPeople()
    {
        // Đơn đặt trước khi có tính năng này: num_adults/children/infants đều = 0
        // nhưng num_people > 0. Áp mã giảm giá lên đơn đó không được vỡ.
        var party = PartyComposition.FromCounts(numPeople: 4, adults: 0, children: 0, infants: 0);

        party.Adults.Should().Be(4);
        party.Total.Should().Be(4);
    }

    [Fact]
    public void DonMoiTrongDb_ThiDocTheoHangVe()
    {
        var party = PartyComposition.FromCounts(numPeople: 4, adults: 2, children: 2, infants: 0);

        party.Adults.Should().Be(2);
        party.Children.Should().Be(2);
        party.Total.Should().Be(4);
    }

    [Fact]
    public void SoAm_BiKepVeKhong()
    {
        var party = PartyComposition.FromRequest(new CreateBookingRequest
        {
            NumAdults = 2,
            NumChildren = -5,
        });

        party.Children.Should().Be(0);
        party.Total.Should().Be(2);
    }
}
