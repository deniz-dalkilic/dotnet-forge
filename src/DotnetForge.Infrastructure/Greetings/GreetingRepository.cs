using DotnetForge.Application.Greetings;
using DotnetForge.Domain.Greetings;
using DotnetForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotnetForge.Infrastructure.Greetings;

public sealed class GreetingRepository : IGreetingRepository
{
    private readonly ForgeDbContext _dbContext;

    public GreetingRepository(ForgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Greeting greeting, CancellationToken cancellationToken = default)
    {
        await _dbContext.Greetings.AddAsync(greeting, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Greeting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Greetings
            .AsNoTracking()
            .SingleOrDefaultAsync(greeting => greeting.Id == id, cancellationToken);
    }
}
