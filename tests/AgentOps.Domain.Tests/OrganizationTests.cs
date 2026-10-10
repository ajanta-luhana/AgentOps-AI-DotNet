using AgentOps.Domain.Entities;

namespace AgentOps.Domain.Tests;

public sealed class OrganizationTests
{
    private static readonly DateTime Now = new(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_TrimsNameAndSetsUtcTimestamps()
    {
        var org = Organization.Create("  Contoso  ", Now);

        Assert.NotEqual(Guid.Empty, org.Id);
        Assert.Equal("Contoso", org.Name);
        Assert.Equal(Now, org.CreatedAtUtc);
        Assert.Equal(Now, org.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsBlankName(string name) =>
        Assert.Throws<ArgumentException>(() => Organization.Create(name, Now));

    [Fact]
    public void Create_RejectsNullName() =>
        Assert.Throws<ArgumentNullException>(() => Organization.Create(null!, Now));

    [Fact]
    public void Create_RejectsNameLongerThanLimit() =>
        Assert.Throws<ArgumentException>(() =>
            Organization.Create(new string('a', Organization.MaxNameLength + 1), Now));

    [Fact]
    public void Create_AcceptsNameAtLimit()
    {
        var org = Organization.Create(new string('a', Organization.MaxNameLength), Now);
        Assert.Equal(Organization.MaxNameLength, org.Name.Length);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Create_RejectsNonUtcTimestamp(DateTimeKind kind) =>
        Assert.Throws<ArgumentException>(() =>
            Organization.Create("Contoso", new DateTime(2026, 1, 15, 10, 30, 0, kind)));

    [Fact]
    public void Rename_UpdatesNameAndUpdatedTimestampOnly()
    {
        var org = Organization.Create("Contoso", Now);
        var later = Now.AddHours(1);

        org.Rename("Fabrikam", later);

        Assert.Equal("Fabrikam", org.Name);
        Assert.Equal(Now, org.CreatedAtUtc);
        Assert.Equal(later, org.UpdatedAtUtc);
    }
}
