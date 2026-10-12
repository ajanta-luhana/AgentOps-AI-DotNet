namespace AgentOps.Domain.Enums;

/// <summary>Role of a user inside their organization. Stored as text so the database stays readable.</summary>
public enum UserRole
{
    Member = 0,
    Admin = 1,
}
