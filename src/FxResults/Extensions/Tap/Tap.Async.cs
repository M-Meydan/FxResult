using FxResults.Core;
using System.Runtime.CompilerServices;

namespace FxResults.Extensions;

/// <summary>
/// Side-effect and result/value capture extensions for Result{T}.
/// <para>
/// Examples:
/// <code>
/// res.Tap(x => Console.WriteLine(x)); // Logs if success, result unchanged
/// res.Tap(out var captured); // captured holds result, flow continues
/// await res.TapAsync(async x => await LogAsync(x)); // Async log
/// res.Tap(out var value); // value holds the result value if success
/// </code>
/// </para>
/// </summary>
public static partial class TapExtensions
{
    /// <summary>
    /// Runs a side-effect if the result is successful (result unchanged).
    /// <example>
    /// res.Tap(x => Console.WriteLine(x));
    /// // Logs if successful, returns input result
    /// </example>
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Action<T> action, string? source = null, [CallerMemberName] string? caller = null)
    {
        var result = await resultTask;
        return result.Tap(action,source, caller);
    }


    /// <summary>
    /// Runs an async side-effect if the result is successful (result unchanged).
    /// <example>
    /// await res.TapAsync(async x => await LogAsync(x));
    /// // Logs asynchronously, returns input result
    /// </example>
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> action, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (result.TryGetValue(out var value))
        {
            try { await action(value!); }
            catch (Exception ex) {
                var effectiveSource = source ?? action.Method?.Name;
                return new Error(ex.Message, ex.GetType().Name, caller, effectiveSource, ex);
            }
        }
        return result;
    }

    /// <summary>
    /// Executes an async side-effect on the successful value of a <c>Task&lt;Result&lt;T&gt;&gt;</c>.
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action, string? source = null, [CallerMemberName] string? caller = null)
    {
        var result = await resultTask;
        return await result.TapAsync(action,source, caller);
    }
}
