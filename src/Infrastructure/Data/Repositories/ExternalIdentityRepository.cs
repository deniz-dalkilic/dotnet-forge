using Microsoft.EntityFrameworkCore;
using Template.Application.Abstractions;
using Template.Domain.Entities;

namespace Template.Infrastructure.Data.Repositories;

public sealed class ExternalIdentityRepository(AppDbContext dbContext) : IExternalIdentityRepository
{
    public Task<ExternalIdentity?> FindAsync(ExternalAuthProvider provider, string subject, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        var normalizedSubject = subject.Trim();

        return dbContext.ExternalIdentities
            .FirstOrDefaultAsync(x => x.Provider == provider && x.Subject == normalizedSubject, cancellationToken);
    }

    public void Add(ExternalIdentity externalIdentity)
    {
        ArgumentNullException.ThrowIfNull(externalIdentity);
        dbContext.ExternalIdentities.Add(externalIdentity);
    }
}
