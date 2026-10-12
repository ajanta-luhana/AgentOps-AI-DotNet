using System.Security.Claims;
using AgentOps.Application.Abstractions;
using AgentOps.Application.Identity;
using AgentOps.Domain.Entities;
using AgentOps.Domain.Enums;
using AgentOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgentOps.Api.Endpoints;

internal static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth");

        auth.MapPost("/register", RegisterAsync);
        auth.MapPost("/login", LoginAsync);
        auth.MapGet("/me", MeAsync).RequireAuthorization();

        return api;
    }

    // Creates a new organization and its first user (an Admin) in one database transaction.
    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        AgentOpsDbContext db,
        IPasswordHasher hasher,
        ITokenService tokens,
        TimeProvider time,
        CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }
        if (string.IsNullOrWhiteSpace(request.OrganizationName))
        {
            errors["organizationName"] = ["Organization name is required."];
        }
        if (request.Password is null
            || request.Password.Length < PasswordRules.MinLength
            || request.Password.Length > PasswordRules.MaxLength)
        {
            errors["password"] = [$"Password must be {PasswordRules.MinLength}-{PasswordRules.MaxLength} characters."];
        }
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var now = time.GetUtcNow().UtcDateTime;
        Organization organization;
        User user;
        try
        {
            organization = Organization.Create(request.OrganizationName!, now);
            user = User.Create(organization.Id, request.Email!, hasher.Hash(request.Password!), UserRole.Admin, now);
        }
        catch (ArgumentException ex)
        {
            // Domain rules (email format, name length) surface as a 400 with the offending field name.
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "request"] = ["The value is not valid."],
            });
        }

        if (await db.Users.AnyAsync(u => u.NormalizedEmail == user.NormalizedEmail, ct))
        {
            return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "An account with this email already exists.");
        }

        db.Organizations.Add(organization);
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two registrations with the same email at the same moment: the unique index decides.
            return Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "An account with this email already exists.");
        }

        return Results.Created("/api/v1/auth/me", ToResponse(user, tokens.Create(user)));
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        AgentOpsDbContext db,
        IPasswordHasher hasher,
        ITokenService tokens,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password)
            || request.Password.Length > PasswordRules.MaxLength)
        {
            return InvalidCredentials();
        }

        var normalized = User.ToNormalizedEmail(request.Email);
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
        if (user is null)
        {
            // Spend the same time as a real check so response time does not reveal which emails exist.
            _ = hasher.Hash(request.Password);
            return InvalidCredentials();
        }

        return hasher.Verify(request.Password, user.PasswordHash)
            ? Results.Ok(ToResponse(user, tokens.Create(user)))
            : InvalidCredentials();
    }

    private static async Task<IResult> MeAsync(ClaimsPrincipal principal, AgentOpsDbContext db, CancellationToken ct)
    {
        if (!Guid.TryParse(principal.FindFirst("sub")?.Value, out var userId))
        {
            return Results.Unauthorized();
        }

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? Results.Unauthorized() : Results.Ok(ToDto(user));
    }

    // Same message for "unknown email" and "wrong password" on purpose.
    private static IResult InvalidCredentials() =>
        Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");

    private static UserDto ToDto(User user) =>
        new(user.Id, user.OrganizationId, user.Email, user.Role.ToString());

    private static AuthResponse ToResponse(User user, AccessToken token) =>
        new(token.Value, token.ExpiresAtUtc, ToDto(user));
}
