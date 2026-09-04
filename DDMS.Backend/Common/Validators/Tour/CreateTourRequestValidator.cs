using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.DTOs.Tour;
using FluentValidation;

namespace DDMS.Backend.Common.Validators.Tour;

public class CreateTourRequestValidator : AbstractValidator<CreateTourRequest>
{
    private static readonly string[] ValidStatus = [.. TourConstants.Statuses.Allowed];
    private static readonly string[] ValidCancelPolicy = ["free", "partial", "no_refund"];

    public CreateTourRequestValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Tour name is required")
            .MaximumLength(255);

        RuleFor(x => x.price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater than or equal to 0");

        RuleFor(x => x.duration_minutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0");

        RuleFor(x => x.max_guests)
            .GreaterThan(0).WithMessage("Max guests must be greater than 0")
            .When(x => x.max_guests.HasValue);

        RuleFor(x => x.status)
            .NotEmpty()
            .Must(s => ValidStatus.Contains(s?.Trim().ToLowerInvariant() ?? string.Empty))
            .WithMessage("Status must be pending, active, inactive or rejected");

        RuleFor(x => x.cancel_policy)
            .NotEmpty()
            .Must(p => ValidCancelPolicy.Contains(p))
            .WithMessage("Cancel policy must be free, partial or no_refund");

        RuleFor(x => x.cancel_hours)
            .GreaterThanOrEqualTo(0)
            .When(x => x.cancel_hours.HasValue);

        RuleFor(x => x.rejection_reason)
            .NotEmpty()
            .WithMessage("Rejection reason is required when rejecting a tour")
            .MaximumLength(2000)
            .When(x => string.Equals(
                x.status?.Trim(), TourConstants.Statuses.Rejected, StringComparison.OrdinalIgnoreCase));
    }
}

public class UpdateTourRequestValidator : AbstractValidator<UpdateTourRequest>
{
    public UpdateTourRequestValidator()
    {
        Include(new CreateTourRequestValidator());
    }
}

public class TourFilterRequestValidator : AbstractValidator<TourFilterRequest>
{
    public TourFilterRequestValidator()
    {
        RuleFor(x => x.status)
            .Must(s => string.IsNullOrWhiteSpace(s) || TourConstants.Statuses.Allowed.Contains(s.Trim().ToLowerInvariant()))
            .WithMessage("Status must be pending, active, inactive or rejected");
    }
}
