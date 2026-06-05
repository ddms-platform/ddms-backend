using DDMS.Backend.Models.DTOs.TourSchedule;
using FluentValidation;

namespace DDMS.Backend.Common.Validators.TourSchedule;

public class CreateTourScheduleRequestValidator : AbstractValidator<CreateTourScheduleRequest>
{
    private static readonly string[] ValidStatus = ["scheduled", "ongoing", "completed", "cancelled"];

    public CreateTourScheduleRequestValidator()
    {
        RuleFor(x => x.tour_id).NotEmpty();
        RuleFor(x => x.start_time).NotEmpty();
        RuleFor(x => x.end_time).NotEmpty();
        RuleFor(x => x.end_time)
            .GreaterThan(x => x.start_time)
            .WithMessage("End time must be greater than start time");
        RuleFor(x => x.status)
            .NotEmpty()
            .Must(s => ValidStatus.Contains(s))
            .WithMessage("Status must be scheduled, ongoing, completed or cancelled");
    }
}

public class UpdateTourScheduleRequestValidator : AbstractValidator<UpdateTourScheduleRequest>
{
    public UpdateTourScheduleRequestValidator()
    {
        Include(new CreateTourScheduleRequestValidator());
    }
}
