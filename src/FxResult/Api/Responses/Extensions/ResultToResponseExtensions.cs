using FxResult.Core;
using FxResult.Core.Meta;

namespace FxResult.Api.Responses.Extensions;

/// <summary>API response conversion for Result{T}.</summary>
public static class ResultToResponseExtensions
{
    /// <summary>Converts to API DTO. Example: <c>result.ToResponseDto()</c></summary>
    public static ResultResponse<T> ToResponseDto<T>(this Result<T> result, MetaInfo? meta = null) =>
        result.TryGetValue(out var value)
            ? ResultResponse<T>.FromSuccess(value!, meta)
            : ResultResponse<T>.FromError(result.Error);
}
