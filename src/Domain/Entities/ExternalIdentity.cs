using Template.Domain.Common;

namespace Template.Domain.Entities;

public sealed class ExternalIdentity : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public ExternalAuthProvider Provider { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public DateTimeOffset LinkedAtUtc { get; private set; }

    public User? User { get; private set; }

    public ExternalIdentity()
    {
        Id = Guid.NewGuid();
        LinkedAtUtc = DateTimeOffset.UtcNow;
    }

    public ExternalIdentity(Guid userId, ExternalAuthProvider provider, string subject, string? email = null) : this()
    {
        SetUserId(userId);
        Provider = provider;
        SetSubject(subject);
        SetEmail(email);
    }

    public void SetUserId(Guid userId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        UserId = userId;
    }

    public void SetSubject(string subject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        Subject = subject.Trim();
    }

    public void SetEmail(string? email)
    {
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}
