using AgentOps.Domain.Entities;

namespace AgentOps.Application.Abstractions;

/// <summary>A signed access token and the moment it stops being valid.</summary>
public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);

public interface ITokenService
{
    AccessToken Create(User user);
}
