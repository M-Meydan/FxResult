using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Side-effect and result/value capture extensions for Result{T}.
/// </summary>
public static partial class TapExtensions
{
    /// <summary>
    /// Runs a side-effect if the result is successful (result unchanged). Handles exceptions by returning an Error.
    /// </summary>
    public static Result<T> Tap<T>(
        this Result<T> result,
        Action<T> action,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.TryGetValue(out var value))
        {
            try
            {
                action(value!);
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
    /// Awaits the Task&lt;Result&lt;T&gt;&gt;, assigns the result to an out parameter, and returns the result for fluent chaining.
    /// </summary>
    public static Result<T> Tap<T>(this Task<Result<T>> resultTask, out Result<T> captured)
    {
        captured = resultTask.ConfigureAwait(false).GetAwaiter().GetResult();
        return captured;
    }

    /// <summary>
    /// Captures the successful value in an <c>out</c> parameter (here the full Result),
    /// returns the original result.
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, out Result<T> captured)
    {
        captured = result;
        return result;
    }
}
