using FxResult.Core;

namespace FxResult.ResultExtensions;

/// <summary>Async chaining for Result{T}. Use <c>ThenTryAsync</c> for fallible operations.</summary>
public static partial class ThenExtensions
{
    /// <summary>Async transform on success. Example: <c>result.ThenAsync(x =&gt; FetchAsync(x))</c></summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> transform)
    {
        if (result.TryGetValue(out var value))
        {
            var next = await transform(value!).ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>Async Result-returning transform. Example: <c>result.ThenAsync(x =&gt; ValidateAsync(x))</c></summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> transform)
        => result.TryGetValue(out var value)
            ? await transform(value!).ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>Async transform with CancellationToken. Example: <c>result.ThenAsync((x, ct) =&gt; FetchAsync(x, ct), ct)</c></summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, CancellationToken, Task<TOut>> transform, CancellationToken cancellationToken = default)
    {
        if (result.TryGetValue(out var value))
        {
            var next = await transform(value!, cancellationToken).ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>Async Result-returning transform with CancellationToken.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, CancellationToken, Task<Result<TOut>>> transform, CancellationToken cancellationToken = default)
        => result.TryGetValue(out var value)
            ? await transform(value!, cancellationToken).ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>Awaits task, then async transform. Example: <c>await task.ThenAsync(x =&gt; FetchAsync(x))</c></summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> transform)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform).ConfigureAwait(false);

    /// <summary>Awaits task, then async Result-returning transform.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> transform)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform).ConfigureAwait(false);

    /// <summary>Awaits task, then async transform with CancellationToken.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, CancellationToken, Task<TOut>> transform, CancellationToken cancellationToken = default)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform, cancellationToken).ConfigureAwait(false);

    /// <summary>Awaits task, then async Result-returning transform with CancellationToken.</summary>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, CancellationToken, Task<Result<TOut>>> transform, CancellationToken cancellationToken = default)
        => await (await resultTask.ConfigureAwait(false)).ThenAsync(transform, cancellationToken).ConfigureAwait(false);

    /// <summary>Awaits task, then sync transform. Example: <c>await task.Then(x =&gt; x * 2)</c></summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> transform)
        => (await resultTask.ConfigureAwait(false)).Then(transform);

    /// <summary>Awaits task, then sync Result-returning transform.</summary>
    public static async Task<Result<TOut>> Then<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Result<TOut>> transform)
        => (await resultTask.ConfigureAwait(false)).Then(transform);

    /// <summary>Async transform on Result{Unit} success. Example: <c>unitResult.ThenAsync(async () =&gt; await LoadAsync())</c></summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<RUnit> result, Func<Task<TOut>> transform)
    {
        if (result.IsSuccess)
        {
            var next = await transform().ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>Async transform on Result{Unit} with CancellationToken.</summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<RUnit> result, Func<CancellationToken, Task<TOut>> transform, CancellationToken cancellationToken = default)
    {
        if (result.IsSuccess)
        {
            var next = await transform(cancellationToken).ConfigureAwait(false);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>Async Result-returning transform on Result{Unit}.</summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<RUnit> result, Func<Task<Result<TOut>>> transform)
        => result.IsSuccess
            ? await transform().ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>Async Result-returning transform on Result{Unit} with CancellationToken.</summary>
    public static async Task<Result<TOut>> ThenAsync<TOut>(this Result<RUnit> result, Func<CancellationToken, Task<Result<TOut>>> transform, CancellationToken cancellationToken = default)
        => result.IsSuccess
            ? await transform(cancellationToken).ConfigureAwait(false)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>Runs an async action on Result{Unit} success with CancellationToken.</summary>
    public static async Task<Result<RUnit>> ThenAsync(this Result<RUnit> result, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        if (result.IsFailure) return result;

        await action(cancellationToken).ConfigureAwait(false);
        return Result<RUnit>.Success(RUnit.Value, result.Meta);
    }

    /// <summary>Runs an async action on Result{Unit} success. Example: <c>unitResult.ThenAsync(async () =&gt; await SaveAsync())</c></summary>
    public static async Task<Result<RUnit>> ThenAsync(this Result<RUnit> result, Func<Task> action)
    {
        if (result.IsFailure) return result;

        await action().ConfigureAwait(false);
        return Result<RUnit>.Success(RUnit.Value, result.Meta);
    }
}
