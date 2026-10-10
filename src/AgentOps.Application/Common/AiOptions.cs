using System.ComponentModel.DataAnnotations;

namespace AgentOps.Application.Common;

/// <summary>
/// Typed AI provider settings (requirements section 9.1). Committed configuration holds placeholders only;
/// real model names come from user-secrets or environment variables. Placeholder rejection is enforced
/// when the model client is introduced (Milestone 4), not here.
/// </summary>
public sealed class AiOptions
{
    public const string SectionName = "AI";

    [Required, RegularExpression("^Ollama$", ErrorMessage = "Only the 'Ollama' provider is supported.")]
    public string Provider { get; set; } = "Ollama";

    [Required, Url]
    public string BaseUrl { get; set; } = "http://localhost:11434";

    [Required]
    public string ChatModel { get; set; } = string.Empty;

    [Required]
    public string EmbeddingModel { get; set; } = string.Empty;

    [Range(1, 600)]
    public int RequestTimeoutSeconds { get; set; } = 120;

    [Range(1, 32768)]
    public int MaxOutputTokens { get; set; } = 2048;

    [Range(1, 64)]
    public int MaxCallsPerTask { get; set; } = 8;
}
