using FxResult.Core;

namespace FxResult.ResultExtensions;

public static partial class ConditionalExtensions
{
    /// <summary>Awaits task, then starts a conditional branch.</summary>
    public static async Task<ResultBranch<TIn, TOut>> If<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, bool> predicate,
        Func<TIn, Result<TOut>> action)
        => (await resultTask.ConfigureAwait(false)).If(predicate, action);

    /// <summary>Awaits branch task, then adds a condition.</summary>
    public static async Task<ResultBranch<TIn, TOut>> ElseIf<TIn, TOut>(
        this Task<ResultBranch<TIn, TOut>> branchTask,
        Func<TIn, bool> predicate,
        Func<TIn, Result<TOut>> action)
    {
        var branch = await branchTask.ConfigureAwait(false);
        branch.TryExecute(predicate, action);
        return branch;
    }

    /// <summary>Awaits branch task, then resolves with default.</summary>
    public static async Task<Result<TOut>> Else<TIn, TOut>(
        this Task<ResultBranch<TIn, TOut>> branchTask,
        Func<TIn, Result<TOut>> elseAction)
        => (await branchTask.ConfigureAwait(false)).Resolve(elseAction);
 
    /// <summary>Starts a conditional branch with async action. Example: <c>result.IfAsync(x =&gt; x &gt; 100, FetchAsync)</c></summary>
    public static async Task<ResultBranch<TIn, TOut>> IfAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, bool> predicate,
        Func<TIn, Task<Result<TOut>>> actionAsync)
    {
        var branch = new ResultBranch<TIn, TOut>(result);
        await branch.TryExecuteAsync(predicate, actionAsync).ConfigureAwait(false);
        return branch;
    }

    /// <summary>Adds async condition to branch. Example: <c>.ElseIfAsync(x =&gt; x &gt; 50, LookupAsync)</c></summary>
    public static async Task<ResultBranch<TIn, TOut>> ElseIfAsync<TIn, TOut>(
        this ResultBranch<TIn, TOut> branch,
        Func<TIn, bool> predicate,
        Func<TIn, Task<Result<TOut>>> actionAsync)
    {
        await branch.TryExecuteAsync(predicate, actionAsync).ConfigureAwait(false);
        return branch;
    }

    /// <summary>Awaits branch task, then adds async condition.</summary>
    public static async Task<ResultBranch<TIn, TOut>> ElseIfAsync<TIn, TOut>(
        this Task<ResultBranch<TIn, TOut>> branchTask,
        Func<TIn, bool> predicate,
        Func<TIn, Task<Result<TOut>>> actionAsync)
    {
        var branch = await branchTask.ConfigureAwait(false);
        await branch.TryExecuteAsync(predicate, actionAsync).ConfigureAwait(false);
        return branch;
    }

    /// <summary>Async default branch. Example: <c>.ElseAsync(FallbackAsync)</c></summary>
    public static async Task<Result<TOut>> ElseAsync<TIn, TOut>(
        this ResultBranch<TIn, TOut> branch,
        Func<TIn, Task<Result<TOut>>> elseActionAsync)
        => await branch.ResolveAsync(elseActionAsync).ConfigureAwait(false);

    /// <summary>Awaits branch task, then resolves with async default.</summary>
    public static async Task<Result<TOut>> ElseAsync<TIn, TOut>(
        this Task<ResultBranch<TIn, TOut>> branchTask,
        Func<TIn, Task<Result<TOut>>> elseActionAsync)
        => await (await branchTask.ConfigureAwait(false)).ResolveAsync(elseActionAsync).ConfigureAwait(false);
}
