using NUnit.Framework;
using FxResult.Core;
using FxResult.ResultExtensions;
using System;
using System.Threading.Tasks;

namespace FxResult.UnitTest.ThenTests;

[TestFixture]
public class ThenAsyncTests : ResultTestBase
{
    [Test]
    public void SyncThen_OnFailure_PropagatesError()
    {
        var initialError = new Error("INIT", "Initial error");
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
        var initialError = new Error("INIT", "Initial error");
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
        var initialError = new Error("INIT", "Initial error");
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
        var initialError = new Error("INIT", "Initial error");
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
    private Result<RUnit> PublishSideEffect(int value, List<string> log)
    {
        log.Add($"Published value: {value}");
        return RUnit.Value;
    }

    [Test]
    public async Task ThenAsync_WithCancellationToken_ShouldPassToken_ToDelegate()
    {
        var result = Result<int>.Success(2);
        using var cts = new CancellationTokenSource();
        var observed = CancellationToken.None;

        var next = await result.ThenAsync(async (x, ct) =>
        {
            await Task.Yield();
            observed = ct;
            return x + 1;
        }, cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(next.IsSuccess, Is.True);
            Assert.That(next.Value, Is.EqualTo(3));
            Assert.That(observed, Is.EqualTo(cts.Token));
        });
    }

    [Test]
    public async Task ThenAsync_WithCancellationToken_ResultReturning_PassesToken()
    {
        var result = Result<int>.Success(2);
        using var cts = new CancellationTokenSource();
        var observed = CancellationToken.None;

        var next = await result.ThenAsync(async (x, ct) =>
        {
            await Task.Yield();
            observed = ct;
            return Result<int>.Success(x + 1);
        }, cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(next.IsSuccess, Is.True);
            Assert.That(next.Value, Is.EqualTo(3));
            Assert.That(observed, Is.EqualTo(cts.Token));
        });
    }

    [Test]
    public async Task ThenAsync_WithCancellationToken_PropagatesFailure()
    {
        var result = Result<int>.Fail("fail");
        using var cts = new CancellationTokenSource();

        var next = await result.ThenAsync(async (x, ct) =>
        {
            await Task.Yield();
            return x + 1;
        }, cts.Token);

        Assert.That(next.IsFailure, Is.True);
        Assert.That(next.Error.Message, Is.EqualTo("fail"));
    }

    [Test]
    public async Task ThenAsync_Unit_FuncTask_ExecutesOnSuccess()
    {
        var result = Result<RUnit>.Success(RUnit.Value);

        var next = await result.ThenAsync(async () =>
        {
            await Task.Yield();
            return 42;
        });

        Assert.That(next.IsSuccess, Is.True);
        Assert.That(next.Value, Is.EqualTo(42));
    }

    [Test]
    public async Task ThenAsync_Unit_FuncTask_PropagatesFailure()
    {
        var result = Result<RUnit>.Fail("fail");

        var next = await result.ThenAsync(async () =>
        {
            await Task.Yield();
            return 42;
        });

        Assert.That(next.IsFailure, Is.True);
        Assert.That(next.Error.Message, Is.EqualTo("fail"));
    }

    [Test]
    public async Task ThenAsync_Unit_FuncCancellationTokenTask_PassesToken()
    {
        var result = Result<RUnit>.Success(RUnit.Value);
        using var cts = new CancellationTokenSource();
        var observed = CancellationToken.None;

        var next = await result.ThenAsync(async ct =>
        {
            await Task.Yield();
            observed = ct;
            return 7;
        }, cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(next.IsSuccess, Is.True);
            Assert.That(next.Value, Is.EqualTo(7));
            Assert.That(observed, Is.EqualTo(cts.Token));
        });
    }

    [Test]
    public async Task ThenAsync_Unit_ResultReturning_ExecutesOnSuccess()
    {
        var result = Result<RUnit>.Success(RUnit.Value);

        var next = await result.ThenAsync(async () =>
        {
            await Task.Yield();
            return Result<string>.Success("ok");
        });

        Assert.That(next.IsSuccess, Is.True);
        Assert.That(next.Value, Is.EqualTo("ok"));
    }

    [Test]
    public async Task ThenAsync_Unit_ResultReturning_PropagatesFailure()
    {
        var result = Result<RUnit>.Fail("fail");

        var next = await result.ThenAsync(async () =>
        {
            await Task.Yield();
            return Result<string>.Success("ok");
        });

        Assert.That(next.IsFailure, Is.True);
    }

    [Test]
    public async Task ThenAsync_Unit_ActionTask_ExecutesOnSuccess()
    {
        var result = Result<RUnit>.Success(RUnit.Value);
        var called = false;

        var next = await result.ThenAsync(async () =>
        {
            await Task.Yield();
            called = true;
        });

        Assert.That(called, Is.True);
        Assert.That(next.IsSuccess, Is.True);
    }

    [Test]
    public async Task ThenAsync_Unit_ActionTask_SkipsOnFailure()
    {
        var result = Result<RUnit>.Fail("fail");
        var called = false;

        var next = await result.ThenAsync(async () =>
        {
            await Task.Yield();
            called = true;
        });

        Assert.That(called, Is.False);
        Assert.That(next.IsFailure, Is.True);
    }

    [Test]
    public async Task ThenAsync_Unit_ActionCancellationTokenTask_PassesToken()
    {
        var result = Result<RUnit>.Success(RUnit.Value);
        using var cts = new CancellationTokenSource();
        var observed = CancellationToken.None;

        var next = await result.ThenAsync(async ct =>
        {
            await Task.Yield();
            observed = ct;
        }, cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(next.IsSuccess, Is.True);
            Assert.That(observed, Is.EqualTo(cts.Token));
        });
    }

    // Async Result<Unit> for logging
    private Task<Result<RUnit>> LogCompletionAsync(List<string> log)
    {
        log.Add("Completed pipeline");
        return Task.FromResult(new Result<RUnit>(RUnit.Value));
    }
}
    

