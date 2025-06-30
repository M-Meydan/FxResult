namespace FxResult.Api.Responses;

/// <summary>
/// DTO structure for converting internal errors to API-safe output.
/// </summary>
public class PublicErrorItem
{
    public PublicErrorItem() {}

    public PublicErrorItem(string code, string message, string source)
    {
        Code = code;
        Message = message;
        Source = source;
    }

    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
}