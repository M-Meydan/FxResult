using FxResults.Core;
using System.Runtime.CompilerServices;

namespace FxResults.Extensions;

/// <summary>
/// Synchronous chaining transformations for Result{T}.
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
    /// <example>
    /// var r2 = r1.Then(x => x.Length);
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> transform, string? source = null, [CallerMemberName] string caller = "")
        => result.TryGetValue(out var value) ? transform(value!) : result.Error!.WithContext(source, caller);

    /// <summary>
    /// Chains another Result-returning transformation.
    /// <example>
    /// var r2 = r1.Then(x => ParseResult(x));
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> transform, string? source = null, [CallerMemberName] string caller = "")
        => result.TryGetValue(out var value) ? transform(value!) : result.Error!.WithContext(source, caller);

    /// <summary>
    /// Chains a transformation and exposes the transformed result via an out parameter.
    /// <example>
    /// var r2 = r1.Then(x => x.Length, out var stepResult);
    /// // r2 and stepResult both contain the Result<int> for the length
    /// </example>
    /// </summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result,Func<TIn, TOut> transform,out Result<TOut> stepResult, string? source = null, [CallerMemberName] string caller = "")
    {
        stepResult = result.Then(transform, source, caller);
        return stepResult;
    }
}