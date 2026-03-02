using Template.Domain.Common;

namespace Template.Domain.Entities;

public sealed class SampleItem : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    public SampleItem()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public SampleItem(string name) : this()
    {
        SetName(name);
    }

    public void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }
}
