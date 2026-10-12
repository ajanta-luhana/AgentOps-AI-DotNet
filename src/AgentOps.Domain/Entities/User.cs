using System.Net.Mail;
using AgentOps.Domain.Enums;

namespace AgentOps.Domain.Entities;

/// <summary>A person who belongs to exactly one <see cref="Organization"/>. Passwords are only ever stored as hashes.</summary>
public sealed class User
{
    public const int MaxEmailLength = 254;

    // Required by EF Core materialization; domain code must use Create().
    private User()
    {
        Email = string.Empty;
        NormalizedEmail = string.Empty;
        PasswordHash = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }

    /// <summary>Email as the user typed it (trimmed). Shown back to the user.</summary>
    public string Email { get; private set; }

    /// <summary>Upper-cased email used for uniqueness checks and login lookups.</summary>
    public string NormalizedEmail { get; private set; }

    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static User Create(Guid organizationId, string email, string passwordHash, UserRole role, DateTime utcNow)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("Organization id is required.", nameof(organizationId));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        if (utcNow.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Timestamps must be UTC.", nameof(utcNow));
        }

        var trimmed = ValidateEmail(email);
        return new User
        {
            Id = Guid.CreateVersion7(),
            OrganizationId = organizationId,
            Email = trimmed,
            NormalizedEmail = ToNormalizedEmail(trimmed),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAtUtc = utcNow,
        };
    }

    /// <summary>Normalization used for lookups, so "A@x.com" and "a@X.com" are the same account.</summary>
    public static string ToNormalizedEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return email.Trim().ToUpperInvariant();
    }

    private static string ValidateEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        var trimmed = email.Trim();
        if (trimmed.Length > MaxEmailLength)
        {
            throw new ArgumentException($"Email must be at most {MaxEmailLength} characters.", nameof(email));
        }
        if (!MailAddress.TryCreate(trimmed, out var parsed) || !string.Equals(parsed.Address, trimmed, StringComparison.Ordinal))
        {
            throw new ArgumentException("Email address is not valid.", nameof(email));
        }
        return trimmed;
    }
}
