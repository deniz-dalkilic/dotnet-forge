using FluentValidation;

namespace DotnetForge.Application.ReferenceScenarios.Greetings;

public sealed class ReferenceScenarioGreetingRequestValidator : AbstractValidator<ReferenceScenarioGreetingRequest>
{
    public ReferenceScenarioGreetingRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TriggerSource)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.TriggerSource));
    }
}
