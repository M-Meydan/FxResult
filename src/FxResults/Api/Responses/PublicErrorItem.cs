namespace FxResults.Api.Responses;

/// <summary>
/// DTO structure for converting internal errors to API-safe output.
/// </summary>
public class PublicErrorItem
{
    public PublicErrorItem(){}

    public PublicErrorItem(string code, string message, string source)
    {
        Code = code; 
        Message = message;
        Source = source;
    }

    public string Code { get; set; }
    public string Message { get; set; }
    public string? Source { get; set; }
}