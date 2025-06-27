using FxResults.Core;
using FxResults.ResultExtensions;

namespace FxResults.UnitTest.FailIfTests;

/// <summary>
/// Unit tests for FailIfExtensions (FailIfNull, FailIf and all overloads).
/// </summary>
[TestFixture]
public class FailIfTests
{
    // 1. FailIfNull

    [Test]
    public void FailIfNull_ReturnsSuccess_ForNonNull()
    {
        string value = "hello";
        var result = value.FailIfNull("Should not be null");
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(value));
    }

    [Test]
    public void FailIfNull_ReturnsFailure_ForNull()
    {
        string? value = null;
        var result = value.FailIfNull("Should not be null");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("Should not be null"));
        Assert.That(result.Error!.Code, Is.EqualTo("NULL_VALUE"));
    }

    [Test]
    public void FailIfNull_ChainedWithThen_SkipsThenOnNull()
    {
        string? value = null;
        var result = value.FailIfNull("Should not be null").Then(v => v.Length);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("Should not be null"));
    }

    [Test]
    public void FailIfNull_SetsSourceAndCallerCorrectly()
    {
        string? value = null;
        var result = value.FailIfNull("Missing!", code: "MISSING", source: "MyMethod",caller:"MethodCalling");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Source, Is.EqualTo("MyMethod"));
        Assert.That(result.Error!.Caller, Is.EqualTo("MethodCalling"));
    }

    // 2. FailIf (Predicate)

    [Test]
    public void FailIf_PredicateTrue_ReturnsFailure()
    {
        var result = Result<int>.Success(5).FailIf(x => x > 3, x => new Error("Too big"));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("Too big"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_PredicateTrue_ReturnsFailure)));
    }

    [Test]
    public void FailIf_PredicateFalse_ReturnsSuccess()
    {
        var result = Result<int>.Success(2).FailIf(x => x > 3, x => new Error("Too big"));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(2));
    }

    [Test]
    public void FailIf_PredicateFalse_ChainedWithThen_RunsThen()
    {
        var result = Result<int>.Success(2).FailIf(x => x > 3, x => new Error("Too big")).Then(x => x * 10);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(20));
    }

    [Test]
    public void FailIf_PredicateTrue_ChainedWithThen_SkipsThen()
    {
        var result = Result<int>.Success(5).FailIf(x => x > 3, x => new Error("Too big")).Then(x => x * 10);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("Too big"));
    }

    [Test]
    public void FailIf_Predicate_SetsCustomErrorCorrectly()
    {
        var result = Result<int>.Success(5).FailIf(x => x > 3, "TOO_BIG", "Too big error!");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Code, Is.EqualTo("TOO_BIG"));
        Assert.That(result.Error!.Message, Is.EqualTo("Too big error!"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_Predicate_SetsCustomErrorCorrectly)));
    }

    [Test]
    public void FailIf_Predicate_WithCustomSource_SetsSourceAndCaller()
    {
        var result = Result<int>.Success(123).FailIf(x => x > 0, code: "TOO_HIGH", message: "Too high!", source: "TestPredicateSource");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Source, Is.EqualTo("TestPredicateSource"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_Predicate_WithCustomSource_SetsSourceAndCaller)));
    }

    // 3. FailIf (Condition)

    [Test]
    public void FailIf_ConditionTrue_ReturnsFailure()
    {
        var result = Result<int>.Success(7).FailIf(true, () => new Error("Always fails"));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("Always fails"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_ConditionTrue_ReturnsFailure)));
    }

    [Test]
    public void FailIf_ConditionFalse_ReturnsSuccess()
    {
        var result = Result<int>.Success(7).FailIf(false, () => new Error("Should not fail"));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(7));
    }

    [Test]
    public void FailIf_Condition_SetsCustomErrorCorrectly()
    {
        var result = Result<int>.Success(8).FailIf(true, "CONDITION", "Condition failed");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Code, Is.EqualTo("CONDITION"));
        Assert.That(result.Error!.Message, Is.EqualTo("Condition failed"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_Condition_SetsCustomErrorCorrectly)));
    }

    [Test]
    public void FailIf_ConditionTrue_ChainedWithThen_SkipsThen()
    {
        var result = Result<int>.Success(7).FailIf(true, () => new Error("fail")).Then(x => x * 2);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail"));
    }

    [Test]
    public void FailIf_Condition_WithCustomSource_SetsSourceAndCaller()
    {
        var result = Result<int>.Success(1).FailIf(true, code: "FAIL", message: "Failed!", source: "TestConditionSource");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Source, Is.EqualTo("TestConditionSource"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_Condition_WithCustomSource_SetsSourceAndCaller)));
    }

    // 4. FailIf (Inline code/message)

    [Test]
    public void FailIf_Predicate_InlineCodeMessage()
    {
        var result = Result<int>.Success(0).FailIf(x => x == 0, "ZERO", "Value is zero");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Code, Is.EqualTo("ZERO"));
        Assert.That(result.Error!.Message, Is.EqualTo("Value is zero"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_Predicate_InlineCodeMessage)));
    }

    [Test]
    public void FailIf_Condition_InlineCodeMessage()
    {
        var result = Result<int>.Success(0).FailIf(true, "ZERO", "Value is zero");
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Code, Is.EqualTo("ZERO"));
        Assert.That(result.Error!.Message, Is.EqualTo("Value is zero"));
        Assert.That(result.Error!.Caller, Is.EqualTo(nameof(FailIf_Condition_InlineCodeMessage)));
    }

#region Ensure tests
    [Test]
        public void Ensure_PredicateTrue_ReturnsSuccess()
        {
            var result = Result<int>.Success(10)
                .Ensure(x => x > 0, "POSITIVE", "Must be positive");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo(10));
        }

        [Test]
        public void Ensure_PredicateFalse_ReturnsFailure()
        {
            var result = Result<int>.Success(-5)
                .Ensure(x => x >= 0, "NEGATIVE", "Must not be negative");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error!.Code, Is.EqualTo("NEGATIVE"));
            Assert.That(result.Error!.Message, Is.EqualTo("Must not be negative"));
            Assert.That(result.Error!.Caller, Is.EqualTo(nameof(Ensure_PredicateFalse_ReturnsFailure)));
        }

        [Test]
        public void Ensure_AlreadyFailed_ShortCircuits()
        {
            var result = Result<int>.Fail("Initial failure")
                .Ensure(x => throw new Exception("Should not be called"), "BAD", "Will not run");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error!.Message, Is.EqualTo("Initial failure"));
        }

    [Test]
    public void Ensure_Failure_BlocksChainedThen()
    {
        var result = Result<int>.Success(0)
            .Ensure(x => x > 0, "ZERO", "Must be positive")
            .Then(x => x + 1);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error!.Code, Is.EqualTo("ZERO"));
        Assert.That(result.Error.Message, Is.EqualTo("Must be positive"));
    }

        [Test]
        public void Ensure_ProvidesCallerAndSource()
        {
            var result = Result<string>.Success("test")
                .Ensure(x => false, code: "FAIL", message: "Failed!", source: "EnsureMethod", caller:"EnsureCaller");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error!.Source, Is.EqualTo("EnsureMethod"));
            Assert.That(result.Error!.Caller, Is.EqualTo("EnsureCaller"));
        }
    #endregion

}
