using DDMS.Backend.Models.DTOs.Route;
using FluentValidation;

namespace DDMS.Backend.Common.Validators.Route;

public class CreateRouteRequestValidator : AbstractValidator<CreateRouteRequest>
{
    public CreateRouteRequestValidator()
    {
        RuleFor(x => x.tour_id).NotEmpty();
        RuleFor(x => x.start_point).NotEmpty().MaximumLength(255);
        RuleFor(x => x.end_point).NotEmpty().MaximumLength(255);
        RuleFor(x => x.sort_order).GreaterThanOrEqualTo(0);
    }
}

public class UpdateRouteRequestValidator : AbstractValidator<UpdateRouteRequest>
{
    public UpdateRouteRequestValidator()
    {
        Include(new CreateRouteRequestValidator());
    }
}
