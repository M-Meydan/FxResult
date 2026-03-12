using System.Diagnostics.CodeAnalysis;
using FxResult.Core;

namespace FxResult.ResultExtensions;

/// <summary>
/// Synchronous chaining transformations for <see cref="Result{T}"/>.
/// These methods are pure and do not include exception handling — use <c>ThenTry</c> for fallible operations.
/// </summary>
/// <remarks>
/// Examples:
/// <code>
/// var res2 = res1.Then(x => x.Length);
/// var res3 = res2.Then(x => Parse(x));
/// var res4 = res3.Then(x => x * 2, out var original);
/// // res4: transformed result, original: previous result
/// </code>
/// </remarks>
[ExcludeFromCodeCoverage]
public static partial class ThenExtensions
{
    /// <summary>
    /// Chains a transformation if the result is successful.
    /// This method does not catch exceptions — use <c>ThenTry</c> for fallible logic.
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> transform)
    {
        if (result.TryGetValue(out var value))
        {
            var next = transform(value!);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>
    /// Chains another <see cref="Result{TOut}"/>-returning transformation.
    /// This method does not catch exceptions — use <c>ThenTry</c> for fallible logic.
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> transform)
        => result.TryGetValue(out var value)
            ? transform(value!)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>
    /// Chains a transformation and exposes the transformed result via an <c>out</c> parameter.
    /// This method does not catch exceptions — use <c>ThenTry</c> for fallible logic.
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> transform, out Result<TOut> stepResult)
    {
        stepResult = result.Then(transform);
        return stepResult;
    }

    /// <summary>
    /// Chains a transformation if the <see cref="Result{Unit}"/> is successful.
    /// This method does not catch exceptions — use <c>ThenTry</c> for fallible logic.
    /// </summary>
    public static Result<TOut> Then<TOut>(this Result<Unit> result, Func<TOut> transform)
    {
        if (result.IsSuccess)
        {
            return Result<TOut>.Success(transform(), result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>
    /// Chains another <see cref="Result{TOut}"/>-returning transformation if the <see cref="Result{Unit}"/> is successful.
    /// This method does not catch exceptions — use <c>ThenTry</c> for fallible logic.
    /// </summary>
    public static Result<TOut> Then<TOut>(this Result<Unit> result, Func<Result<TOut>> transform)
        => result.IsSuccess
            ? transform()
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>
    /// Chains an action when the input <see cref="Result{Unit}"/> is successful.
    /// Use for void-style continuations. Exceptions thrown by <paramref name="action"/> are not caught by this method.
    /// </summary>
    public static Result<Unit> Then(this Result<Unit> result, Action action)
    {
        if (result.IsFailure) return result;

        action();
        return Result<Unit>.Success(Unit.Value, result.Meta);
    }
}
