using DDMS.Backend.Models.DTOs.TourContent;
using FluentValidation;

namespace DDMS.Backend.Common.Validators.TourContent;

public class CreateTourImageRequestValidator : AbstractValidator<CreateTourImageRequest>
{
    public CreateTourImageRequestValidator()
    {
        RuleFor(x => x.tour_id).NotEmpty();
        RuleFor(x => x.image_url).NotEmpty();
        RuleFor(x => x.sort_order).GreaterThanOrEqualTo(0);
    }
}

public class UpdateTourImageRequestValidator : AbstractValidator<UpdateTourImageRequest>
{
    public UpdateTourImageRequestValidator()
    {
        Include(new CreateTourImageRequestValidator());
    }
}

public class CreateFaqRequestValidator : AbstractValidator<CreateFaqRequest>
{
    public CreateFaqRequestValidator()
    {
        RuleFor(x => x.tour_id).NotEmpty();
        RuleFor(x => x.question).NotEmpty();
        RuleFor(x => x.answer).NotEmpty();
        RuleFor(x => x.sort_order).GreaterThanOrEqualTo(0);
    }
}

public class UpdateFaqRequestValidator : AbstractValidator<UpdateFaqRequest>
{
    public UpdateFaqRequestValidator()
    {
        Include(new CreateFaqRequestValidator());
    }
}

public class CreateDockScheduleRequestValidator : AbstractValidator<CreateDockScheduleRequest>
{
    public CreateDockScheduleRequestValidator()
    {
        RuleFor(x => x.dock_id).NotEmpty();
        RuleFor(x => x.boat_id).NotEmpty();
        RuleFor(x => x.start_time).NotEmpty();
        RuleFor(x => x.end_time).NotEmpty();
        RuleFor(x => x.end_time)
            .GreaterThan(x => x.start_time)
            .WithMessage("End time must be greater than start time");
    }
}

public class UpdateDockScheduleRequestValidator : AbstractValidator<UpdateDockScheduleRequest>
{
    public UpdateDockScheduleRequestValidator()
    {
        Include(new CreateDockScheduleRequestValidator());
    }
}
