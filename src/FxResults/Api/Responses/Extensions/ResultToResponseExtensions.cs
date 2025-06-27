using FxResults.Api.Responses;
using FxResults.Core;

namespace FxResults.Api.Responses.Extensions;

/// <summary>
/// Converts Result{T} to ResultResponse{T} for API/DTO output
/// </summary>
public static class ResultToResponseExtensions
{
    /// <summary>
    /// Converts a Result to a DTO, optionally with metadata.
    /// <example>
    /// var response = result.ToResponseDto();
    /// // If result success, response.Data set, else response.Error set
    /// </example>
    /// </summary>
    public static ResultResponse<T> ToResponseDto<T>(this Result<T> result, MetaInfo? meta = null) =>
        result.TryGetValue(out var value)
            ? ResultResponse<T>.FromSuccess(value!, meta)
            : ResultResponse<T>.FromError(result.Error!); // Uses null-forgiving operator
}
