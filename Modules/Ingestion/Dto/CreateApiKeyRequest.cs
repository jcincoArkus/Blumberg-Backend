namespace Modules.Ingestion.Dto;

/// <summary>
/// Request to create an API key
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>Human-readable name (e.g. "Gateway A")</summary>
    /// <example>Gateway A</example>
    public string Name { get; set; } = string.Empty;
}
