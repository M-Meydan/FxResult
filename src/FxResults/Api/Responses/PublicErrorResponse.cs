namespace FxResults.Api.Responses;

/// <summary>
/// DTO structure for public error responses.
/// </summary>
public class PublicErrorResponse
{
    public string Code { get; set; } = "UNKNOWN";
    public string Message { get; set; } = "An error occurred.";
    public List<PublicErrorItem> Details { get; set; } = new();
}