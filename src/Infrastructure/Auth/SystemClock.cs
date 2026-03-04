using Template.Application.Abstractions;

namespace Template.Infrastructure.Auth;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
