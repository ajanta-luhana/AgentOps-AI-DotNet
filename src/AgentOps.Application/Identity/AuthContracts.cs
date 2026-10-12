namespace AgentOps.Application.Identity;

/// <summary>Body of POST /auth/register. Creates a new organization and its first (Admin) user.</summary>
public sealed record RegisterRequest(string? Email, string? Password, string? OrganizationName);

/// <summary>Body of POST /auth/login.</summary>
public sealed record LoginRequest(string? Email, string? Password);

/// <summary>Safe view of a user: never contains the password hash.</summary>
public sealed record UserDto(Guid Id, Guid OrganizationId, string Email, string Role);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserDto User);

/// <summary>Password rules in one place so API and tests agree.</summary>
public static class PasswordRules
{
    public const int MinLength = 12;
    public const int MaxLength = 128;
}
