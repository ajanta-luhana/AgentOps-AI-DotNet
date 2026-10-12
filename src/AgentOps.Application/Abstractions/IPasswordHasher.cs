namespace AgentOps.Application.Abstractions;

/// <summary>One-way password hashing. The stored string contains everything needed to verify later (salt, cost).</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string storedHash);
}
