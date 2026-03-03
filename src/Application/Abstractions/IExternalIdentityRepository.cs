using Template.Domain.Entities;

namespace Template.Application.Abstractions;

public interface IExternalIdentityRepository
{
    Task<ExternalIdentity?> FindAsync(ExternalAuthProvider provider, string subject, CancellationToken cancellationToken = default);
    void Add(ExternalIdentity externalIdentity);
}
