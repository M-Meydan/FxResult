using FxResult.Core;

namespace FxResult.ResultExtensions;

/// <summary>
/// Asynchronous chaining transformations for Result{T} and Task&lt;Result{T}&gt;.
/// These methods assume pure (non-throwing) delegates. Use ThenTryAsync for fallible operations.
/// </summary>
public static partial class ThenExtensions
{
    /// <summary>
    /// Chains a pure async transformation on a successful Result.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> transform)
    {
        if (result.TryGetValue(out var value))
        {
            var next = await transform(value!).ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>
    /// Chains a pure async transformation returning Result&lt;TOut&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> transform)
        => result.TryGetValue(out var value)
            ? await transform(value!).ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>
    /// Chains a pure async transformation on a successful Result with CancellationToken support.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, CancellationToken, Task<TOut>> transform, CancellationToken cancellationToken = default)
    {
        if (result.TryGetValue(out var value))
        {
            var next = await transform(value!, cancellationToken).ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>
    /// Chains a pure async transformation returning Result&lt;TOut&gt; with CancellationToken support.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, CancellationToken, Task<Result<TOut>>> transform, CancellationToken cancellationToken = default)
        => result.TryGetValue(out var value)
            ? await transform(value!, cancellationToken).ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>
    /// Async chaining — pure async transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> transform)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform).ConfigureAwait(false);

    /// <summary>
    /// Async chaining — Result-returning async transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> transform)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform).ConfigureAwait(false);

    /// <summary>
    /// Async chaining — pure async transform on Task&lt;Result&lt;T&gt;&gt; with CancellationToken support.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, CancellationToken, Task<TOut>> transform, CancellationToken cancellationToken = default)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Async chaining — Result-returning async transform on Task&lt;Result&lt;T&gt;&gt; with CancellationToken support.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, CancellationToken, Task<Result<TOut>>> transform, CancellationToken cancellationToken = default)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Async chaining — sync transform on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> transform)
        => (await resultTask.ConfigureAwait(false)).Then(transform);

    /// <summary>
    /// Async chaining — sync transform returning Result&lt;TOut&gt; on Task&lt;Result&lt;T&gt;&gt;.
    /// </summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Result<TOut>> transform)
        => (await resultTask.ConfigureAwait(false)).Then(transform);

    /// <summary>
    /// Chains a pure async transformation if the Result&lt;Unit&gt; is successful.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<Unit> result, Func<Task<TOut>> transform)
    {
        if (result.IsSuccess)
        {
            var next = await transform().ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>
    /// Chains a pure async transformation if the Result&lt;Unit&gt; is successful (CancellationToken support).
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<Unit> result, Func<CancellationToken, Task<TOut>> transform, CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
        {
            var next = await transform(cancellationToken).ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>
    /// Chains a pure async transformation returning Result&lt;TOut&gt; if the Result&lt;Unit&gt; is successful.
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<Unit> result, Func<Task<Result<TOut>>> transform)
        => result.IsSuccess
            ? await transform().ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>
    /// Chains a pure async transformation returning Result&lt;TOut&gt; if the Result&lt;Unit&gt; is successful (CancellationToken support).
    /// </summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<Unit> result, Func<CancellationToken, Task<Result<TOut>>> transform, CancellationToken cancellationToken = default)
        => result.IsSuccess
            ? await transform(cancellationToken).ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>
    /// Executes the specified asynchronous action if the current result represents a success (CancellationToken support).
    /// </summary>
    public static async Task<Result<Unit>> ThenAsync(this Result<Unit> result, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        if (result.IsFailure) return result;

        await action(cancellationToken).ConfigureAwait(false);
        return Result<Unit>.Success(Unit.Value, result.Meta);
    }

    /// <summary>
    /// Executes the specified asynchronous action if the current result represents success.
    /// </summary>
    public static async Task<Result<Unit>> ThenAsync(this Result<Unit> result, Func<Task> action)
    {
        if (result.IsFailure) return result;

        await action().ConfigureAwait(false);
        return Result<Unit>.Success(Unit.Value, result.Meta);
    }
}
