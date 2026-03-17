using FxResult.Core;
using FxResult.ResultExtensions;

namespace FxResult.UnitTest.ConditionalTests;

[TestFixture]
public class ConditionalAsyncTests : ResultTestBase
{
    static async Task<Result<string>> LookupPremiumLabelAsync(int value)
    {
        await Task.Delay(1);
        return Result<string>.Success($"premium:{value}");
    }

    static async Task<Result<string>> LookupStandardLabelAsync(int value)
    {
        await Task.Delay(1);
        return Result<string>.Success($"standard:{value}");
    }

    static async Task<Result<string>> LookupDefaultLabelAsync(int value)
    {
        await Task.Delay(1);
        return Result<string>.Success($"basic:{value}");
    }

    static Result<string> FormatHigh(int value) =>
        Result<string>.Success($"high:{value}");

    static Result<string> FormatMedium(int value) =>
        Result<string>.Success($"medium:{value}");

    static Result<string> FormatDefault(int value) =>
        Result<string>.Success($"default:{value}");

    static async Task<Result<int>> ApplyPremiumDiscountAsync(int price)
    {
        await Task.Delay(1);
        return Result<int>.Success((int)(price * 0.8));
    }

    static async Task<Result<int>> ApplyStandardDiscountAsync(int price)
    {
        await Task.Delay(1);
        return Result<int>.Success((int)(price * 0.9));
    }

    static async Task<Result<int>> NoDiscountAsync(int price)
    {
        await Task.Delay(1);
        return Result<int>.Success(price);
    }

    [Test]
    public async Task IfAsync_MatchesFirstBranch_ReturnsFirstResult()
    {
        var result = await Result<int>.Success(150)
            .IfAsync(x => x > 100, LookupPremiumLabelAsync)
            .ElseIfAsync(x => x > 50, LookupStandardLabelAsync)
            .ElseAsync(LookupDefaultLabelAsync);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("premium:150"));
    }

    [Test]
    public async Task IfAsync_MatchesSecondBranch_ReturnsElseIfResult()
    {
        var result = await Result<int>.Success(75)
            .IfAsync(x => x > 100, LookupPremiumLabelAsync)
            .ElseIfAsync(x => x > 50, LookupStandardLabelAsync)
            .ElseAsync(LookupDefaultLabelAsync);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("standard:75"));
    }

    [Test]
    public async Task IfAsync_NoMatch_ReturnsElseResult()
    {
        var result = await Result<int>.Success(10)
            .IfAsync(x => x > 100, LookupPremiumLabelAsync)
            .ElseIfAsync(x => x > 50, LookupStandardLabelAsync)
            .ElseAsync(LookupDefaultLabelAsync);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("basic:10"));
    }

    [Test]
    public async Task IfAsync_SourceIsFailure_PropagatesError()
    {
        var error = new Error("SRC_ERR", "source failed");
        var result = await Result<int>.Fail(error)
            .IfAsync(x => x > 100, LookupPremiumLabelAsync)
            .ElseAsync(LookupDefaultLabelAsync);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("SRC_ERR"));
    }

    [Test]
    public async Task If_OnTaskResult_MatchesBranch()
    {
        var result = await AsTask(Result<int>.Success(150))
            .If(x => x > 100, FormatHigh)
            .Else(FormatDefault);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("high:150"));
    }

    [Test]
    public async Task If_OnTaskResult_FallsToElse()
    {
        var result = await AsTask(Result<int>.Success(10))
            .If(x => x > 100, FormatHigh)
            .Else(FormatDefault);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("default:10"));
    }

    [Test]
    public async Task ElseIf_OnTaskBranch_WorksCorrectly()
    {
        var result = await AsTask(Result<int>.Success(75))
            .If(x => x > 100, FormatHigh)
            .ElseIf(x => x > 50, FormatMedium)
            .Else(FormatDefault);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("medium:75"));
    }

    [Test]
    public async Task Else_OnTaskBranch_NoMatch_CanReturnFail()
    {
        var result = await AsTask(Result<int>.Success(10))
            .If(x => x > 100, FormatHigh)
            .Else(_ => Result<string>.Fail(new Error("NO_MATCH", "No branch matched")));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo("NO_MATCH"));
    }

    [Test]
    public async Task If_SyncBranch_ElseIfAsync_WorksCorrectly()
    {
        var branch = Result<int>.Success(75)
            .If(x => x > 100, FormatHigh);

        var result = await branch
            .ElseIfAsync(x => x > 50, LookupStandardLabelAsync)
            .ElseAsync(LookupDefaultLabelAsync);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("standard:75"));
    }

    [Test]
    public async Task IfAsync_FirstMatchSkipsRest()
    {
        bool secondEvaluated = false;

        var result = await Result<int>.Success(150)
            .IfAsync(x => x > 100, ApplyPremiumDiscountAsync)
            .ElseIfAsync(x => { secondEvaluated = true; return x > 50; }, ApplyStandardDiscountAsync)
            .ElseAsync(NoDiscountAsync);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(120));
        Assert.That(secondEvaluated, Is.False);
    }

    [Test]
    public async Task IfAsync_WithSyncElse_WorksCorrectly()
    {
        var branch = await Result<int>.Success(10)
            .IfAsync(x => x > 100, LookupPremiumLabelAsync);

        var result = branch.Else(FormatDefault);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("default:10"));
    }
}
