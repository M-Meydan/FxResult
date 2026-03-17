using System.Diagnostics.CodeAnalysis;
using FxResult.Core;

namespace FxResult.ResultExtensions;

/// <summary>Sync chaining for Result{T}. Use <c>ThenTry</c> for fallible operations.</summary>
[ExcludeFromCodeCoverage]
public static partial class ThenExtensions
{
    /// <summary>Transforms the value if successful. Example: <c>result.Then(x =&gt; x * 2)</c></summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> transform)
    {
        if (result.TryGetValue(out var value))
        {
            var next = transform(value!);
            return Result<TOut>.Success(next, result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>Chains a Result-returning transform. Example: <c>result.Then(x =&gt; Validate(x))</c></summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> transform)
        => result.TryGetValue(out var value)
            ? transform(value!)
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>Transforms and captures via out. Example: <c>result.Then(x =&gt; x * 2, out var prev)</c></summary>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> transform, out Result<TOut> stepResult)
    {
        stepResult = result.Then(transform);
        return stepResult;
    }

    /// <summary>Chains a transform on Result{Unit}. Example: <c>unitResult.Then(() =&gt; 42)</c></summary>
    public static Result<TOut> Then<TOut>(this Result<RUnit> result, Func<TOut> transform)
    {
        if (result.IsSuccess)
        {
            return Result<TOut>.Success(transform(), result.Meta);
        }

        return Result<TOut>.Fail(result.Error, result.Meta);
    }

    /// <summary>Chains a Result-returning transform on Result{Unit}. Example: <c>unitResult.Then(() =&gt; LoadData())</c></summary>
    public static Result<TOut> Then<TOut>(this Result<RUnit> result, Func<Result<TOut>> transform)
        => result.IsSuccess
            ? transform()
            : Result<TOut>.Fail(result.Error, result.Meta);

    /// <summary>Runs an action on Result{Unit} success. Example: <c>unitResult.Then(() =&gt; Save())</c></summary>
    public static Result<RUnit> Then(this Result<RUnit> result, Action action)
    {
        if (result.IsFailure) return result;

        action();
        return Result<RUnit>.Success(RUnit.Value, result.Meta);
    }
}
