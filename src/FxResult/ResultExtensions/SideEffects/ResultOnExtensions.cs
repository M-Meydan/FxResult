using FxResult.Core;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FxResult.ResultExtensions
{
    /// <summary>
    /// "On..." extensions for Result{T} — fluent hooks for success, failure, and finalization logic.
    /// All handlers return a new Result, allowing transformation and recovery.
    /// 
    /// Example usage:
    /// <code>
    /// var result = Result<string>.Try(() => ReadFile("path.txt"))
    ///     .OnFailure(res => res.Error.Exception switch
    ///     {
    ///         FileNotFoundException => Result.Success("[default content]"),
    ///         _ => res
    ///     })
    ///     .OnSuccess(res => Result.Success(res.Value.Trim()));
    /// </code>
    /// </summary>
    public static class ResultOnExtensions
    {
        /// <summary>
        /// Executes a function if the result is successful. Catches exceptions and returns a new failed result.
        /// Example:
        /// <code>
        /// var updated = result.OnSuccess(res => Result.Success(res.Value.ToUpper()));
        /// </code>
        /// </summary>
        public static Result<T> OnSuccess<T>(this Result<T> result, Func<Result<T>, Result<T>> func)
        {
            if (result.IsSuccess)
            {
                try { return func(result); }
                catch (Exception ex)
                {
                    return new Error(ex.Message, "ON_SUCCESS_ERROR", Exception: ex);
                }
            }
            return result;
        }

        /// <summary>
        /// Executes a function if the result is successful (async version) and catches exceptions, returning a new failed result.
        /// Example:
        /// <code>
        /// var updated = await result.OnSuccessAsync(async res => Result.Success(await TransformAsync(res.Value)));
        /// </code>
        /// </summary>
        public static async Task<Result<T>> OnSuccessAsync<T>(this Result<T> result, Func<Result<T>, Task<Result<T>>> func)
        {
            if (result.IsSuccess)
            {
                try { return await func(result); }
                catch (Exception ex)
                {
                    return new Error(ex.Message, "ON_SUCCESS_ASYNC_ERROR", Exception: ex);
                }
            }
            return result;
        }

        /// <summary>
        /// Executes a function if the result is a failure. Allows transforming or recovering the result.
        /// Example:
        /// <code>
        /// var recovered = result.OnFailure(res => Result.Success("fallback"));
        /// </code>
        /// </summary>
        public static Result<T> OnFailure<T>(this Result<T> result, Func<Result<T>, Result<T>> func)
        {
            return result.IsFailure ? func(result) : result;
        }

        /// <summary>
        /// Executes a function if the result is a failure (async version). Allows transforming or recovering the result. 
        /// </summary>
        public static async Task<Result<T>> OnFailure<T>(this Task<Result<T>> resultTask, Func<Result<T>, Result<T>> func)
        {
            var result = await resultTask;
            return result.IsFailure ? func(result) : result;
        }

        
        /// <summary>
        /// Executes an async function if the result is a failure.
        /// Example:
        /// <code>
        /// var recovered = await result.OnFailureAsync(async res => Result.Success(await GetFallbackAsync()));
        /// </code>
        /// </summary>
        public static async Task<Result<T>> OnFailureAsync<T>(this Result<T> result, Func<Result<T>, Task<Result<T>>> func)
        {
            return result.IsFailure ? await func(result) : result;
        }

        /// <summary>
        /// Executes an async function if the result is a failure (async version). 
        /// </summary>
        public static async Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>, Task<Result<T>>> func)
        {
            var result = await resultTask;
            return result.IsFailure ? await func(result) : result;
        }


        /// <summary>
        /// Executes a function regardless of result status.
        /// Example:
        /// <code>
        /// var finalized = result.OnFinally(res =>
        /// {
        ///     Log(res.IsSuccess);
        ///     return res;
        /// });
        /// </code>
        /// </summary>
        public static Result<T> OnFinally<T>(this Result<T> result, Func<Result<T>, Result<T>> func)
        {
            return func(result);
        }

        /// <summary>
        /// Executes a function regardless of result status (async version).
        /// Example:
        /// </summary>
        public static async Task<Result<T>> OnFinally<T>(this Task<Result<T>> resultTask, Func<Result<T>, Result<T>> func)
        {
            var result = await resultTask;
            return func(result);
        }

        /// <summary>
        /// Executes an async function regardless of result status.
        /// Example:
        /// <code>
        /// var finalized = await result.OnFinallyAsync(async res =>
        /// {
        ///     await LogAsync(res);
        ///     return res;
        /// });
        /// </code>
        /// </summary>
        public static async Task<Result<T>> OnFinallyAsync<T>(this Result<T> result, Func<Result<T>, Task<Result<T>>> func)
        {
            return await func(result);
        }

        /// <summary>
        /// Executes an async function regardless of result status (async version).
        /// </summary>
        public static async Task<Result<T>> OnFinallyAsync<T>(this Task<Result<T>> resultTask, Func<Result<T>, Task<Result<T>>> func)
        {
            var result = await resultTask;
            return await func(result);
        }
    }
}
