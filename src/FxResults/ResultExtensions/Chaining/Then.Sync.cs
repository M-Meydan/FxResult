using FxResults.Core;

namespace FxResults.ResultExtensions;

/// <summary>
/// Synchronous chaining transformations for Result{T}.
/// These methods are pure and do not include exception handling — use <c>ThenTry</c> for fallible operations.
/// <para>
/// Examples:
/// <code>
/// var res2 = res1.Then(x => x.Length);
/// var res3 = res2.Then(x => Parse(x));
/// var res4 = res3.Then(x => x * 2, out var original);
/// // res4: transformed result, original: previous result
/// </code>
/// </para>
/// </summary>
public static partial class ThenExtensions
{
    /// <summary>
    /// Chains a transformation if the result is successful.
    /// This method does not catch exceptions — use ThenTry for fallible logic.
    /// <example>
    /// var r2 = r1.Then(x => x.Length);
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result,Func<TIn, TOut> transform)
        => result.TryGetValue(out var value) ? transform(value!) : result.Error!;

    /// <summary>
    /// Chains another Result-returning transformation.
    /// This method does not catch exceptions — use ThenTry for fallible logic.
    /// <example>
    /// var r2 = r1.Then(x => ParseResult(x));
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result,Func<TIn, Result<TOut>> transform)
        => result.TryGetValue(out var value) ? transform(value!) : result.Error!;

    /// <summary>
    /// Chains a transformation and exposes the transformed result via an out parameter.
    /// This method does not catch exceptions — use ThenTry for fallible logic.
    /// <example>
    /// var r2 = r1.Then(x => x.Length, out var stepResult);
    /// // r2 and stepResult both contain the Result<int> for the length
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result,Func<TIn, TOut> transform,out Result<TOut> stepResult)
    {
        stepResult = result.Then(transform);
        return stepResult;
    }

    /// <summary>
    /// Chains a transformation if the Result<Unit> is successful.
    /// This method does not catch exceptions — use ThenTry for fallible logic.
    /// <example>
    /// var r2 = r1.Then(() => "Hello");
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TOut>(this Result<Unit> result, Func<TOut> transform)
        => result.IsSuccess ? transform() : result.Error!;

    /// <summary>
    /// Chains another Result-returning transformation if the Result<Unit> is successful.
    /// This method does not catch exceptions — use ThenTry for fallible logic.
    /// <example>
    /// var r2 = r1.Then(() => Result.Success("Hello"));
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TOut>(this Result<Unit> result, Func<Result<TOut>> transform)
        => result.IsSuccess ? transform() : result.Error!;

    public static Result<Unit> Then(this Result<Unit> result, Action action)
    {
        if (result.IsFailure) return result;

        action();
        return Result<Unit>.Success(Unit.Value, result.Meta);
    }
}
