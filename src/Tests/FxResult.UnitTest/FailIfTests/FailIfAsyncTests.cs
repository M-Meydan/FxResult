using FxResult.Core;
using FxResult.ResultExtensions;
namespace FxResult.UnitTest.FailIfTests;

[TestFixture]
public class FailIfAsyncTests
{
    [Test]
    public async Task FailIfAsync_PredicateTrue_ReturnsFailure()
    {
        var result = Result<string>.Success("error");

        var output = await result.FailIfAsync(async val => await Task.FromResult(val == "error"), "FAIL_CODE", "It failed");

        Assert.That(output.IsFailure, Is.True);
        Assert.That(output.Error!.Code, Is.EqualTo("FAIL_CODE"));
        Assert.That(output.Error!.Message, Is.EqualTo("It failed"));
        Assert.That(output.Error!.Caller, Is.EqualTo(nameof(FailIfAsync_PredicateTrue_ReturnsFailure)));
    }

    [Test]
    public async Task FailIfAsync_PredicateFalse_ReturnsSuccess()
    {
        var result = Result<string>.Success("good");

        var output = await result.FailIfAsync(async val => await Task.FromResult(false), "SHOULD_NOT_FAIL", "False predicate");

        Assert.That(output.IsSuccess, Is.True);
        Assert.That(output.Value, Is.EqualTo("good"));
    }

    [Test]
    public async Task FailIfAsync_ConditionTrue_ReturnsFailure()
    {
        var result = Result<int>.Success(42);

        var output = await result.FailIfAsync(async () => await Task.FromResult(true), "COND_FAIL", "Condition failed");

        Assert.That(output.IsFailure, Is.True);
        Assert.That(output.Error!.Code, Is.EqualTo("COND_FAIL"));
        Assert.That(output.Error!.Message, Is.EqualTo("Condition failed"));
        Assert.That(output.Error!.Caller, Is.EqualTo(nameof(FailIfAsync_ConditionTrue_ReturnsFailure)));
    }

    [Test]
    public async Task FailIfAsync_ConditionFalse_ReturnsSuccess()
    {
        var result = Result<int>.Success(42);

        var output = await result.FailIfAsync(async () => await Task.FromResult(false), "NO_FAIL", "Should not fail");

        Assert.That(output.IsSuccess, Is.True);
        Assert.That(output.Value, Is.EqualTo(42));
    }

    [Test]
    public async Task FailIfAsync_Predicate_ShortCircuitsOnFailedResult()
    {
        var result = Result<string>.Fail("Early error");

        var output = await result.FailIfAsync(val => throw new Exception("Should not be called"), "IGNORED", "Not evaluated");

        Assert.That(output.IsFailure, Is.True);
        Assert.That(output.Error!.Message, Is.EqualTo("Early error"));
    }

    [Test]
    public async Task FailIfAsync_Condition_ShortCircuitsOnFailedResult()
    {
        var result = Result<int>.Fail("Initial fail");

        var output = await result.FailIfAsync(async () => throw new Exception("Should not be evaluated"), "IGNORED", "Not evaluated");

        Assert.That(output.IsFailure, Is.True);
        Assert.That(output.Error!.Message, Is.EqualTo("Initial fail"));
    }

    [Test]
    public async Task FailIfAsync_WhenPredicateThrows_ReturnsErrorResult()
    {
        // Arrange
        var result = Result<int>.Success(42);

        // Act
        var errorResult = await result.FailIfAsync<int>(
            async x =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Predicate failed!");
            },
            "EXC", "Predicate threw"
        );

        // Assert
        Assert.That(errorResult.IsFailure, Is.True);
        Assert.That(errorResult.Error, Is.Not.Null);
        Assert.That(errorResult.Error!.Code, Is.EqualTo(nameof(InvalidOperationException)));
        Assert.That(errorResult.Error!.Message, Is.EqualTo("Predicate failed!"));
    }

    #region EnsureAsync tests
    [Test]
    public async Task EnsureAsync_PredicateTrue_ReturnsSuccess()
    {
        var result = Result<int>.Success(10);

        var output = await result.EnsureAsync(async x => await Task.FromResult(x > 0), "POSITIVE", "Must be positive");

        Assert.That(output.IsSuccess, Is.True);
        Assert.That(output.Value, Is.EqualTo(10));
    }

    [Test]
    public async Task EnsureAsync_PredicateFalse_ReturnsFailure()
    {
        var result = Result<int>.Success(-1);

        var output = await result.EnsureAsync(async x => await Task.FromResult(x >= 0), "NEGATIVE", "Must not be negative");

        Assert.That(output.IsFailure, Is.True);
        Assert.That(output.Error!.Code, Is.EqualTo("NEGATIVE"));
        Assert.That(output.Error!.Message, Is.EqualTo("Must not be negative"));
        Assert.That(output.Error!.Caller, Is.EqualTo(nameof(EnsureAsync_PredicateFalse_ReturnsFailure)));
    }

    [Test]
    public async Task EnsureAsync_AlreadyFailed_SkipsEvaluation()
    {
        var result = Result<int>.Fail("Initial failure");

        var output = await result.EnsureAsync(async x => throw new Exception("Should not be called"), "SHOULD_NOT_RUN", "Error");

        Assert.That(output.IsFailure, Is.True);
        Assert.That(output.Error!.Message, Is.EqualTo("Initial failure"));
    }

    [Test]
    public async Task EnsureAsync_Failure_HaltsChaining()
    {
        var result = await Result<int>.Success(0)
            .EnsureAsync(async x => await Task.FromResult(x > 0), "ZERO", "Must be positive")
            .Then(x => x + 1);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo("ZERO"));
    }

    [Test]
    public async Task EnsureAsync_SetsSourceAndCaller()
    {
        var result = await Result<string>.Success("abc")
            .EnsureAsync(async x => false, "ENSURE_FAIL", "Should fail", source: "TestSource", caller:"TestCaller");

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Source, Is.EqualTo("TestSource"));
        Assert.That(result.Error!.Caller, Is.EqualTo("TestCaller"));
    }

    #endregion
}
