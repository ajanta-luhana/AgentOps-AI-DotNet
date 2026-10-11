using AgentOps.Application.Abstractions;
using AgentOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentOps.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "DefaultConnection";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Fail fast at startup if the connection string is missing; never fall back to a default credential.
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionStrings:{ConnectionStringName} is not configured. Use dotnet user-secrets or an environment variable.");
        }

        services.AddDbContext<AgentOpsDbContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(AgentOpsDbContext).Assembly.FullName)));

        services.AddScoped<IReadinessProbe, PostgresReadinessProbe>();
        services.AddHostedService<DatabaseWarmupService>();
        return services;
    }
}
