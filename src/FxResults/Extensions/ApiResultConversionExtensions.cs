using FxResults.Api.Responses;
using FxResults.Core;

namespace FxResults.Extensions;


/// <summary>
/// Extension methods for converting internal errors to public DTOs.
/// </summary>
public static class ApiResultConversionExtensions
{
    /// <summary>
    /// Converts an internal <see cref="Error"/> to a public <see cref="PublicErrorResponse"/>.
    /// </summary>
    /// <param name="error">The internal error.</param>
    /// <returns>The public error response.</returns>
    /// <example>
    /// var publicError = internalError.ToPublicDto();
    /// </example>
    public static PublicErrorResponse ToPublicDto(this Error? error)
    {
        if (error == null)
            return new PublicErrorResponse();

        return new PublicErrorResponse
        {
            Code = error.Code,
            Message = error.Message,
            Details = new List<PublicErrorItem>
            {
                new() { Code = error.Code, Message = error.Message, Source = error.Source }
            }
        };
    }
}
