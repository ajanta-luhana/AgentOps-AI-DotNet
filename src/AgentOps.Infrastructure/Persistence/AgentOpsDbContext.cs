using AgentOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentOps.Infrastructure.Persistence;

public sealed class AgentOpsDbContext(DbContextOptions<AgentOpsDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // pgvector is enabled through the migration (CREATE EXTENSION IF NOT EXISTS vector).
        // Vector columns and indexes arrive in Milestone 6 once the embedding dimension is chosen.
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Organization>(e =>
        {
            e.ToTable("Organizations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(Organization.MaxNameLength).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.Property(x => x.UpdatedAtUtc).IsRequired();
        });
    }
}
