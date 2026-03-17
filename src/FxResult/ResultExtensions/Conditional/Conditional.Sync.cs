using FxResult.Core;
using System.Diagnostics.CodeAnalysis;

namespace FxResult.ResultExtensions;

/// <summary>
/// Conditional branching: <c>.If().ElseIf().Else()</c> chains on Result{T}.
/// Example: <c>result.If(x =&gt; x &gt; 100, Classify).ElseIf(x =&gt; x &gt; 50, Label).Else(Default)</c>
/// </summary>
[ExcludeFromCodeCoverage]
public static partial class ConditionalExtensions
{
    /// <summary>Starts a conditional branch. Example: <c>result.If(x =&gt; x &gt; 100, x =&gt; Result.Success("high"))</c></summary>
    public static ResultBranch<TIn, TOut> If<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, bool> predicate,
        Func<TIn, Result<TOut>> action)
    {
        var branch = new ResultBranch<TIn, TOut>(result);
        branch.TryExecute(predicate, action);
        return branch;
    }

    /// <summary>Adds a condition to the branch chain. Example: <c>.ElseIf(x =&gt; x &gt; 50, Format)</c></summary>
    public static ResultBranch<TIn, TOut> ElseIf<TIn, TOut>(
        this ResultBranch<TIn, TOut> branch,
        Func<TIn, bool> predicate,
        Func<TIn, Result<TOut>> action)
    {
        branch.TryExecute(predicate, action);
        return branch;
    }

    /// <summary>Default branch when nothing matched. Example: <c>.Else(x =&gt; Result.Success("default"))</c></summary>
    public static Result<TOut> Else<TIn, TOut>(
        this ResultBranch<TIn, TOut> branch,
        Func<TIn, Result<TOut>> elseAction)
        => branch.Resolve(elseAction);
}
