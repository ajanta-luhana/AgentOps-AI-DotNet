using System.ComponentModel.DataAnnotations;
using AgentOps.Application.Common;

namespace AgentOps.Application.Tests;

public sealed class AiOptionsValidationTests
{
    private static AiOptions Valid() => new()
    {
        ChatModel = "REPLACE_WITH_INSTALLED_MODEL",
        EmbeddingModel = "REPLACE_WITH_INSTALLED_EMBEDDING_MODEL",
    };

    private static List<ValidationResult> Validate(AiOptions options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void DefaultsWithModelNames_AreValid() => Assert.Empty(Validate(Valid()));

    [Theory]
    [InlineData(0)]
    [InlineData(65)]
    public void MaxCallsPerTask_OutOfRange_IsInvalid(int value)
    {
        var options = Valid();
        options.MaxCallsPerTask = value;
        Assert.NotEmpty(Validate(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(601)]
    public void RequestTimeoutSeconds_OutOfRange_IsInvalid(int value)
    {
        var options = Valid();
        options.RequestTimeoutSeconds = value;
        Assert.NotEmpty(Validate(options));
    }

    [Fact]
    public void UnsupportedProvider_IsInvalid()
    {
        var options = Valid();
        options.Provider = "SomethingElse";
        Assert.NotEmpty(Validate(options));
    }

    [Fact]
    public void BaseUrl_NotAUrl_IsInvalid()
    {
        var options = Valid();
        options.BaseUrl = "not a url";
        Assert.NotEmpty(Validate(options));
    }

    [Fact]
    public void MissingModelNames_AreInvalid() => Assert.NotEmpty(Validate(new AiOptions()));
}
