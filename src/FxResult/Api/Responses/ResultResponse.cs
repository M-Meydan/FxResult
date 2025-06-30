using FxResult.Api.Responses.Extensions;
using FxResult.Core;

namespace FxResult.Api.Responses;
/// <summary>
/// DTO structure for API responses containing data or errors.
/// </summary>
/// <typeparam name="T">The type of data.</typeparam>
public class ResultResponse<T>
{
    public T? Data { get; set; }
    public PublicErrorResponse? Error { get; set; }
    public object? Meta { get; set; }

    /// <summary>
    /// Creates a successful response with data and optional metadata.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="meta">Optional metadata.</param>
    /// <returns>A <see cref="ResultResponse{T}"/> representing success.</returns>
    /// <example>
    /// var response = ResultResponse&lt;string&gt;.FromSuccess("hello");
    /// </example>
    public static ResultResponse<T> FromSuccess(T data, object? meta = null) =>
        new() { Data = data, Meta = meta };

    /// <summary>
    /// Creates an error response from an internal error.
    /// </summary>
    /// <param name="error">The internal error.</param>
    /// <returns>A <see cref="ResultResponse{T}"/> representing failure.</returns>
    /// <example>
    /// var errorResponse = ResultResponse&lt;string&gt;.FromError(error);
    /// </example>
    public static ResultResponse<T> FromError(Error error) =>
        new() { Error = error.ToPublicDto() };
}
