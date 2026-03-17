using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>Side-effect and capture extensions for Result{T}.</summary>
public static partial class TapExtensions
{
    /// <summary>Runs a side-effect on success (unchanged). Example: <c>result.Tap(x =&gt; Log(x))</c></summary>
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

    /// <summary>Awaits and captures via out. Example: <c>task.Tap(out var captured)</c></summary>
    public static Result<T> Tap<T>(this Task<Result<T>> resultTask, out Result<T> captured)
    {
        captured = resultTask.ConfigureAwait(false).GetAwaiter().GetResult();
        return captured;
    }

    /// <summary>Captures the result via out. Example: <c>result.Tap(out var captured)</c></summary>
    public static Result<T> Tap<T>(this Result<T> result, out Result<T> captured)
    {
        captured = result;
        return result;
    }
}
