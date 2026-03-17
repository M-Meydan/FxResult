using FxResult.Core;
using FxResult.Core.Meta;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>Async failure side-effect extensions.</summary>
public static partial class TapExtensions
{
    /// <summary>Runs async side-effect on failure. Example: <c>result.TapFailureAsync(async (err, meta) =&gt; await AlertAsync(err))</c></summary>
    public static async Task<Result<T>> TapFailureAsync<T>(
        this Result<T> result,
        Func<Error, MetaInfo, Task> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.IsSuccess)
            return result;

        try
        {
            await action(result.Error!, result.Meta).ConfigureAwait(false);
        }
        catch
        {
            // Never let logging/side-effect failures change the business result.
        }

        return result;
    }

    /// <summary>Awaits task, then runs async side-effect on failure.</summary>
    public static async Task<Result<T>> TapFailureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, MetaInfo, Task> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.TapFailureAsync(action, source, caller).ConfigureAwait(false);
    }
}
