using DotnetForge.Application.Common;
using DotnetForge.Application.Greetings;
using DotnetForge.Domain.Greetings;
using FluentValidation;

namespace DotnetForge.Application.ReferenceScenarios.Greetings;

public sealed class ReferenceScenarioGreetingService : IReferenceScenarioGreetingService
{
    private const string DefaultTriggerSource = "reference-scenario-api";

    private readonly IValidator<ReferenceScenarioGreetingRequest> _validator;
    private readonly IGreetingRepository _greetingRepository;
    private readonly IGreetingCache _greetingCache;
    private readonly IReferenceScenarioJobDispatcher _jobDispatcher;

    public ReferenceScenarioGreetingService(
        IValidator<ReferenceScenarioGreetingRequest> validator,
        IGreetingRepository greetingRepository,
        IGreetingCache greetingCache,
        IReferenceScenarioJobDispatcher jobDispatcher)
    {
        _validator = validator;
        _greetingRepository = greetingRepository;
        _greetingCache = greetingCache;
        _jobDispatcher = jobDispatcher;
    }

    public async Task<Result<ReferenceScenarioGreetingResponse>> ExecuteAsync(
        ReferenceScenarioGreetingRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).Distinct().ToArray());

            return Result<ReferenceScenarioGreetingResponse>.ValidationFailure(errors);
        }

        var greeting = Greeting.Create(request.Name, DateTimeOffset.UtcNow);
        await _greetingRepository.AddAsync(greeting, cancellationToken);

        var greetingResponse = GreetingResponse.FromDomain(greeting);
        await _greetingCache.SetAsync(greetingResponse, cancellationToken);

        var triggerSource = string.IsNullOrWhiteSpace(request.TriggerSource)
            ? DefaultTriggerSource
            : request.TriggerSource.Trim();

        var backgroundJobId = _jobDispatcher.EnqueueGreetingFollowUp(
            $"Reference scenario follow-up for {greetingResponse.Name}",
            correlationId);

        return Result<ReferenceScenarioGreetingResponse>.Success(
            ReferenceScenarioGreetingResponse.Create(greetingResponse, backgroundJobId, correlationId, triggerSource));
    }

    public async Task<Result<ReferenceScenarioGreetingDetailsResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
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
            return Result<ReferenceScenarioGreetingDetailsResponse>.Failure(
                Error.NotFound("reference_scenario.greeting.not_found", $"Reference scenario greeting '{id}' was not found."));
        }

        return Result<ReferenceScenarioGreetingDetailsResponse>.Success(
            ReferenceScenarioGreetingDetailsResponse.Create(greeting));
    }
}
