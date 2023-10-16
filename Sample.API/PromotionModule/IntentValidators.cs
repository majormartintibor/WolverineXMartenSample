using FluentValidation;
using FluentValidation.Results;
using JasperFx.Core;
using Wolverine.FluentValidation;

namespace Sample.API.PromotionModule;

/// <summary>
/// Validators get called before the coad reaches the Handler.
/// This way you can trust your commands and don't need to pollute
/// your handlers with guard clauses.
/// If validation fails a ValidationException gets thrown.
/// </summary>
public sealed class RequestPromotionValidator : AbstractValidator<RequestPromotion>
{
    public RequestPromotionValidator()
    {
        RuleFor(c => c.Promotee).NotEmpty().WithMessage("Promotee may not be empty");
        RuleFor(c => c.Promotee).MinimumLength(4).WithMessage("Promotee min length is 4");
        RuleFor(c => c.Promotee).MaximumLength(10).WithMessage("Promotee max length is 10");
    }
}

public sealed class SupervisorRespondsValidator : AbstractValidator<SupervisorResponds>
{
    public SupervisorRespondsValidator(IDateTimeOffsetProvider dateTimeOffsetProvider)
    {
        RuleFor(c => c.PromotionId).NotEmpty().WithMessage("Id must be provided");

        RuleFor(c => c.DecisionMadeAt)
            .LessThan(dateTimeOffsetProvider.GetCurrentDateTimeOffset())
            .WithMessage("The decision can not be in the future");
    }
}

public sealed class HRRespondsValidator : AbstractValidator<HRResponds>
{
    public HRRespondsValidator(IDateTimeOffsetProvider dateTimeOffsetProvider)
    {
        RuleFor(c => c.PromotionId).NotEmpty().WithMessage("Id must be provided");

        RuleFor(c => c.DecisionMadeAt)
            .LessThan(dateTimeOffsetProvider.GetCurrentDateTimeOffset())
            .WithMessage("The decision can not be in the future");
    }
}

public sealed class CEORespondsValidator : AbstractValidator<CEOResponds>
{
    public CEORespondsValidator(IDateTimeOffsetProvider dateTimeOffsetProvider)
    {
        RuleFor(c => c.PromotionId).NotEmpty().WithMessage("Id must be provided");

        RuleFor(c => c.DecisionMadeAt)
            .LessThan(dateTimeOffsetProvider.GetCurrentDateTimeOffset())
            .WithMessage("The decision can not be in the future");
    }
}

public sealed class RequestPromotionStatusValidator : AbstractValidator<RequestPromotionStatus>
{
    public RequestPromotionStatusValidator()
    {
        RuleFor(c => c.PromotionId).NotEmpty().WithMessage("Id must be provided");
    }
}

public sealed class CustomValidationException : Exception
{
    public CustomValidationException(string? message) : base(message)
    {
    }
}

public sealed class CustomFailureAction<T> : IFailureAction<T>
{
    public void Throw(T message, IReadOnlyList<ValidationFailure> failures)
    {
        string validationFailureMessage = "Validation errors: "
            + failures
                .Select(x => x.FormattedMessagePlaceholderValues["PropertyName"] + " " + x.ErrorMessage).Join(", ");

        throw new CustomValidationException(validationFailureMessage);
    }
}

public sealed class RequestPromotionDetailsValidator : AbstractValidator<RequestPromotionDetails>
{
    public RequestPromotionDetailsValidator()
    {
        RuleFor(c => c.PromotionId).NotEmpty().WithMessage("Id must be provided");
    }
}

public sealed class RequestPromotionDetailsWithVersionValidator : AbstractValidator<RequestPromotionDetailsWithVersion>
{
    public RequestPromotionDetailsWithVersionValidator()
    {
        RuleFor(c => c.PromotionId).NotEmpty().WithMessage("Id must be provided");
        RuleFor(c => c.Version).NotEmpty().WithMessage("Version must be provided");
    }
}