using DotnetForge.Application.Common;
using DotnetForge.Domain.Greetings;
using FluentValidation;

namespace DotnetForge.Application.Greetings;

public sealed class GreetingApplicationService : IGreetingApplicationService
{
    private readonly IValidator<GreetingRequest> _validator;
    private readonly IGreetingRepository _greetingRepository;
    private readonly IGreetingCache _greetingCache;

    public GreetingApplicationService(
        IValidator<GreetingRequest> validator,
        IGreetingRepository greetingRepository,
        IGreetingCache greetingCache)
    {
        _validator = validator;
        _greetingRepository = greetingRepository;
        _greetingCache = greetingCache;
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
        await _greetingRepository.AddAsync(greeting, cancellationToken);
        var response = GreetingResponse.FromDomain(greeting);
        await _greetingCache.SetAsync(response, cancellationToken);

        return Result<GreetingResponse>.Success(response);
    }

    public async Task<Result<GreetingResponse>> GetGreetingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var greeting = await _greetingCache.GetOrCreateAsync(
            id,
            async token =>
            {
                var entity = await _greetingRepository.GetByIdAsync(id, token);
                return entity is null ? null : GreetingResponse.FromDomain(entity);
            },
            cancellationToken);

        if (greeting is null)
        {
            return Result<GreetingResponse>.Failure(
                Error.NotFound("greetings.not_found", $"Greeting '{id}' was not found."));
        }

        return Result<GreetingResponse>.Success(greeting);
    }
}
