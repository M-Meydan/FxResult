using FxResult.Core;
using FxResult.Core.Meta;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Side-effect extensions for failure paths.
/// </summary>
public static partial class TapExtensions
{
    /// <summary>
    /// Runs a side-effect if the result is a failure. The result is returned unchanged.
    /// Intended for logging/telemetry.
    /// </summary>
    public static Result<T> TapFailure<T>(
        this Result<T> result,
        Action<Error, MetaInfo> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.IsSuccess)
            return result;

        try
        {
            action(result.Error!, result.Meta);
        }
        catch
        {
            // Never let logging/side-effect failures change the business result.
        }

        return result;
    }

    /// <summary>
    /// Asynchronous version of <see cref="TapFailure{T}(Result{T}, Action{Error, MetaInfo}, string?, string?)"/>.
    /// Awaits the task, then runs the same side-effect if the result is a failure.
    /// </summary>
    public static async Task<Result<T>> TapFailure<T>(
        this Task<Result<T>> task,
        Action<Error, MetaInfo> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        var result = await task.ConfigureAwait(false);
        return result.TapFailure(action, source, caller);
    }
}
