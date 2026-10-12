using System.Diagnostics;
using System.Text;
using AgentOps.Api.Configuration;
using AgentOps.Api.Endpoints;
using AgentOps.Application.Common;
using AgentOps.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Typed options validated at startup: a bad configuration stops the process instead of failing later.
builder.Services.AddOptions<AiOptions>()
    .Bind(builder.Configuration.GetSection(AiOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
if (corsSettings.AllowedOrigins.Length == 0 || corsSettings.AllowedOrigins.Any(o => o == "*"))
{
    throw new InvalidOperationException("Cors:AllowedOrigins must list explicit origins; wildcard origins are not allowed.");
}

// JWT settings (the signing key is a secret: user-secrets or Jwt__SigningKey). A missing/short key stops startup.
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearer.MapInboundClaims = false; // keep short claim names: sub, org, role
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "sub",
            RoleClaimType = "role",
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(corsSettings.AllowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// RFC 7807 Problem Details for all errors, with a correlation id and no internal exception detail.
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var v1 = app.MapGroup("/api/v1");
v1.MapHealthEndpoints();
v1.MapAuthEndpoints();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
