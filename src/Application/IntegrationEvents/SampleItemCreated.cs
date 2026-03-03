namespace Template.Application.IntegrationEvents;

public sealed record SampleItemCreated(Guid SampleItemId, string Name, DateTime CreatedAtUtc);
