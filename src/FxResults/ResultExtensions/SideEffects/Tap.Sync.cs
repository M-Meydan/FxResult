using FxResults.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace FxResults.ResultExtensions;

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
    /// Runs a side-effect if the result is successful (result unchanged). Handles exceptions by returning an Error.
    /// <example>
    /// res.Tap(x => Console.WriteLine(x));
    /// // Logs if successful, returns input result
    /// </example>
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (result.TryGetValue(out var value))
        {
            try { action(value!); }
            catch (Exception ex) {
                var effectiveSource = source ?? action.Method?.Name;
                return new Error(ex.Message, ex.GetType().Name, effectiveSource,caller, ex);
            }
        }
        return result;
    }

    /// <summary>
    /// Awaits the Task&lt;Result&lt;T&gt;&gt;, assigns the result to an out parameter, and returns the result for fluent chaining.
    /// <example>
    /// var final = await taskResult.Tap(out var captured);
    /// // 'captured' holds the Result&lt;T&gt; at that step.
    /// </example>
    /// </summary>
    public static Result<T> Tap<T>(this Task<Result<T>> resultTask, out Result<T> captured)
    {
        captured = resultTask.ConfigureAwait(false).GetAwaiter().GetResult();
        return captured;
    }

    /// <summary>
    /// Captures the successful value in an <c>out</c> parameter (null if failed), returns the original result.
    /// <example>
    /// res.Tap(out var value);
    /// // value is result.Value if success, null otherwise
    /// </example>
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, out Result<T> captured)
    {
        captured = result;
        return result;
    }

}
