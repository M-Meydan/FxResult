using FxResults.Api.Responses;
using FxResults.Core;
using System.Collections.Generic;

namespace FxResults.Api.Responses.Extensions;


/// <summary>
/// Extension methods for converting internal errors to public DTOs.
/// </summary>
public static class ErrorToPublicResponseExtensions
{
    /// <summary>
    /// Converts an internal <see cref="Error"/> to a public <see cref="PublicErrorResponse"/>.
    /// </summary>
    /// <example>
    /// var publicError = internalError.ToPublicDto();
    /// </example>
    public static PublicErrorResponse ToPublicDto(this Error? error)
    {
        if (error == null)
            return new PublicErrorResponse();

        var details = new List<PublicErrorItem>();

        // Add the primary error details
        details.Add(new() { Code = error.Code, Message = error.Message, Source = error.Source });

        // Add exception details if available
        if (error.Exception != null)
        {
            details.Add(new PublicErrorItem("EXCEPTION",error.Exception.Message, error.Exception.GetType().Name));

            // Recursively add inner exception details
            var innerEx = error.Exception.InnerException;
            while (innerEx != null)
            {
                details.Add(new PublicErrorItem("INNER_EXCEPTION", innerEx.Message, innerEx.GetType().Name));
                innerEx = innerEx.InnerException;
            }
        }

        return new PublicErrorResponse(error.Code, error.Message,details);
    }
}