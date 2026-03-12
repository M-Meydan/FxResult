using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Asynchronous exception-safe transformations for <see cref="Result{T}"/> and Task&lt;Result{T}&gt;.
/// </summary>
public static partial class ThenExtensions
{
    /// <summary>
    /// Async chaining — sync exception-safe transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTry<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => (await resultTask.ConfigureAwait(false)).ThenTry(transform, source, caller);

    /// <summary>
    /// Async chaining — sync exception-safe Result-returning transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTry<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => (await resultTask.ConfigureAwait(false)).ThenTry(transform, source, caller);

    /// <summary>
    /// Async chaining — async exception-safe transform on Result&lt;T&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return Result<TOut>.Fail(result.Error.WithContext(source, caller), result.Meta);

        try
        {
            var next = await transform(value!).ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }
        catch (Exception ex)
        {
            var effectiveSource =
                source ?? $"{transform.Method?.DeclaringType?.FullName}.{transform.Method?.Name}";

            return Result<TOut>.Fail(
                new Error(ex.GetType().Name, ex.Message, effectiveSource, caller, ex),
                result.Meta);
        }
    }

    /// <summary>
    /// Async chaining — async exception-safe Result-returning transform on Result&lt;T&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.TryGetValue(out var value))
            return Result<TOut>.Fail(result.Error.WithContext(source, caller), result.Meta);

        try
        {
            return await transform(value!).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var effectiveSource =
                source ?? $"{transform.Method?.DeclaringType?.FullName}.{transform.Method?.Name}";

            return Result<TOut>.Fail(
                new Error(ex.GetType().Name, ex.Message, effectiveSource, caller, ex),
                result.Meta);
        }
    }

    /// <summary>
    /// Async chaining — async exception-safe transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => await (await resultTask.ConfigureAwait(false)).ThenTryAsync(transform, source, caller).ConfigureAwait(false);

    /// <summary>
    /// Async chaining — async exception-safe Result-returning transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => await (await resultTask.ConfigureAwait(false)).ThenTryAsync(transform, source, caller).ConfigureAwait(false);
}