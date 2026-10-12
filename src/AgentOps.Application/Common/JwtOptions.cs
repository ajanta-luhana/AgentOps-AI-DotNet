using System.ComponentModel.DataAnnotations;

namespace AgentOps.Application.Common;

/// <summary>
/// JWT settings. Issuer/audience live in appsettings.json; the signing key is a secret and must come from
/// user-secrets or an environment variable (Jwt__SigningKey). The API refuses to start without a long enough key.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required, MinLength(32, ErrorMessage = "Jwt:SigningKey must be at least 32 characters.")]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 30;
}
