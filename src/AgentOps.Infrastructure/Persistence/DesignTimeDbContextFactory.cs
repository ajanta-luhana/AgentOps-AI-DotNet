using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgentOps.Infrastructure.Persistence;

/// <summary>
/// Used only by `dotnet ef`. Reads the connection string from the environment so no credentials live in source.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AgentOpsDbContext>
{
    public AgentOpsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException(
                "Set the ConnectionStrings__DefaultConnection environment variable before running dotnet ef.");

        var options = new DbContextOptionsBuilder<AgentOpsDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AgentOpsDbContext).Assembly.FullName))
            .Options;
        return new AgentOpsDbContext(options);
    }
}
