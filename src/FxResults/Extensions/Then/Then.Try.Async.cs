using FxResults.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FxResults.Extensions;

/// <summary>
/// Asynchronous exception-safe transformations for Result{T} and Task<Result{T}>.
/// <para>
/// Examples:
/// <code>
/// var r2 = await r1.ThenTryAsync(async x => await DangerousAsync(x));
/// var r3 = await r1.ThenTryAsync(async x => await TryAsync(x));
/// </code>
/// </para>
/// </summary>
public static partial class ThenExtensions
{
    /// <summary>
    /// Try/catch safe transformation on Task&lt;Result&lt;T&gt;&gt; with sync transform.
    /// <example>
    /// var r2 = await r1.ThenTry(x => DangerousOp(x));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, TOut> transform, string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return result.ThenTry(transform, source, caller);
    }

    /// <summary>
    /// Try/catch safe Result-returning transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Result<TOut>> transform, string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return result.ThenTry(transform,source, caller);
    }

    /// <summary>
    /// Async try/catch safe transformation.
    /// <example>
    /// var r2 = await r1.ThenTryAsync(async x => await DangerousAsync(x));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<TOut>> transform, string? source = null, [CallerMemberName] string caller = "")
    {
        if (!result.TryGetValue(out var value))
            return result.Error!;
        try { return await transform(value!); }
        catch (Exception ex)
        {
            var effectiveSource = source ?? transform.Method?.Name;
            return new Error(ex.Message, ex.GetType().Name, caller, effectiveSource, ex);
        }
    }

    /// <summary>
    /// Async try/catch safe Result-returning transformation.
    /// <example>
    /// var r2 = await r1.ThenTryAsync(async x => await TryAsync(x));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<Result<TOut>>> transform,string? source = null, [CallerMemberName] string caller = "")
    {
        if (!result.TryGetValue(out var value))
            return result.Error!;

        try { return await transform(value!); }
        catch (Exception ex)
        {
            var effectiveSource = source ?? transform.Method?.Name;
            return new Error(ex.Message, ex.GetType().Name, caller, effectiveSource, ex);
        }
    }

    /// <summary>
    /// Async try/catch safe transformation on Task&lt;Result&lt;T&gt;&gt; with Task&lt;TOut&gt; transform.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<TOut>> transform,string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return await result.ThenTryAsync(transform, caller);
    }

    /// <summary>
    /// Async try/catch safe Result-returning transformation on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<Result<TOut>>> transform,string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return await result.ThenTryAsync(transform, source, caller);
    }
}