namespace FxResult.Core;

/// <summary>
/// Tracks If/ElseIf branching state for a <see cref="Result{T}"/> pipeline.
/// Ensures <c>ElseIf</c> and <c>Else</c> can only follow an <c>If</c>.
/// </summary>
/// <typeparam name="TIn">Source value type.</typeparam>
/// <typeparam name="TOut">Resulting value type after the branch action.</typeparam>
public sealed class ResultBranch<TIn, TOut>
{
    private readonly Result<TIn> _source;
    private Result<TOut>? _result;
    private bool _matched;

    internal ResultBranch(Result<TIn> source) => _source = source;

    internal void TryExecute(Func<TIn, bool> predicate, Func<TIn, Result<TOut>> action)
    {
        if (_matched || _source.IsFailure) return;
        if (!predicate(_source.Value)) return;
        _result = action(_source.Value);
        _matched = true;
    }

    internal Result<TOut> Resolve(Func<TIn, Result<TOut>> elseAction)
    {
        if (_source.IsFailure) return Result<TOut>.Fail(_source.Error, _source.Meta);
        if (_matched) return _result!.Value;
        return elseAction(_source.Value);
    }

    internal async Task<Result<TOut>> ResolveAsync(Func<TIn, Task<Result<TOut>>> elseActionAsync)
    {
        if (_source.IsFailure) return Result<TOut>.Fail(_source.Error, _source.Meta);
        if (_matched) return _result!.Value;
        return await elseActionAsync(_source.Value).ConfigureAwait(false);
    }

    internal async Task TryExecuteAsync(Func<TIn, bool> predicate, Func<TIn, Task<Result<TOut>>> actionAsync)
    {
        if (_matched || _source.IsFailure) return;
        if (!predicate(_source.Value)) return;
        _result = await actionAsync(_source.Value).ConfigureAwait(false);
        _matched = true;
    }
}
