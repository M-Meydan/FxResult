using FxResult.Api.Responses;
using FxResult.Core;
using FxResult.Core.Meta;

namespace FxResult.Api.Responses.Extensions;

/// <summary>
/// Converts Result{T} to ResultResponse{T} for API/DTO output
/// </summary>
public static class ResultToResponseExtensions
{
    /// <summary>
    /// Converts a Result to a DTO, optionally with metadata.
    /// </summary>
    public static ResultResponse<T> ToResponseDto<T>(this Result<T> result, MetaInfo? meta = null) =>
        result.TryGetValue(out var value)
            ? ResultResponse<T>.FromSuccess(value!, meta)
            : ResultResponse<T>.FromError(result.Error);
}
