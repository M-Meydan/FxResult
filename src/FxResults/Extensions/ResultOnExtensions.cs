using FxResults.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FxResults.Extensions;

/// <summary>
/// "On..." extensions for Result{T}—side-effect hooks for success, failure, and finally logic. 
/// Can be used for logging, notifications, or cleanup actions. 
/// Catches exceptions in actions and returns an Error result.
/// <para>
/// Examples:
/// <code>
/// var result = Result.Success(42)
///     .OnSuccess(x => Console.WriteLine(x))
///     .OnFailure(err => Console.WriteLine(err.Message))
///     .OnFinally(r => Console.WriteLine(r.IsSuccess));
/// </code>
/// </para>
/// </summary>
public static class ResultOnExtensions
{
    /// <summary>
    /// Executes a synchronous action if the result is successful. Catches exceptions and returns an Error result.
    /// <example>
    /// var newResult = result.OnSuccess(v => DoSomething(v));
    /// </example>
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action, [CallerMemberName] string? caller = null)
    {
        if (result.TryGetValue(out var value))
        {
            try { action(value!); }
            catch (Exception ex)
            {
                return new Error(ex.Message, "ON_SUCCESS_ERROR", Exception: ex).WithContext(caller ?? nameof(OnSuccess));
            }
        }
        return result;
    }

    /// <summary>
    /// Executes an async action if the result is successful. Catches exceptions and returns an Error result.
    /// <example>
    /// var newResult = await result.OnSuccessAsync(async v => await DoAsync(v));
    /// </example>
    /// </summary>
    public static async Task<Result<T>> OnSuccessAsync<T>(this Result<T> result, Func<T, Task> action, [CallerMemberName] string? caller = null)
    {
        if (result.TryGetValue(out var value))
        {
            try { await action(value!); }
            catch (Exception ex)
            {
                return new Error(ex.Message, "ON_SUCCESS_ASYNC_ERROR", Exception: ex).WithContext(caller ?? nameof(OnSuccessAsync));
            }
        }
        return result;
    }

    /// <summary>
    /// Executes a synchronous action if the result is a failure.
    /// <example>
    /// var newResult = result.OnFailure(err => Log(err.Message));
    /// </example>
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (!result.IsSuccess)
            action(result.Error!);
        return result;
    }

    /// <summary>
    /// Executes an async action if the result is a failure.
    /// <example>
    /// var newResult = await result.OnFailureAsync(async err => await LogAsync(err.Message));
    /// </example>
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Error, Task> action)
    {
        if (!result.IsSuccess)
            await action(result.Error!);
        return result;
    }

    /// <summary>
    /// Executes a synchronous action after the result, regardless of outcome.
    /// <example>
    /// var newResult = result.OnFinally(res => Log(res.IsSuccess));
    /// </example>
    /// </summary>
    public static Result<T> OnFinally<T>(this Result<T> result, Action<Result<T>> action)
    {
        action(result);
        return result;
    }

    /// <summary>
    /// Executes an async action after the result, regardless of outcome.
    /// <example>
    /// var newResult = await result.OnFinallyAsync(async res => await LogAsync(res.IsSuccess));
    /// </example>
    /// </summary>
    public static async Task<Result<T>> OnFinallyAsync<T>(this Result<T> result, Func<Result<T>, Task> action)
    {
        await action(result);
        return result;
    }
}
