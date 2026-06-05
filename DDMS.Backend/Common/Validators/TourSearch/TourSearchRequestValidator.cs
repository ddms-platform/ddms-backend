using DDMS.Backend.Models.DTOs.TourSearch;
using FluentValidation;

namespace DDMS.Backend.Common.Validators.TourSearch;

public class TourSearchRequestValidator : AbstractValidator<TourSearchRequest>
{
    public TourSearchRequestValidator()
    {
        RuleFor(x => x.min_price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.min_price.HasValue);

        RuleFor(x => x.max_price)
            .GreaterThanOrEqualTo(x => x.min_price)
            .When(x => x.min_price.HasValue && x.max_price.HasValue);

        RuleFor(x => x.status)
            .Must(s => s is null or "active" or "inactive")
            .WithMessage("Status must be active or inactive");

        RuleFor(x => x.sort_by)
            .Must(s => s is null or "price" or "rating")
            .WithMessage("sort_by must be price or rating");

        RuleFor(x => x.min_duration_minutes)
            .GreaterThan(0)
            .When(x => x.min_duration_minutes.HasValue);

        RuleFor(x => x.max_duration_minutes)
            .GreaterThanOrEqualTo(x => x.min_duration_minutes)
            .When(x => x.min_duration_minutes.HasValue && x.max_duration_minutes.HasValue);
    }
}
