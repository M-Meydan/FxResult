using FxResults.Core;
using System;
using System.Threading.Tasks;

namespace FxResults.ResultExtensions;

/// <summary>
/// Asynchronous chaining transformations for Result{T} and Task&lt;Result{T}&gt;.
/// These methods assume pure (non-throwing) delegates. Use ThenTryAsync for fallible operations.
/// <para>
/// Examples:
/// <code>
/// var r2 = await r1.ThenAsync(x => Task.FromResult(x.Length));
/// var r3 = await r1.ThenAsync(x => Task.FromResult(Result.Success(x.ToString())));
/// </code>
/// </para>
/// </summary>
public static partial class ThenExtensions
{
    /// <summary>
    /// Chains a pure async transformation on a successful Result.
    /// This method does not catch exceptions — use ThenTryAsync for fallible logic.
    /// <example>
    /// var r2 = await r1.ThenAsync(x => Task.FromResult(x.Length));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<TOut>> transform) =>
        result.TryGetValue(out var value)?(await transform(value!)): result.Error!;

    /// <summary>
    /// Chains a pure async transformation returning Result&lt;TOut&gt;.
    /// This method does not catch exceptions — use ThenTryAsync for fallible logic.
    /// <example>
    /// var r2 = await r1.ThenAsync(x => Task.FromResult(Result.Success(x.ToString())));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result,Func<TIn, Task<Result<TOut>>> transform) =>
        result.TryGetValue(out var value) ? await transform(value!) : result.Error!;

    /// <summary>
    /// Async chaining — pure async transform on Task&lt;Result&lt;T&gt;&gt;.
    /// <example>
    /// var r2 = await resultTask.ThenAsync(x => Task.FromResult(x.Length));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<TOut>> transform) =>
        await (await resultTask).ThenAsync(transform);

    /// <summary>
    /// Async chaining — Result-returning async transform on Task&lt;Result&lt;T&gt;&gt;.
    /// <example>
    /// var r2 = await resultTask.ThenAsync(x => Task.FromResult(Result.Success(x.Length)));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Task<Result<TOut>>> transform) =>
        await (await resultTask).ThenAsync(transform);

    /// <summary>
    /// Async chaining — sync transform on Task&lt;Result&lt;T&gt;&gt;.
    /// <example>
    /// var r2 = await resultTask.Then(x => x.Length);
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, TOut> transform) =>
        (await resultTask).Then(transform);

    /// <summary>
    /// Async chaining — sync transform returning Result&lt;TOut&gt; on Task&lt;Result&lt;T&gt;&gt;.
    /// <example>
    /// var r2 = await resultTask.Then(x => Result.Success(x.Length));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask,Func<TIn, Result<TOut>> transform) =>
        (await resultTask).Then(transform);

    /// <summary>
    /// Chains a pure async transformation if the Result&lt;Unit&gt; is successful.
    /// This method does not catch exceptions — use ThenTryAsync for fallible logic.
    /// <example>
    /// var r2 = await r1.ThenAsync(() => Task.FromResult("Hello"));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<Unit> result, Func<Task<TOut>> transform) =>
        result.IsSuccess ? await transform() : result.Error!;

    /// <summary>
    /// Chains a pure async transformation returning Result&lt;TOut&gt; if the Result&lt;Unit&gt; is successful.
    /// This method does not catch exceptions — use ThenTryAsync for fallible logic.
    /// <example>
    /// var r2 = await r1.ThenAsync(() => Task.FromResult(Result.Success("Hello")));
    /// </example>
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<Unit> result, Func<Task<Result<TOut>>> transform) =>
        result.IsSuccess ? await transform() : result.Error!;

    /// <summary>
    /// Executes the specified asynchronous action if the current result represents a success.
    /// </summary>
    /// <example>
    /// var result = await someResult.ThenAsync(async () => await SomeAsyncOperation());
    /// </example>
    public static async Task<Result<Unit>> ThenAsync(this Result<Unit> result, Func<Task> action)
    {
        if (result.IsFailure) return result;

        await action();
        return Result<Unit>.Success(Unit.Value, result.Meta);
    }
}
