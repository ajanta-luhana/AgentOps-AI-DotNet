using System.Globalization;
using System.Security.Claims;
using System.Text;
using AgentOps.Application.Abstractions;
using AgentOps.Application.Common;
using AgentOps.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgentOps.Infrastructure.Authentication;

/// <summary>Issues HS256-signed JWTs. Claims: sub (user id), org (organization id), role, email.</summary>
public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    public const string OrganizationClaim = "org";
    public const string RoleClaim = "role";

    public AccessToken Create(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var jwt = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(jwt.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expires,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString("D", CultureInfo.InvariantCulture)),
                new Claim(OrganizationClaim, user.OrganizationId.ToString("D", CultureInfo.InvariantCulture)),
                new Claim(RoleClaim, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256),
        };

        return new AccessToken(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}
