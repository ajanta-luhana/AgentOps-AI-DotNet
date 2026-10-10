namespace AgentOps.Application.Abstractions;

/// <summary>A dependency check surfaced by /health/ready. Implementations must not leak secrets or exception details.</summary>
public interface IReadinessProbe
{
    string Name { get; }

    Task<bool> CheckAsync(CancellationToken cancellationToken);
}
