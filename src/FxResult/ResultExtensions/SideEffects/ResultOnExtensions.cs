using FxResult.Core;
using System.Diagnostics.CodeAnalysis;

namespace FxResult.ResultExtensions.SideEffects;

/// <summary>
/// "On..." extensions for <see cref="Result{T}"/> — fluent hooks for success, failure, and finalization logic.
/// All handlers return a new result, allowing transformation and recovery.
/// <para>
/// Example usage:
/// </para>
/// <code>
/// var result = Result&lt;string&gt;.Try(() =&gt; ReadFile("path.txt"))
///     .OnFailure(res =&gt; res.Error.Exception switch
///     {
///         FileNotFoundException =&gt; Result.Success("[default content]"),
///         _ =&gt; res
///     })
///     .OnSuccess(res =&gt; Result.Success(res.Value.Trim()));
/// </code>
/// </summary>
[ExcludeFromCodeCoverage]
public static class ResultOnExtensions
{
    /// <summary>
    /// Executes a function if the result is successful. Catches exceptions and returns a new failed result.
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Func<Result<T>, Result<T>> func)
    {
        if (!result.IsSuccess) return result;

        try
        {
            return func(result);
        }
        catch (Exception ex)
        {
            return Errors.OnSuccessCallback(ex);
        }
    }

    /// <summary>
    /// Executes a function if the result is successful (async version). Catches exceptions and returns a new failed result.
    /// </summary>
    public static async Task<Result<T>> OnSuccessAsync<T>(this Result<T> result, Func<Result<T>, Task<Result<T>>> func)
    {
        if (!result.IsSuccess) return result;

        try
        {
            return await func(result).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Errors.OnSuccessCallbackAsync(ex);
        }
    }

    /// <summary>
    /// Executes a function if the result is successful (async version) with <see cref="CancellationToken"/> support.
    /// Catches exceptions and returns a new failed result.
    /// </summary>
    public static async Task<Result<T>> OnSuccessAsync<T>(
        this Result<T> result,
        Func<Result<T>, CancellationToken, Task<Result<T>>> func,
        CancellationToken cancellationToken = default)
    {
        if (!result.IsSuccess) return result;

        try
        {
            return await func(result, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Errors.OnSuccessCallbackAsync(ex);
        }
    }

    /// <summary>
    /// Executes a function if the result is a failure. Allows transforming or recovering the result.
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Func<Result<T>, Result<T>> func)
        => result.IsFailure ? func(result) : result;

    /// <summary>
    /// Executes a function if the result is a failure (async wrapper for <c>Task&lt;Result&lt;T&gt;&gt;</c>).
    /// Allows transforming or recovering the result.
    /// </summary>
    public static async Task<Result<T>> OnFailure<T>(this Task<Result<T>> resultTask, Func<Result<T>, Result<T>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.IsFailure ? func(result) : result;
    }

    /// <summary>
    /// Executes an async function if the result is a failure.
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Result<T>, Task<Result<T>>> func)
        => result.IsFailure ? await func(result).ConfigureAwait(false) : result;

    /// <summary>
    /// Executes an async function if the result is a failure (supports <see cref="CancellationToken"/>).
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(
        this Result<T> result,
        Func<Result<T>, CancellationToken, Task<Result<T>>> func,
        CancellationToken cancellationToken = default)
        => result.IsFailure ? await func(result, cancellationToken).ConfigureAwait(false) : result;

    /// <summary>
    /// Executes an async function if the resolved result is a failure (async wrapper for <c>Task&lt;Result&lt;T&gt;&gt;</c>).
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>, Task<Result<T>>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.IsFailure ? await func(result).ConfigureAwait(false) : result;
    }

    /// <summary>
    /// Executes an async function if the resolved result is a failure (supports <see cref="CancellationToken"/>).
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Result<T>, CancellationToken, Task<Result<T>>> func,
        CancellationToken cancellationToken = default)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.IsFailure ? await func(result, cancellationToken).ConfigureAwait(false) : result;
    }

    /// <summary>
    /// Executes a function regardless of result status.
    /// </summary>
    public static Result<T> OnFinally<T>(this Result<T> result, Func<Result<T>, Result<T>> func)
        => func(result);

    /// <summary>
    /// Executes a function regardless of result status (async wrapper for <c>Task&lt;Result&lt;T&gt;&gt;</c>).
    /// </summary>
    public static async Task<Result<T>> OnFinally<T>(this Task<Result<T>> resultTask, Func<Result<T>, Result<T>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return func(result);
    }

    /// <summary>
    /// Executes an async function regardless of result status.
    /// </summary>
    public static async Task<Result<T>> OnFinallyAsync<T>(this Result<T> result, Func<Result<T>, Task<Result<T>>> func)
        => await func(result).ConfigureAwait(false);

    /// <summary>
    /// Executes an async function regardless of result status (supports <see cref="CancellationToken"/>).
    /// </summary>
    public static async Task<Result<T>> OnFinallyAsync<T>(
        this Result<T> result,
        Func<Result<T>, CancellationToken, Task<Result<T>>> func,
        CancellationToken cancellationToken = default)
        => await func(result, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Executes an async function regardless of result status (async wrapper for <c>Task&lt;Result&lt;T&gt;&gt;</c>).
    /// </summary>
    public static async Task<Result<T>> OnFinallyAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>, Task<Result<T>>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await func(result).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an async function regardless of result status (supports <see cref="CancellationToken"/>).
    /// </summary>
    public static async Task<Result<T>> OnFinallyAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Result<T>, CancellationToken, Task<Result<T>>> func,
        CancellationToken cancellationToken = default)
    {
        var result = await resultTask.ConfigureAwait(false);
        return await func(result, cancellationToken).ConfigureAwait(false);
    }
}
