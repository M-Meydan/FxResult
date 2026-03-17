using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>Async exception-safe transforms. Catches exceptions and returns Error.</summary>
public static partial class ThenExtensions
{
    /// <summary>Awaits task, then sync exception-safe transform. Example: <c>await task.ThenTry(int.Parse)</c></summary>
    public static async Task<Result<TOut>> ThenTry<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => (await resultTask.ConfigureAwait(false)).ThenTry(transform, source, caller);

    /// <summary>Awaits task, then sync exception-safe Result-returning transform.</summary>
    public static async Task<Result<TOut>> ThenTry<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => (await resultTask.ConfigureAwait(false)).ThenTry(transform, source, caller);

    /// <summary>Async exception-safe transform. Example: <c>result.ThenTryAsync(x =&gt; FetchAsync(x))</c></summary>
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

    /// <summary>Async exception-safe Result-returning transform.</summary>
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

    /// <summary>Awaits task, then async exception-safe transform.</summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<TOut>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => await (await resultTask.ConfigureAwait(false)).ThenTryAsync(transform, source, caller).ConfigureAwait(false);

    /// <summary>Awaits task, then async exception-safe Result-returning transform.</summary>
    public static async Task<Result<TOut>> ThenTryAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> transform,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => await (await resultTask.ConfigureAwait(false)).ThenTryAsync(transform, source, caller).ConfigureAwait(false);
}