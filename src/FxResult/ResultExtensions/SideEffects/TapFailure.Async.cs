using FxResult.Core;
using FxResult.Core.Meta;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Async side-effect extensions for failure paths.
/// </summary>
public static partial class TapExtensions
{
    /// <summary>
    /// Runs an async side-effect if the result is a failure. The result is returned unchanged.
    /// </summary>
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

    /// <summary>
    /// Runs an async side-effect if the awaited result is a failure. The result is returned unchanged.
    /// </summary>
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
