using FxResult.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FxResult.ResultExtensions;

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
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, TOut> transform, string? source = null, [CallerMemberName] string? caller = null)
    {
        var result = await resultTask;
        return result.ThenTry(transform, source, caller);
    }

    /// <summary>
    /// Try/catch safe Result-returning transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Result<TOut>> transform, string? source = null, [CallerMemberName] string? caller = null)
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
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<TOut>> transform, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return result.Error!;
        try { return await transform(value!); }
        catch (Exception ex)
        {
             var effectiveSource = source ?? (transform.Method?.DeclaringType?.FullName + "." + transform.Method?.Name);
            return new Error(ex.Message, ex.GetType().Name, effectiveSource, caller, ex);
        }
    }

    /// <summary>
    /// Async try/catch safe Result-returning transformation.
    /// <example>
    /// var r2 = await r1.ThenTryAsync(async x => await TryAsync(x));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<Result<TOut>>> transform,string? source = null, [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return result.Error!;

        try { return await transform(value!); }
        catch (Exception ex)
        {
            var effectiveSource = source ?? (transform.Method?.DeclaringType?.FullName + "." + transform.Method?.Name);
            return new Error(ex.Message, ex.GetType().Name, effectiveSource, caller, ex);
        }
    }

    /// <summary>
    /// Async try/catch safe transformation on Task&lt;Result&lt;T&gt;&gt; with Task&lt;TOut&gt; transform.
    /// </summary>
    /// <example>
    /// var r2 = await resultTask.ThenTryAsync(async x => await DangerousAsync(x));
    /// </example>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<TOut>> transform,string? source = null, [CallerMemberName] string? caller = null)
    {
        var result = await resultTask;
        return await result.ThenTryAsync(transform, source, caller);
    }

    /// <summary>
    /// Async try/catch safe Result-returning transformation on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    /// <example>
    /// var r2 = await resultTask.ThenTryAsync(async x => await TryAsync(x));
    /// </example>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<Result<TOut>>> transform,string? source = null, [CallerMemberName] string? caller = null)
    {
        var result = await resultTask;
        return await result.ThenTryAsync(transform, source, caller);
    }

    /// <summary>
    /// Asynchronous chaining for Result<Unit>. 
    /// </summary>
    /// <example>
    /// var result = await someResult.ThenTry(async () => await SomeAsyncOperation()); 
    /// </example>
    public static async Task<Result<Unit>> ThenTry(this Result<Unit> result, Func<Task> action, string? source = null, [CallerMemberName] string? caller = null)
    {
        if (result.IsFailure) return result;

        try
        {
            await action();
            return Result<Unit>.Success(Unit.Value, result.Meta);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Fail(ex, ex.GetType().Name, source, caller);
        }
    }
}