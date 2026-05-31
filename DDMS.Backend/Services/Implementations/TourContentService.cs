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
        return TourContentMapper.Map(image);
    }

    public async Task<TourImageResponse> UpdateImageAsync(Guid id, UpdateTourImageRequest request, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(request.tour_id, cancellationToken);

        var image = await _repository.GetImageByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.TourImageNotFound, ErrorCode.Messages.TourImageNotFound);

        image.tour_id = request.tour_id;
        image.image_url = request.image_url;
        image.public_id = request.public_id;
        image.caption = request.caption;
        image.sort_order = request.sort_order;

        _repository.UpdateImage(image);
        await _repository.SaveChangesAsync(cancellationToken);
        return TourContentMapper.Map(image);
    }

    public async Task DeleteImageAsync(Guid id, CancellationToken cancellationToken)
    {
        var image = await _repository.GetImageByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.TourImageNotFound, ErrorCode.Messages.TourImageNotFound);

        _repository.DeleteImage(image);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TourImageResponse>> GetImagesByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(tourId, cancellationToken);
        var images = await _repository.GetImagesByTourIdAsync(tourId, cancellationToken);
        return images.Select(TourContentMapper.Map).ToList();
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
        return TourContentMapper.Map(faq);
    }

    public async Task<FaqResponse> UpdateFaqAsync(Guid id, UpdateFaqRequest request, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(request.tour_id, cancellationToken);

        var faq = await _repository.GetFaqByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.FaqNotFound, ErrorCode.Messages.FaqNotFound);

        faq.tour_id = request.tour_id;
        faq.question = request.question;
        faq.answer = request.answer;
        faq.sort_order = request.sort_order;
        faq.updated_at = DateTime.UtcNow;

        _repository.UpdateFaq(faq);
        await _repository.SaveChangesAsync(cancellationToken);
        return TourContentMapper.Map(faq);
    }

    public async Task DeleteFaqAsync(Guid id, CancellationToken cancellationToken)
    {
        var faq = await _repository.GetFaqByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.FaqNotFound, ErrorCode.Messages.FaqNotFound);

        _repository.DeleteFaq(faq);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<FaqResponse>> GetFaqsByTourIdAsync(Guid tourId, CancellationToken cancellationToken)
    {
        await EnsureTourExistsAsync(tourId, cancellationToken);
        var faqs = await _repository.GetFaqsByTourIdAsync(tourId, cancellationToken);
        return faqs.Select(TourContentMapper.Map).ToList();
    }

    public async Task<DockScheduleResponse> CreateDockScheduleAsync(CreateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        await ValidateDockScheduleBusinessAsync(request, null, cancellationToken);

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
        return TourContentMapper.Map(dockSchedule);
    }

    public async Task<DockScheduleResponse> UpdateDockScheduleAsync(Guid id, UpdateDockScheduleRequest request, CancellationToken cancellationToken)
    {
        await ValidateDockScheduleBusinessAsync(request, id, cancellationToken);

        var dockSchedule = await _repository.GetDockScheduleByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.DockScheduleNotFound, ErrorCode.Messages.DockScheduleNotFound);

        dockSchedule.dock_id = request.dock_id;
        dockSchedule.boat_id = request.boat_id;
        dockSchedule.schedule_id = request.schedule_id;
        dockSchedule.start_time = request.start_time;
        dockSchedule.end_time = request.end_time;

        _repository.UpdateDockSchedule(dockSchedule);
        await _repository.SaveChangesAsync(cancellationToken);
        return TourContentMapper.Map(dockSchedule);
    }

    public async Task DeleteDockScheduleAsync(Guid id, CancellationToken cancellationToken)
    {
        var dockSchedule = await _repository.GetDockScheduleByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(ErrorCode.DockScheduleNotFound, ErrorCode.Messages.DockScheduleNotFound);

        _repository.DeleteDockSchedule(dockSchedule);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<DockScheduleResponse>> GetDockSchedulesByDockIdAsync(Guid dockId, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsDockAsync(dockId, cancellationToken))
        {
            throw new AppException(ErrorCode.DockNotExists, ErrorCode.Messages.DockNotExists);
        }

        var dockSchedules = await _repository.GetDockSchedulesByDockIdAsync(dockId, cancellationToken);
        return dockSchedules.Select(TourContentMapper.Map).ToList();
    }

    private async Task EnsureTourExistsAsync(Guid tourId, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsTourAsync(tourId, cancellationToken))
        {
            throw new AppException(ErrorCode.TourNotExists, ErrorCode.Messages.TourNotExists);
        }
    }

    private async Task ValidateDockScheduleBusinessAsync(
        CreateDockScheduleRequest request,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsDockAsync(request.dock_id, cancellationToken))
        {
            throw new AppException(ErrorCode.DockNotExists, ErrorCode.Messages.DockNotExists);
        }

        if (!await _repository.ExistsBoatAsync(request.boat_id, cancellationToken))
        {
            throw new AppException(ErrorCode.BoatNotExists, ErrorCode.Messages.BoatNotExists);
        }

        var hasOverlap = await _repository.HasOverlapAsync(
            request.dock_id,
            request.start_time,
            request.end_time,
            excludeId,
            cancellationToken);

        if (hasOverlap)
        {
            throw new AppException(ErrorCode.DockScheduleOverlap, ErrorCode.Messages.DockScheduleOverlap);
        }
    }
}
