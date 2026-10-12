using AgentOps.Domain.Entities;
using AgentOps.Domain.Enums;

namespace AgentOps.Domain.Tests;

public sealed class UserTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
    private static readonly Guid OrgId = Guid.CreateVersion7();

    [Fact]
    public void Create_TrimsEmailAndNormalizesToUpperCase()
    {
        var user = User.Create(OrgId, "  Rakesh@Example.com ", "hash", UserRole.Admin, Now);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(OrgId, user.OrganizationId);
        Assert.Equal("Rakesh@Example.com", user.Email);
        Assert.Equal("RAKESH@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal(Now, user.CreatedAtUtc);
    }

    [Fact]
    public void ToNormalizedEmail_TreatsDifferentCasingAsSameAccount() =>
        Assert.Equal(User.ToNormalizedEmail("A@x.com"), User.ToNormalizedEmail(" a@X.COM "));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("two@@example.com")]
    [InlineData("Display Name <a@example.com>")]
    public void Create_RejectsInvalidEmail(string email) =>
        Assert.ThrowsAny<ArgumentException>(() => User.Create(OrgId, email, "hash", UserRole.Member, Now));

    [Fact]
    public void Create_RejectsNullEmail() =>
        Assert.Throws<ArgumentNullException>(() => User.Create(OrgId, null!, "hash", UserRole.Member, Now));

    [Fact]
    public void Create_RejectsEmailLongerThanLimit()
    {
        var email = new string('a', User.MaxEmailLength) + "@example.com";
        Assert.Throws<ArgumentException>(() => User.Create(OrgId, email, "hash", UserRole.Member, Now));
    }

    [Fact]
    public void Create_RejectsEmptyOrganizationId() =>
        Assert.Throws<ArgumentException>(() => User.Create(Guid.Empty, "a@example.com", "hash", UserRole.Member, Now));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankPasswordHash(string hash) =>
        Assert.Throws<ArgumentException>(() => User.Create(OrgId, "a@example.com", hash, UserRole.Member, Now));

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Create_RejectsNonUtcTimestamp(DateTimeKind kind) =>
        Assert.Throws<ArgumentException>(() =>
            User.Create(OrgId, "a@example.com", "hash", UserRole.Member, new DateTime(2026, 1, 15, 10, 30, 0, kind)));
}
