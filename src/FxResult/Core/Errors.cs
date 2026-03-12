using System.Diagnostics.CodeAnalysis;

namespace FxResult.Core;

/// <summary>
/// Canonical factories for common error codes.
/// Keeps code/message ordering consistent and avoids magic strings.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Errors
{
    public static Error NullValue(
        Exception? exception = null,
        string? source = null,
        string? caller = null)
        => new(ErrorCodes.NULL_VALUE, "Value was null.", source, caller, exception);

    public static Error DataAccess(
        Exception exception,
        string? source = null,
        string? caller = null)
        => new(ErrorCodes.DATA_ACCESS_ERROR, exception.Message, source, caller, exception);

    public static Error OnSuccessCallback(Exception exception, string? source = null, string? caller = null)
        => new(ErrorCodes.ON_SUCCESS_ERROR, exception.Message, source, caller, exception);

    public static Error OnSuccessCallbackAsync(Exception exception, string? source = null, string? caller = null)
        => new(ErrorCodes.ON_SUCCESS_ASYNC_ERROR, exception.Message, source, caller, exception);
}
