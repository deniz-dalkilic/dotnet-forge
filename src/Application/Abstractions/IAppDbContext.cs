using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;

namespace Template.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<SampleItem> SampleItems { get; }
    DbSet<User> Users { get; }
    DbSet<ExternalIdentity> ExternalIdentities { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
