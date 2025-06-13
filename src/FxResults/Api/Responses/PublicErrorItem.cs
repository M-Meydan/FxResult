namespace FxResults.Api.Responses;

/// <summary>
/// DTO structure for converting internal errors to API-safe output.
/// </summary>
public class PublicErrorItem
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string? Source { get; set; }
}