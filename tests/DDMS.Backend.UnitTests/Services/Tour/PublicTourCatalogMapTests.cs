using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Implementations;
using DDMS.Backend.Shared.Builders.EntityBuilders;
using DDMS.Backend.Shared.Constants;
using FluentAssertions;
using Moq;

namespace DDMS.Backend.UnitTests.Services.Tour;

/// <summary>
/// Trang chi tiết tour công khai (GET /api/public/tours/{id}) phải trả số khách tối đa
/// và thuyền chạy tour. Trước đây response không có hai field này nên FE hardcode
/// "8 people" và hiện "Boat: N/A".
/// </summary>
public class PublicTourCatalogMapTests
{
    private readonly Mock<IOwnerToursRepository> _tourRepo = new();
    private readonly Mock<ITourImageRepository> _imageRepo = new();
    private readonly Mock<IFaqRepository> _faqRepo = new();

    private PublicTourCatalogService CreateSut() =>
        new(_tourRepo.Object, _imageRepo.Object, _faqRepo.Object);

    /// <summary>
    /// Dựng tour active kèm 1 lịch trình. <paramref name="departingInDays"/> âm = lịch đã qua,
    /// tức không còn lịch "active" nào.
    /// </summary>
    private DDMS.Backend.Models.Entities.tour ArrangeTour(
        int? maxGuests,
        boat? boatEntity,
        double departingInDays = 10,
        string scheduleStatus = TourConstants.ScheduleStatuses.Scheduled)
    {
        var entity = new TourBuilder()
            .WithMaxGuests(maxGuests)
            .WithStatus(TourStatuses.Active)
            .Build();

        var scheduleBuilder = new TourScheduleBuilder()
            .WithTour(entity)
            .WithStatus(scheduleStatus)
            .DepartingInDays(departingInDays);

        scheduleBuilder = boatEntity is null
            ? scheduleBuilder.WithNoBoat()
            : scheduleBuilder.WithBoat(boatEntity);

        entity.tour_schedules = new List<tour_schedule> { scheduleBuilder.Build() };

        _tourRepo.Setup(r => r.GetActiveByIdAsync(entity.id)).ReturnsAsync(entity);
        return entity;
    }

    [Fact]
    public async Task GetActiveTourAsync_TraSoKhachToiDaVaThuyen()
    {
        var boatEntity = new BoatBuilder().WithName("Rồng Vàng").Build();
        var entity = ArrangeTour(maxGuests: 8, boatEntity: boatEntity);

        var result = await CreateSut().GetActiveTourAsync(entity.id);

        result.maxGuests.Should().Be(8);
        result.boatId.Should().Be(TestGuids.BoatId);
        result.boatName.Should().Be("Rồng Vàng");
    }

    [Fact]
    public async Task GetActiveTourAsync_TraNullKhiTourChuaKhaiSoKhachToiDa()
    {
        var entity = ArrangeTour(maxGuests: null, boatEntity: new BoatBuilder().Build());

        var result = await CreateSut().GetActiveTourAsync(entity.id);

        result.maxGuests.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveTourAsync_VanTraThuyenKhiChiConLichDaQua()
    {
        // Hết lịch tương lai thì vẫn phải hiện tên thuyền, không để trang chi tiết ra "N/A".
        var boatEntity = new BoatBuilder().WithName("Hải Âu").Build();
        var entity = ArrangeTour(maxGuests: 12, boatEntity: boatEntity, departingInDays: -5);

        var result = await CreateSut().GetActiveTourAsync(entity.id);

        result.boatName.Should().Be("Hải Âu");
        result.maxGuests.Should().Be(12);
    }

    [Fact]
    public async Task GetActiveTourAsync_TraBoatNullKhiTourChuaGanThuyen()
    {
        var entity = ArrangeTour(maxGuests: 8, boatEntity: null);

        var result = await CreateSut().GetActiveTourAsync(entity.id);

        result.boatId.Should().BeNull();
        result.boatName.Should().BeNull();
        result.maxGuests.Should().Be(8);
    }
}
