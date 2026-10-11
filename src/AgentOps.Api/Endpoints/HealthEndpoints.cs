using AgentOps.Application.Abstractions;

namespace AgentOps.Api.Endpoints;

internal static partial class HealthLog
{
    public const string Category = "AgentOps.Api.Health";

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Readiness probe {Probe} failed with {ExceptionType}.")]
    public static partial void ProbeFailed(this ILogger logger, string probe, string exceptionType);
}

internal static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this RouteGroupBuilder api)
    {
        var health = api.MapGroup("/health");

        // Liveness: the process is up. Touches no dependency.
        health.MapGet("/live", () => Results.Ok(new { status = "Live" }));

        // Readiness: reports dependency status only. Never returns exception text, connection strings or paths.
        health.MapGet("/ready", async (IEnumerable<IReadinessProbe> probes, ILoggerFactory loggerFactory, CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger(HealthLog.Category);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));

            var checks = new List<object>();
            var allHealthy = true;
            foreach (var probe in probes)
            {
                bool healthy;
                try
                {
                    healthy = await probe.CheckAsync(timeout.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
                {
                    // Log only the exception type; messages can contain connection details.
                    logger.ProbeFailed(probe.Name, ex.GetType().Name);
                    healthy = false;
                }
                allHealthy &= healthy;
                checks.Add(new { name = probe.Name, status = healthy ? "Healthy" : "Unhealthy" });
            }

            var body = new { status = allHealthy ? "Ready" : "NotReady", checks };
            return allHealthy ? Results.Ok(body) : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
        });

        return api;
    }
}
