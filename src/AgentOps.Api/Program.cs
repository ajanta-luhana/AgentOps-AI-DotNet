using System.Diagnostics;
using AgentOps.Api.Configuration;
using AgentOps.Api.Endpoints;
using AgentOps.Application.Common;
using AgentOps.Infrastructure;

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

builder.Services.AddInfrastructure(builder.Configuration);

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

app.MapGroup("/api/v1").MapHealthEndpoints();

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
