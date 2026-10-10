namespace AgentOps.Domain.Entities;

/// <summary>Tenant root. Every tenant-owned entity is reachable from an Organization.</summary>
public sealed class Organization
{
    public const int MaxNameLength = 200;

    // Required by EF Core materialization; domain code must use Create().
    private Organization() => Name = string.Empty;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public static Organization Create(string name, DateTime utcNow)
    {
        EnsureUtc(utcNow);
        return new Organization
        {
            Id = Guid.CreateVersion7(),
            Name = NormalizeName(name),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };
    }

    public void Rename(string name, DateTime utcNow)
    {
        EnsureUtc(utcNow);
        Name = NormalizeName(name);
        UpdatedAtUtc = utcNow;
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
        {
            throw new ArgumentException($"Organization name must be at most {MaxNameLength} characters.", nameof(name));
        }
        return trimmed;
    }

    private static void EnsureUtc(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.", nameof(value));
        }
    }
}
