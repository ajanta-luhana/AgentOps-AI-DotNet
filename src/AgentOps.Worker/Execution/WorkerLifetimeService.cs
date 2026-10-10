using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentOps.Worker.Execution;

/// <summary>
/// Milestone 1 host skeleton. It performs NO task processing: the durable queue, leases, recovery and
/// execution arrive in Milestone 3. It exists so the worker process starts, validates configuration and
/// shuts down cleanly.
/// </summary>
internal sealed partial class WorkerLifetimeService(ILogger<WorkerLifetimeService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Started();
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information,
        Message = "AgentOps AI .NET worker started. Task processing is not implemented until Milestone 3.")]
    private partial void Started();
}
