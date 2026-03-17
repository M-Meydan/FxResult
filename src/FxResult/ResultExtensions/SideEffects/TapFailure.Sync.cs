using FxResult.Core;
using FxResult.Core.Meta;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>Failure side-effect extensions — logging/telemetry without changing the result.</summary>
public static partial class TapExtensions
{
    /// <summary>Runs a side-effect on failure (unchanged). Example: <c>result.TapFailure((err, meta) =&gt; Log(err))</c></summary>
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

    /// <summary>Awaits task, then runs sync side-effect on failure.</summary>
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
