using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.TourContent;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class TourContentService : ITourContentService
{
    private readonly ITourContentRepository _repository;

    public TourContentService(ITourContentRepository repository)
    {
        _repository = repository;
    }

    public async Task<TourImageResponse> CreateImageAsync(CreateTourImageRequest request, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(request.tour_id, cancellationToken);

        var image = new tour_image
        {
            id = Guid.NewGuid(),
            tour_id = request.tour_id,
            image_url = request.image_url,
            public_id = request.public_id,
            caption = request.caption,
            sort_order = request.sort_order,
            created_at = DateTime.UtcNow
        };

        await _repository.AddImageAsync(image, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(image);
    }

    public async Task<TourImageResponse> UpdateImageAsync(Guid id, UpdateTourImageRequest request, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(request.tour_id, cancellationToken);
        var image = await _repository.GetImageByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tour image not found");

        image.tour_id = request.tour_id;
        image.image_url = request.image_url;
        image.public_id = request.public_id;
        image.caption = request.caption;
        image.sort_order = request.sort_order;

        _repository.UpdateImage(image);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(image);
    }

    public async Task DeleteImageAsync(Guid id, CancellationToken cancellationToken)
    {
        var image = await _repository.GetImageByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tour image not found");

        _repository.DeleteImage(image);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TourImageResponse>> GetImagesByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(tourId, cancellationToken);
        var images = await _repository.GetImagesByTourIdAsync(tourId, cancellationToken);
        return images.Select(Map).ToList();
    }

    public async Task<FaqResponse> CreateFaqAsync(CreateFaqRequest request, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(request.tour_id, cancellationToken);

        var faq = new faq
        {
            id = Guid.NewGuid(),
            tour_id = request.tour_id,
            question = request.question,
            answer = request.answer,
            sort_order = request.sort_order,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _repository.AddFaqAsync(faq, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(faq);
    }

    public async Task<FaqResponse> UpdateFaqAsync(Guid id, UpdateFaqRequest request, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(request.tour_id, cancellationToken);
        var faq = await _repository.GetFaqByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Faq not found");

        faq.tour_id = request.tour_id;
        faq.question = request.question;
        faq.answer = request.answer;
        faq.sort_order = request.sort_order;
        faq.updated_at = DateTime.UtcNow;

        _repository.UpdateFaq(faq);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(faq);
    }

    public async Task DeleteFaqAsync(Guid id, CancellationToken cancellationToken)
    {
        var faq = await _repository.GetFaqByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Faq not found");

        _repository.DeleteFaq(faq);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<FaqResponse>> GetFaqsByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(tourId, cancellationToken);
        var faqs = await _repository.GetFaqsByTourIdAsync(tourId, cancellationToken);
        return faqs.Select(Map).ToList();
    }

    public async Task<DockScheduleResponse> CreateDockScheduleAsync(CreateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        await ValidateDockScheduleRequestAsync(request, null, cancellationToken);

        var dockSchedule = new dock_schedule
        {
            id = Guid.NewGuid(),
            dock_id = request.dock_id,
            boat_id = request.boat_id,
            schedule_id = request.schedule_id,
            start_time = request.start_time,
            end_time = request.end_time,
            created_at = DateTime.UtcNow
        };

        await _repository.AddDockScheduleAsync(dockSchedule, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(dockSchedule);
    }

    public async Task<DockScheduleResponse> UpdateDockScheduleAsync(Guid id, UpdateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        await ValidateDockScheduleRequestAsync(request, id, cancellationToken);
        var dockSchedule = await _repository.GetDockScheduleByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Dock schedule not found");

        dockSchedule.dock_id = request.dock_id;
        dockSchedule.boat_id = request.boat_id;
        dockSchedule.schedule_id = request.schedule_id;
        dockSchedule.start_time = request.start_time;
        dockSchedule.end_time = request.end_time;

        _repository.UpdateDockSchedule(dockSchedule);
        await _repository.SaveChangesAsync(cancellationToken);
        return Map(dockSchedule);
    }

    public async Task DeleteDockScheduleAsync(Guid id, CancellationToken cancellationToken)
    {
        var dockSchedule = await _repository.GetDockScheduleByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Dock schedule not found");

        _repository.DeleteDockSchedule(dockSchedule);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DockScheduleResponse>> GetDockSchedulesByDockIdAsync(Guid dockId, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsDockAsync(dockId, cancellationToken))
        {
            throw new BadRequestException("Dock does not exist");
        }

        var dockSchedules = await _repository.GetDockSchedulesByDockIdAsync(dockId, cancellationToken);
        return dockSchedules.Select(Map).ToList();
    }

    private async Task EnsureTourExistsAsync(Guid tourId, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsTourAsync(tourId, cancellationToken))
        {
            throw new BadRequestException("Tour does not exist");
        }
    }

    private async Task ValidateDockScheduleRequestAsync(CreateDockScheduleRequest request, Guid? excludeId, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsDockAsync(request.dock_id, cancellationToken))
        {
            throw new BadRequestException("Dock does not exist");
        }

        if (!await _repository.ExistsBoatAsync(request.boat_id, cancellationToken))
        {
            throw new BadRequestException("Boat does not exist");
        }

        if (request.end_time <= request.start_time)
        {
            throw new BadRequestException("End time must be greater than start time");
        }

        var hasOverlap = await _repository.HasOverlapAsync(
            request.dock_id,
            request.start_time,
            request.end_time,
            excludeId,
            cancellationToken);

        if (hasOverlap)
        {
            throw new BadRequestException("Dock time slot overlaps with existing schedule");
        }
    }

    private static TourImageResponse Map(tour_image source)
    {
        return new TourImageResponse
        {
            id = source.id,
            tour_id = source.tour_id,
            image_url = source.image_url,
            public_id = source.public_id,
            caption = source.caption,
            sort_order = source.sort_order
        };
    }

    private static FaqResponse Map(faq source)
    {
        return new FaqResponse
        {
            id = source.id,
            tour_id = source.tour_id,
            question = source.question,
            answer = source.answer,
            sort_order = source.sort_order
        };
    }

    private static DockScheduleResponse Map(dock_schedule source)
    {
        return new DockScheduleResponse
        {
            id = source.id,
            dock_id = source.dock_id,
            boat_id = source.boat_id,
            schedule_id = source.schedule_id,
            start_time = source.start_time,
            end_time = source.end_time
        };
    }
}
