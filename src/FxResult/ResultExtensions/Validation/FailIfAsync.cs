using FxResult.Core;
using System.Runtime.CompilerServices;

namespace FxResult.ResultExtensions;

/// <summary>
/// Asynchronous invariant enforcement helpers for <see cref="Result{T}"/>.
/// </summary>
public static partial class FailIfExtensions
{
    /// <summary>
    /// Converts a <see cref="Task{TResult}"/> that may yield a nullable reference type into a <see cref="Result{T}"/>.
    /// </summary>
    public static async Task<Result<T>> FailIfNullAsync<T>(
        this Task<T?> task,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : class
    {
        var value = await task.ConfigureAwait(false);
        return value == null
            ? new Error(code, message, source, caller)
            : Result<T>.Success(value);
    }

    /// <summary>
    /// Converts a <see cref="Task{TResult}"/> that may yield a nullable value type into a <see cref="Result{T}"/>.
    /// </summary>
    public static async Task<Result<T>> FailIfNullAsync<T>(
        this Task<T?> task,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : struct
    {
        var value = await task.ConfigureAwait(false);
        return value == null
            ? new Error(code, message, source, caller)
            : Result<T>.Success(value.Value);
    }

    /// <summary>
    /// Converts an awaited <see cref="Result{T}"/> containing a nullable value type into a non-nullable <see cref="Result{T}"/>,
    /// failing when the contained value is <c>null</c>. Propagates failure (and metadata) when the input result is a failure.
    /// </summary>
    public static async Task<Result<T>> FailIfNullAsync<T>(
        this Task<Result<T?>> task,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : struct
    {
        var result = await task.ConfigureAwait(false);
        if (!result.IsSuccess) return Result<T>.Fail(result.Error, result.Meta);

        return result.Value is null
            ? Result<T>.Fail(new Error(code, message, source, caller), result.Meta)
            : Result<T>.Success(result.Value.Value, result.Meta);
    }

    /// <summary>
    /// Converts an awaited <see cref="Result{T}"/> containing a nullable reference type into a non-nullable <see cref="Result{T}"/>,
    /// failing when the contained value is <c>null</c>. Propagates failure (and metadata) when the input result is a failure.
    /// </summary>
    public static async Task<Result<T>> FailIfNullAsync<T>(
        this Task<Result<T?>> task,
        string message,
        string code = ErrorCodes.NULL_VALUE,
        string? source = null,
        [CallerMemberName] string? caller = null)
        where T : class
    {
        var result = await task.ConfigureAwait(false);
        if (!result.IsSuccess) return Result<T>.Fail(result.Error, result.Meta);

        return result.Value is null
            ? Result<T>.Fail(new Error(code, message, source, caller), result.Meta)
            : Result<T>.Success(result.Value, result.Meta);
    }

    /// <summary>
    /// Asynchronously fails when <paramref name="predicateAsync"/> returns <c>true</c> for the current value.
    /// </summary>
    public static async Task<Result<T>> FailIfAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicateAsync,
        string code,
        string message,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;

        try
        {
            if (await predicateAsync(result.Value!).ConfigureAwait(false))
                return Result<T>.Fail(new Error(code, message, source, caller), result.Meta);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new Error(ex.GetType().Name, ex.Message, source, caller, ex), result.Meta);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously fails when <paramref name="predicateAsync"/> returns <c>true</c> for the current value.
    /// Supports <see cref="CancellationToken"/> and preserves metadata on failure.
    /// </summary>
    public static async Task<Result<T>> FailIfAsync<T>(
        this Result<T> result,
        Func<T, CancellationToken, Task<bool>> predicateAsync,
        string code,
        string message,
        CancellationToken cancellationToken = default,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.IsFailure) return result;

        try
        {
            if (await predicateAsync(result.Value!, cancellationToken).ConfigureAwait(false))
                return Result<T>.Fail(new Error(code, message, source, caller), result.Meta);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new Error(ex.GetType().Name, ex.Message, source, caller, ex), result.Meta);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously fails if the provided <paramref name="conditionAsync"/> returns <c>true</c>.
    /// </summary>
    public static async Task<Result<T>> FailIfAsync<T>(
        this Result<T> result,
        Func<Task<bool>> conditionAsync,
        string code,
        string message,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;

        try
        {
            if (await conditionAsync().ConfigureAwait(false))
                return Result<T>.Fail(new Error(code, message, source, caller), result.Meta);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new Error(ex.GetType().Name, ex.Message, source, caller, ex), result.Meta);
        }

        return result;
    }

    /// <summary>
    /// Ensures that an async predicate returns <c>true</c> for the current value; otherwise returns the error produced by <paramref name="errorFactory"/>.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicateAsync,
        Func<Error> errorFactory,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (!result.IsSuccess) return result;

        try
        {
            if (await predicateAsync(result.Value!).ConfigureAwait(false))
                return result;
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new Error(ex.GetType().Name, ex.Message, source, caller, ex), result.Meta);
        }

        var err = errorFactory();
        return Result<T>.Fail(err with { Caller = err.Caller ?? caller, Source = err.Source ?? source }, result.Meta);
    }

    /// <summary>
    /// Ensures that an async predicate returns <c>true</c> for the current value; otherwise returns the error produced by <paramref name="errorFactory"/>.
    /// Supports <see cref="CancellationToken"/> and preserves metadata on failure.
    /// </summary>
    public static async Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, CancellationToken, Task<bool>> predicateAsync,
        Func<Error> errorFactory,
        CancellationToken cancellationToken = default,
        string? source = null,
        [CallerMemberName] string? caller = null)
    {
        if (result.IsFailure) return Result<T>.Fail(result.Error, result.Meta);

        try
        {
            if (await predicateAsync(result.Value!, cancellationToken).ConfigureAwait(false))
                return result;
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(new Error(ex.GetType().Name, ex.Message, source, caller, ex), result.Meta);
        }

        var err = errorFactory();
        return Result<T>.Fail(err with { Caller = err.Caller ?? caller, Source = err.Source ?? source }, result.Meta);
    }

    /// <summary>
    /// Ensures that an async predicate returns <c>true</c> for the current value; otherwise returns a failed result with the provided code and message.
    /// </summary>
    public static Task<Result<T>> EnsureAsync<T>(
        this Result<T> result,
        Func<T, Task<bool>> predicateAsync,
        string code,
        string message,
        string? source = null,
        [CallerMemberName] string? caller = null)
        => result.EnsureAsync(predicateAsync, () => new Error(code, message, source, caller), source, caller);
}
