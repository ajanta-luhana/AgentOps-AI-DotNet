using AgentOps.Application.Abstractions;

namespace AgentOps.Infrastructure.Persistence;

internal sealed class PostgresReadinessProbe(AgentOpsDbContext db) : IReadinessProbe
{
    public string Name => "postgresql";

    public Task<bool> CheckAsync(CancellationToken cancellationToken) =>
        db.Database.CanConnectAsync(cancellationToken);
}
