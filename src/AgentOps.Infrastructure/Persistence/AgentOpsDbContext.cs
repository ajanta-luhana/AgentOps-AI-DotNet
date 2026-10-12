using AgentOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgentOps.Infrastructure.Persistence;

public sealed class AgentOpsDbContext(DbContextOptions<AgentOpsDbContext> options) : DbContext(options)
{
    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();

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

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Email).HasMaxLength(User.MaxEmailLength).IsRequired();
            e.Property(x => x.NormalizedEmail).HasMaxLength(User.MaxEmailLength).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.HasIndex(x => x.NormalizedEmail).IsUnique();
            e.HasIndex(x => x.OrganizationId);
            e.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
