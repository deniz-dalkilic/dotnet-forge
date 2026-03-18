using FluentValidation;

namespace DotnetForge.Application.Greetings;

public sealed class GreetingRequestValidator : AbstractValidator<GreetingRequest>
{
    public GreetingRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
