using NUnit.Framework;
using FxResults.Core;
using FxResults.ResultExtensions;
using System;
using System.Threading.Tasks;

namespace FxResults.UnitTest.ThenTests;

[TestFixture]
public class ThenAsyncTests : ResultTestBase
{
    [Test]
    public void SyncThen_OnFailure_PropagatesError()
    {
        var initialError = new Error("Initial error");
        var result = Result<int>.Fail(initialError).Then(x => x * 2);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error!.Message, Is.EqualTo(initialError.Message));
        });
    }

    [Test]
    public async Task AsyncThen_OnFailure_PropagatesError()
    {
        var initialError = new Error("Initial error");
        var result = await Result<int>.Fail(initialError)
            .ThenAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error!.Message, Is.EqualTo(initialError.Message));
        });
    }

    [Test]
    public void SyncThen_ResultReturningTransform_OnSuccess()
    {
        var result = Result<int>.Success(5)
            .Then(x => Result<int>.Success(x * 2));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(10));
        });
    }

    [Test]
    public void SyncThen_ResultReturningTransform_OnFailure_PropagatesError()
    {
        var initialError = new Error("Initial error");
        var result = Result<int>.Fail(initialError)
            .Then(x => Result<int>.Success(x * 2));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error!.Message, Is.EqualTo(initialError.Message));
            Assert.That(result.Error.Source, Is.Null);
        });
    }

    [Test]
    public void SyncThen_WithOutParameter_CapturesResult()
    {
        var result = Result<int>.Success(5)
            .Then(x => x * 2, out var capturedResult);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(10));
            Assert.That(capturedResult.IsSuccess, Is.True);
            Assert.That(capturedResult.Value, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task AsyncThen_TaskResultReturningTransform_OnSuccess()
    {
        var result = await Result<int>.Success(5)
            .ThenAsync(async x => { await Task.Delay(10); return Result<int>.Success(x * 2); });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task TaskThenAsync_TaskResultReturningTransform_OnSuccess()
    {
        var result = await AsTask(Result<int>.Success(5))
            .ThenAsync(async x =>
            {
                await Task.Delay(10);
                return Result<int>.Success(x * 2);
            });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(10));
        });
    }

    [Test]
    public async Task TaskThenAsync_TaskResultReturningTransform_OnFailure_PropagatesError()
    {
        var initialError = new Error("Initial error");
        var result = await AsTask(Result<int>.Fail(initialError))
            .ThenAsync(async x =>
            {
                await Task.Delay(10);
                return Result<int>.Success(x * 2);
            });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error!.Message, Is.EqualTo(initialError.Message));
        });
    }

    [Test]
    public async Task AsyncThen_ErrorEnrichment_WithSourceAndCaller()
    {
        // Arrange
        var result = await Result<int>.Success(0)
            .ThenTryAsync(async x =>
            {
                await Task.Delay(10);
                return 10 / x; // forces divide-by-zero exception
            }, source: "MyService", caller: "MyMethod");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error!.Source, Is.EqualTo("MyService"));
            Assert.That(result.Error!.Caller, Is.EqualTo("MyMethod"));
            Assert.That(result.Error!.Exception, Is.TypeOf<DivideByZeroException>());
        });
    }

    [Test]
    public async Task ThenAsync_NestedExceptionHandling_StopsExecutionAndPropagatesError()
    {
        var result = await Result<int>.Success(1)
            .ThenTryAsync(async x => await Task.FromException<int>(new DivideByZeroException("divide by zero")))
            .ThenAsync(x => Task.FromResult(x * 10));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Exception, Is.TypeOf<DivideByZeroException>());
        Assert.That(result.Error.Message, Is.EqualTo("divide by zero"));
    }

    [Test]
    public async Task Then_MixedResultAndUnitFlow_ExecutesCorrectly()
    {
        var log = new List<string>();

        var result = await Result<string>.Success("42")
            .Then(int.Parse, out var parsedResult) // Result<int>
            .Then(x => x * 2, out var doubledResult) // Result<int>
            .Then(x => PublishSideEffect(x, log)) // Result<Unit>
            .ThenAsync(_ => LogCompletionAsync(log)); // Result<Unit>

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(log.Count, Is.EqualTo(2));
        Assert.That(log[0], Is.EqualTo("Published value: 84"));
        Assert.That(log[1], Is.EqualTo("Completed pipeline"));

        Assert.That(parsedResult.IsSuccess, Is.True);
        Assert.That(parsedResult.Value, Is.EqualTo(42));
        Assert.That(doubledResult.IsSuccess, Is.True);
        Assert.That(doubledResult.Value, Is.EqualTo(84));
    }

    // Helper that returns Result<Unit>
    private Result<Unit> PublishSideEffect(int value, List<string> log)
    {
        log.Add($"Published value: {value}");
        return Unit.Value;
    }

    // Async Result<Unit> for logging
    private Task<Result<Unit>> LogCompletionAsync(List<string> log)
    {
        log.Add("Completed pipeline");
        return Task.FromResult(new Result<Unit>(Unit.Value));
    }
}
    

