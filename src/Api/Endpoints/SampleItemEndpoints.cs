using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Template.Application.Abstractions;
using Template.Application.IntegrationEvents;
using Template.Domain.Entities;

namespace Template.Api.Endpoints;

public static class SampleItemEndpoints
{
    public static IEndpointRouteBuilder MapSampleItemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sample-items", async (
            CreateSampleItemRequest request,
            IAppDbContext dbContext,
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            CancellationToken ct) =>
        {
            var item = new SampleItem(request.Name);
            await dbContext.SampleItems.AddAsync(item, ct);
            await unitOfWork.SaveChangesAsync(ct);

            var integrationEvent = new SampleItemCreated(item.Id, item.Name, item.CreatedAtUtc);
            await eventBus.PublishAsync(integrationEvent, ct);

            return Results.Created($"/api/sample-items/{item.Id}", new
            {
                item.Id,
                item.Name,
                item.CreatedAtUtc
            });
        });

        return endpoints;
    }

    public sealed record CreateSampleItemRequest(string Name);
}
