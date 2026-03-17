using FxResult.Core;

namespace FxResult.Api.Responses.Extensions;

/// <summary>Converts internal Error to public-safe DTO.</summary>
public static class ErrorToPublicResponseExtensions
{
    /// <summary>Maps Error to PublicErrorResponse. Example: <c>error.ToPublicDto()</c></summary>
    public static PublicErrorResponse ToPublicDto(this Error? error)
    {
        if (error == null)
            return new PublicErrorResponse();

        var details = new List<PublicErrorItem>();

        details.Add(new() { Code = error.Code, Message = error.Message, Source = error.Source });

        if (error.Exception != null)
        {
            details.Add(new PublicErrorItem("EXCEPTION", error.Exception.Message, error.Exception.GetType().Name));

            var innerEx = error.Exception.InnerException;
            while (innerEx != null)
            {
                details.Add(new PublicErrorItem("INNER_EXCEPTION", innerEx.Message, innerEx.GetType().Name));
                innerEx = innerEx.InnerException;
            }
        }

        return new PublicErrorResponse(error.Code, error.Message, details);
    }
}