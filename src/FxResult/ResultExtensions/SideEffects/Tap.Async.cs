using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Side-effect and result/value capture extensions for Result{T}.
/// </summary>
public static partial class TapExtensions
{
    /// <summary>
    /// Runs a side-effect if the result is successful (result unchanged).
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(action, source, caller);
    }

    /// <summary>
    /// Runs an async side-effect if the result is successful (result unchanged).
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.TryGetValue(out var value))
        {
            try
            {
                await action(value!).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var effectiveSource = source ?? action.Method?.Name;
                return Result<T>.Fail(new Error(ex.GetType().Name, ex.Message, effectiveSource, caller, ex), result.Meta);
            }
        }

        return result;
    }

    /// <summary>
    /// Runs an async side-effect if the result is successful (result unchanged) (CancellationToken support).
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.TryGetValue(out var value))
        {
            try
            {
                await action(value!, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var effectiveSource = source ?? action.Method?.Name;
                return Result<T>.Fail(new Error(ex.GetType().Name, ex.Message, effectiveSource, caller, ex), result.Meta);
            }
        }

        return result;
    }

    /// <summary>
    /// Executes an async side-effect on the successful value of a <c>Task&lt;Result&lt;T&gt;&gt;</c>.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(action, source, caller).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an async side-effect on the successful value of a <c>Task&lt;Result&lt;T&gt;&gt;</c> (CancellationToken support).
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, CancellationToken, Task> action,
        CancellationToken cancellationToken = default,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(action, cancellationToken, source, caller).ConfigureAwait(false);
    }
}
