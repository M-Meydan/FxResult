using FxResults.Core;
using FxResults.Extensions;
using FxResults.Extensions.FailIf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FxResults.UnitTest.ThenTests;

/// <summary>
/// Unit tests for ThenExtensions (Then, ThenTry, ThenAsync, multi-capture, and chaining).
/// </summary>
[TestFixture]
public class ThenExtensionsTests
{
    // Helpers for exception tests
    private int ThrowInvalidOperationException()
        => throw new InvalidOperationException("oops");

    private int ThrowApplicationException()
        => throw new ApplicationException("bad");

    private Task<int> ThrowInvalidOperationExceptionAsync()
        => Task.FromException<int>(new InvalidOperationException("async fail"));

    private Task<int> ThrowApplicationExceptionAsync()
        => Task.FromException<int>(new ApplicationException("async fail"));

    // Helper to simulate async
    private static Task<T> AsTask<T>(T value) => Task.FromResult(value);

    // 1. Then (sync)

    [Test]
    public void Then_Sync_AppliesTransform_OnSuccess()
    {
        var result = Result.Success(2)
            .Then(x => x * 3);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(6));
    }

    [Test]
    public void Then_Sync_DoesNotApply_OnFailure()
    {
        var result = Result.Fail<int>("fail")
            .Then(x => x * 3);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail"));
    }

    [Test]
    public void Then_ResultReturning_AppliesTransform_OnSuccess()
    {
        var result = Result.Success(2)
            .Then(x => Result.Success(x * 5));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(10));
    }

    [Test]
    public void Then_ResultReturning_DoesNotApply_OnFailure()
    {
        var result = Result.Fail<int>("fail")
            .Then(x => Result.Success(x * 5));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail"));
    }

    // 2. ThenTry (sync, exception safety)

    [Test]
    public void ThenTry_CatchesException_AndReturnsFailure()
    {
        var result = Result.Success(3)
            .ThenTry(x => ThrowInvalidOperationException());

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("oops"));
        Assert.That(result.Error!.Code, Is.EqualTo("InvalidOperationException"));
    }

    [Test]
    public void ThenTry_ResultReturning_CatchesException_AndReturnsFailure()
    {
        var result = Result.Success(3)
            .ThenTry(x => ThrowApplicationException());

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("bad"));
        Assert.That(result.Error!.Code, Is.EqualTo("ApplicationException"));
    }

    [Test]
    public void ThenTry_DoesNotApply_OnFailure()
    {
        var result = Result.Fail<int>("fail")
            .ThenTry(x => x * 2);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail"));
    }

    // 3. ThenAsync

    [Test]
    public async Task ThenAsync_AppliesTransform_OnSuccess()
    {
        var result = await AsTask(Result.Success(5))
            .ThenAsync(x => Task.FromResult(x + 10));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(15));
    }

    [Test]
    public async Task ThenAsync_DoesNotApply_OnFailure()
    {
        var result = await AsTask(Result.Fail<int>("fail"))
            .ThenAsync(x => Task.FromResult(x + 10)); 

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail"));
    }

    [Test]
    public async Task ThenAsync_ResultReturning_AppliesTransform_OnSuccess()
    {
        var result = await AsTask(Result.Success(7))
            .ThenAsync(x => Task.FromResult(Result.Success(x * 2)));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(14));
    }

    [Test]
    public async Task ThenAsync_ResultReturning_DoesNotApply_OnFailure()
    {
        var result = await AsTask(Result.Fail<int>("fail"))
            .ThenAsync(x => Task.FromResult(Result.Success(x * 2)));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail"));
    }

    [Test]
    public async Task ThenAsync_PropagatesError()
    {
        var fail = await AsTask(Result.Fail<int>("bad"));
        var result = await AsTask(fail).ThenAsync(x => Task.FromResult( x * 5));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("bad"));
    }

    [Test]
    public async Task ThenAsync_Chaining_WorksCorrectly()
    {
        var result = await AsTask(Result.Success(2))
            .ThenAsync(x => Task.FromResult(x * 2))
            .ThenAsync(x => Task.FromResult(x + 5));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(9));
    }

    // 4. ThenTryAsync

    [Test]
    public async Task ThenTryAsync_CatchesAsyncException()
    {
        var result = await Result.Success(3)
            .ThenTryAsync(x => ThrowInvalidOperationExceptionAsync());

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("async fail"));
        Assert.That(result.Error!.Code, Is.EqualTo("InvalidOperationException"));
    }

    [Test]
    public async Task ThenTryAsync_ResultReturning_CatchesAsyncException()
    {
        var result = await Result.Success(3)
            .ThenTryAsync(x => ThrowApplicationExceptionAsync());

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("async fail"));
        Assert.That(result.Error!.Code, Is.EqualTo("ApplicationException"));
    }

    [Test]
    public async Task ThenTryAsync_DoesNotApply_OnFailure()
    {
        var result = await Result.Fail<int>("fail")
            .ThenTryAsync(x => Task.FromResult(x * 2));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail"));
    }

    // 5. Multi-capture

    [Test]
    public void Then_WithMultiCapture_CapturesAndTransforms()
    {
        var result = Result.Success("abc")
            .Then((x) => x.Length, out var captured);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(3));
        Assert.That(captured.IsSuccess, Is.True);
        Assert.AreEqual(captured.Value, result.Value);
    }

    [Test]
    public void Then_WithMultiCapture_OnFailure_DoesNotCaptureOrTransform()
    {
        var result = Result.Fail<string>("fail")
                            .Then((x) => x.Length, out var captured);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(captured.IsSuccess, Is.False);
        Assert.That(captured.Error!.Message, Is.EqualTo("fail"));
    }

    // 6. Complex/Chained

    [Test]
    public void Then_Chained_StopsOnFirstFailure()
    {
        var result = Result.Success(1)
            .Then(x => x + 1)
            .ThenTry(x => ThrowApplicationException()) // stops here
            .Then(x => x * 10);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("bad"));
    }

    [Test]
    public async Task ThenAsync_Chained_MixSyncAndAsync_AllSucceed()
    {
        var result = await AsTask(Result.Success(1))
            .ThenAsync(x => Task.FromResult(x + 2))
            .ThenAsync(x => Task.FromResult(x * 3));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(9));
    }

    [Test]
    public async Task ThenAsync_Chained_StopsOnFailure()
    {
        var result = await AsTask(Result.Success(1))
            .ThenAsync(x => Task.FromResult(x + 1))
            .ThenTryAsync<int, int>(x => ThrowInvalidOperationExceptionAsync())
            .ThenAsync(x => Task.FromResult(x * 10));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("async fail"));
    }

    [Test]
    public void Then_MultiStep_WithCapturedParam_ErrorHaltsFlow_CapturedParamAccessible()
    {
        // Arrange
        var start = Result.Success("hello");

        // Act
        var result = start
            .Then(x => x.ToUpper(), out var capturedUpper)     // capturedUpper = "HELLO"
            .Then(x => x + "WORLD")                          // "HELLOWORLD"
            .ThenTry(ThrowInvalidOperationException,"Throwing Exception") // fail here
            .Then(x => x + "after error");                     // should not run

        // Assert: Chain fails at the exception
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail at 3"));
        Assert.That(result.Error!.Code, Is.EqualTo(nameof(InvalidOperationException)));
        Assert.That(result.Error!.Source, Is.EqualTo("Throwing Exception"));

        // Assert: Captured result is still accessible and correct
        Assert.That(capturedUpper.IsSuccess, Is.True);
        Assert.That(capturedUpper.Value, Is.EqualTo("HELLO"));
    }

    // Helper (should be in test class or local to the test)
    private string ThrowInvalidOperationException(string _) => throw new InvalidOperationException("fail at 3");


    [Test]
    public async Task Complex_Chained_Scenario_WithInlineOutVars()
    {
        // Arrange
        var log = new List<string>();
        string input = "  8  ";

        // Local helpers
        string Trim(string s) => s.Trim();
        int ParseInt(string s) => int.Parse(s);
        Task<Result<bool>> IsEvenAsync(int n) => Task.FromResult(n % 2 == 0 ? true : Result.Fail<bool>("Odd number"));
        int DoubleIt(int n) => n * 2;
        Task<string> ToApiStringAsync(int n) => Task.FromResult($"Number: {n}");

        // Act
        var result = await input
            .FailIfNull("Input is required")
            .FailIf(s => string.IsNullOrWhiteSpace(s), "EMPTY", "Input is empty")
            .Then(Trim, out var capturedTrimmed)
            .ThenTry(ParseInt, out var capturedParsed)
            .ThenAsync(async n =>
            {
                var evenRes = await IsEvenAsync(n);
                if (!evenRes.IsSuccess)
                    throw new Exception("Not even!");
                return n;
            })
            .Then(DoubleIt)
            .Tap(out var capturedDoubled)
            .ThenAsync(ToApiStringAsync)
            .TapAsync(final =>
            {
                log.Add($"Trimmed='{capturedTrimmed.Value}', Parsed={capturedParsed.Value}, Doubled={capturedDoubled.Value}, Final='{final}'");
                return Task.CompletedTask;
            });

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("Number: 16"));
        Assert.That(log, Has.Count.EqualTo(1));
        Assert.That(log[0], Is.EqualTo("Trimmed='8', Parsed=8, Doubled=16, Final='Number: 16'"));

        Assert.That(capturedTrimmed.IsSuccess, Is.True);
        Assert.That(capturedTrimmed.Value, Is.EqualTo("8"));
        Assert.That(capturedParsed.IsSuccess, Is.True);
        Assert.That(capturedParsed.Value, Is.EqualTo(8));
        Assert.That(capturedDoubled.IsSuccess, Is.True);
        Assert.That(capturedDoubled.Value, Is.EqualTo(16));
    }


}
