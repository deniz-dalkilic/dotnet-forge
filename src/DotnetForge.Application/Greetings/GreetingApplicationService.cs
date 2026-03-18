using DotnetForge.Application.Common;
using DotnetForge.Domain.Greetings;
using FluentValidation;

namespace DotnetForge.Application.Greetings;

public sealed class GreetingApplicationService : IGreetingApplicationService
{
    private readonly IValidator<GreetingRequest> _validator;

    public GreetingApplicationService(IValidator<GreetingRequest> validator)
    {
        _validator = validator;
    }

    public async Task<Result<GreetingResponse>> CreateGreetingAsync(GreetingRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).Distinct().ToArray());

            return Result<GreetingResponse>.ValidationFailure(errors);
        }

        var greeting = Greeting.Create(request.Name, DateTimeOffset.UtcNow);
        var response = new GreetingResponse(greeting.Name, greeting.Message, greeting.CreatedAtUtc);

        return Result<GreetingResponse>.Success(response);
    }
}
