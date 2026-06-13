using DDMS.Backend.Common.Exceptions;
using DDMS.Backend.Models.DTOs.Tours;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Repositories.Interfaces;
using DDMS.Backend.Services.Interfaces;

namespace DDMS.Backend.Services.Implementations;

public class FaqService : IFaqService
{
    private readonly IFaqRepository _faqRepository;
    private readonly IOwnerToursRepository _tourRepository;

    public FaqService(IFaqRepository faqRepository, IOwnerToursRepository tourRepository)
    {
        _faqRepository = faqRepository;
        _tourRepository = tourRepository;
    }

    public async Task<List<FaqItemResponse>> GetByTourIdAsync(Guid tourId, Guid userId)
    {
        await EnsureTourOwnedAsync(tourId, userId);
        var items = await _faqRepository.GetByTourIdAsync(tourId);
        return items.Select(MapFaq).ToList();
    }

    public async Task<FaqItemResponse> GetByIdAsync(Guid tourId, Guid faqId, Guid userId)
    {
        await EnsureTourOwnedAsync(tourId, userId);
        var entity = await _faqRepository.GetByIdAsync(faqId, tourId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        return MapFaq(entity);
    }

    public async Task<FaqItemResponse> CreateAsync(Guid tourId, Guid userId, CreateFaqRequest request)
    {
        await EnsureTourOwnedAsync(tourId, userId);
        ValidateFaqInput(request.question, request.answer, request.sortOrder);

        var entity = new faq
        {
            id = Guid.NewGuid(),
            tour_id = tourId,
            question = request.question.Trim(),
            answer = request.answer.Trim(),
            sort_order = request.sortOrder,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        await _faqRepository.AddAsync(entity);
        return MapFaq(entity);
    }

    public async Task<FaqItemResponse> UpdateAsync(Guid tourId, Guid faqId, Guid userId, UpdateFaqRequest request)
    {
        await EnsureTourOwnedAsync(tourId, userId);
        ValidateFaqInput(request.question, request.answer, request.sortOrder);

        var entity = await _faqRepository.GetByIdAsync(faqId, tourId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        entity.question = request.question.Trim();
        entity.answer = request.answer.Trim();
        entity.sort_order = request.sortOrder;
        entity.updated_at = DateTime.UtcNow;

        await _faqRepository.UpdateAsync(entity);
        return MapFaq(entity);
    }

    public async Task DeleteAsync(Guid tourId, Guid faqId, Guid userId)
    {
        await EnsureTourOwnedAsync(tourId, userId);

        var entity = await _faqRepository.GetByIdAsync(faqId, tourId);
        if (entity is null)
        {
            throw new NotFoundException();
        }

        await _faqRepository.DeleteAsync(entity);
    }

    private async Task EnsureTourOwnedAsync(Guid tourId, Guid userId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId, userId);
        if (tour is null)
        {
            throw new AppException(ErrorCode.FaqTourNotFound, ErrorCode.Messages.FaqTourNotFound);
        }
    }

    private static void ValidateFaqInput(string question, string answer, int sortOrder)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(question))
        {
            errors["question"] = [ErrorCode.Messages.FaqQuestionRequired];
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            errors["answer"] = [ErrorCode.Messages.FaqAnswerRequired];
        }

        if (sortOrder < 0)
        {
            errors["sortOrder"] = [ErrorCode.Messages.FaqSortOrderInvalid];
        }

        if (errors.Count > 0)
        {
            throw new AppException(ErrorCode.FaqValidationFailed, ErrorCode.Messages.TourValidationFailed, errors);
        }
    }

    private static FaqItemResponse MapFaq(faq entity)
    {
        return new FaqItemResponse
        {
            id = entity.id,
            tourId = entity.tour_id,
            question = entity.question,
            answer = entity.answer,
            sortOrder = entity.sort_order,
            createdAt = entity.created_at,
            updatedAt = entity.updated_at
        };
    }
}
