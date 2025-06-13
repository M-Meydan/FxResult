using FxResults.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FxResults.Extensions;

/// <summary>
/// Asynchronous chaining transformations for Result{T} and Task<Result{T}>.
/// <para>
/// Examples:
/// <code>
/// var r2 = await r1.Then(x => x.ToString());
/// var r3 = await r1.ThenAsync(x => Task.FromResult(x.Length));
/// </code>
/// </para>
/// </summary>
public static partial class ThenExtensions
{

    /// <summary>
    /// For Task&lt;Result&lt;T&gt;&gt; with sync transform and caller.
    /// </summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, TOut> transform, string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return result.Then(transform,source, caller);
    }

    /// <summary>
    /// For Task&lt;Result&lt;T&gt;&gt; with Result-returning sync transform.
    /// </summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Result<TOut>> transform, string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return result.Then(transform, source, caller);
    }

    /// <summary>
    /// Asynchronously chains a transformation if result is successful.
    /// <example>
    /// var r2 = await r1.ThenAsync(x => Task.FromResult(x.Length));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<TOut>> transform, string? source = null, [CallerMemberName] string caller = "")
        => result.TryGetValue(out var value) ? await transform(value!) : result.Error!.WithContext(source,caller);

    /// <summary>
    /// Asynchronously chains another Result-returning transformation.
    /// <example>
    /// var r2 = await r1.ThenAsync(x => Task.FromResult(Result.Success(x.Length)));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<Result<TOut>>> transform,string? source = null, [CallerMemberName] string caller = "")
        => result.TryGetValue(out var value) ? await transform(value!): result.Error!.WithContext(source, caller);

    /// <summary>
    /// Async chaining (sync transform on async result).
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<TOut>> transform,string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return await result.ThenAsync(transform,source, caller);
    }

    /// <summary>
    /// Async chaining (Result-returning transform on async result).
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<Result<TOut>>> transform,string? source = null, [CallerMemberName] string caller = "")
    {
        var result = await resultTask;
        return await result.ThenAsync(transform,source, caller);
    }
}
