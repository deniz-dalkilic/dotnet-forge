using Template.Domain.Common;

namespace Template.Domain.Entities;

public sealed class User : Entity<Guid>
{
    public string? Email { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public User()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public User(string displayName, string? email = null) : this()
    {
        SetDisplayName(displayName);
        SetEmail(email);
    }

    public void SetDisplayName(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        DisplayName = displayName.Trim();
    }

    public void SetEmail(string? email)
    {
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}
