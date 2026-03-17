using FxResult.Core;
using FxResult.Core;
using FxResult.ResultExtensions;

namespace FxResult.UnitTest.ConditionalTests;

[TestFixture]
public class ConditionalTests : ResultTestBase
{
    static Result<string> ClassifyHighValue(int value) =>
        Result<string>.Success($"high:{value}");

    static Result<string> ClassifyMediumValue(int value) =>
        Result<string>.Success($"medium:{value}");

    static Result<string> ClassifyDefault(int value) =>
        Result<string>.Success($"default:{value}");

    static Result<int> ApplyPremiumDiscount(int price) =>
        Result<int>.Success((int)(price * 0.8));

    static Result<int> ApplyStandardDiscount(int price) =>
        Result<int>.Success((int)(price * 0.9));

    static Result<int> NoDiscount(int price) =>
        Result<int>.Success(price);

    static Result<string> RejectOrder(int _) =>
        Result<string>.Fail(new Error("ORDER_REJECTED", "Order value too low"));

    [Test]
    public void If_MatchesFirstBranch_ReturnsFirstResult()
    {
        var result = Result<int>.Success(150)
            .If(x => x > 100, ClassifyHighValue)
            .ElseIf(x => x > 50, ClassifyMediumValue)
            .Else(ClassifyDefault);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("high:150"));
    }

    [Test]
    public void If_MatchesSecondBranch_ReturnsElseIfResult()
    {
        var result = Result<int>.Success(75)
            .If(x => x > 100, ClassifyHighValue)
            .ElseIf(x => x > 50, ClassifyMediumValue)
            .Else(ClassifyDefault);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("medium:75"));
    }

    [Test]
    public void If_NoMatch_ReturnsElseResult()
    {
        var result = Result<int>.Success(10)
            .If(x => x > 100, ClassifyHighValue)
            .ElseIf(x => x > 50, ClassifyMediumValue)
            .Else(ClassifyDefault);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("default:10"));
    }

    [Test]
    public void If_SourceIsFailure_PropagatesError()
    {
        var error = new Error("SRC_ERR", "source failed");
        var result = Result<int>.Fail(error)
            .If(x => x > 100, ClassifyHighValue)
            .ElseIf(x => x > 50, ClassifyMediumValue)
            .Else(ClassifyDefault);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("SRC_ERR"));
    }

    [Test]
    public void If_FirstBranchMatches_SkipsSubsequentBranches()
    {
        bool secondEvaluated = false;
        bool elseEvaluated = false;

        var result = Result<int>.Success(150)
            .If(x => x > 100, ApplyPremiumDiscount)
            .ElseIf(x => { secondEvaluated = true; return x > 50; }, ApplyStandardDiscount)
            .Else(x => { elseEvaluated = true; return NoDiscount(x); });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(120));
        Assert.That(secondEvaluated, Is.False);
        Assert.That(elseEvaluated, Is.False);
    }

    [Test]
    public void If_SameType_WorksCorrectly()
    {
        var result = Result<int>.Success(75)
            .If(x => x > 100, ApplyPremiumDiscount)
            .ElseIf(x => x > 50, ApplyStandardDiscount)
            .Else(NoDiscount);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(67));
    }

    [Test]
    public void Else_NoMatch_CanReturnFail()
    {
        var result = Result<int>.Success(10)
            .If(x => x > 100, ClassifyHighValue)
            .Else(_ => Result<string>.Fail(new Error("NO_MATCH", "No branch matched")));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("NO_MATCH"));
    }

    [Test]
    public void Else_WithMatch_ReturnsMatchedResult()
    {
        var result = Result<int>.Success(200)
            .If(x => x > 100, ClassifyHighValue)
            .Else(_ => Result<string>.Fail(new Error("NO_MATCH", "No branch matched")));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("high:200"));
    }

    [Test]
    public void Else_SourceIsFailure_PropagatesOriginalError()
    {
        var error = new Error("ORIGINAL", "original error");
        var result = Result<int>.Fail(error)
            .If(x => x > 100, ClassifyHighValue)
            .Else(_ => Result<string>.Fail(new Error("NO_MATCH", "No branch matched")));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("ORIGINAL"));
    }

    [Test]
    public void If_MultipleElseIf_MatchesCorrectBranch()
    {
        static Result<string> Grade(string g) => Result<string>.Success(g);

        var result = Result<int>.Success(35)
            .If(x => x > 100, _ => Grade("A"))
            .ElseIf(x => x > 75, _ => Grade("B"))
            .ElseIf(x => x > 50, _ => Grade("C"))
            .ElseIf(x => x > 25, _ => Grade("D"))
            .Else(_ => Grade("F"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("D"));
    }

    [Test]
    public void If_BranchActionReturnsFail_ProducesFail()
    {
        var result = Result<int>.Success(5)
            .If(x => x < 10, RejectOrder)
            .Else(ClassifyDefault);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("ORDER_REJECTED"));
    }

    [Test]
    public void If_SourceFailureWithMeta_PreservesMeta()
    {
        var meta = new FxResult.Core.Meta.MetaInfo { CorrelationId = "test-correlation" };
        var error = new Error("SRC_ERR", "source failed");
        var result = Result<int>.Fail(error, meta)
            .If(x => x > 100, ClassifyHighValue)
            .Else(ClassifyDefault);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Meta.CorrelationId, Is.EqualTo("test-correlation"));
    }
}
