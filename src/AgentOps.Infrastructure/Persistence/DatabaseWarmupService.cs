using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgentOps.Infrastructure.Persistence;

/// <summary>
/// Opens one database connection shortly after startup so the first real request (for example /health/ready)
/// does not pay the cold-start cost. Best effort: a failure is logged by exception type only and never stops the host.
/// </summary>
internal sealed partial class DatabaseWarmupService(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseWarmupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AgentOpsDbContext>();
            if (await db.Database.CanConnectAsync(stoppingToken))
            {
                WarmedUp();
            }
            else
            {
                Unavailable();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Exception messages can contain connection details; log the type only.
            Failed(ex.GetType().Name);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Database connection warmed up.")]
    private partial void WarmedUp();

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Database warm-up could not connect.")]
    private partial void Unavailable();

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Database warm-up failed with {ExceptionType}.")]
    private partial void Failed(string exceptionType);
}
