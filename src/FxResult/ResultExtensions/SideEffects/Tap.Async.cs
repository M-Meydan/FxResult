using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>Async side-effect extensions for Result{T}.</summary>
public static partial class TapExtensions
{
    /// <summary>Awaits task, then runs sync side-effect on success.</summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Action<T> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Tap(action, source, caller);
    }

    /// <summary>Runs async side-effect on success. Example: <c>result.TapAsync(x =&gt; LogAsync(x))</c></summary>
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

    /// <summary>Runs async side-effect on success with CancellationToken.</summary>
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

    /// <summary>Awaits task, then runs async side-effect on success.</summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await result.TapAsync(action, source, caller).ConfigureAwait(false);
    }

    /// <summary>Awaits task, then runs async side-effect on success with CancellationToken.</summary>
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
