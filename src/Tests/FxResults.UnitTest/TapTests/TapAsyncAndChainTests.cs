using FxResults.Core;
using FxResults.ResultExtensions;


namespace FxResults.UnitTest.TapTests;


[TestFixture]
public class TapAsyncAndChainTests
{
    // Helpers for test flows
    private Task<Result<int>> AsyncStart(int n) => Task.FromResult(new Result<int>(n));
    private Task<Result<int>> AsyncFail(string message) => Task.FromResult(Result<int>.Fail(message));
    private Task<Result<int>> AddAsync(int n, int amount) => Task.FromResult(Result<int>.Success(n + amount));
    private Task<Result<string>> ToStringAsync(int n) => Task.FromResult(Result<string>.Success($"Num:{n}"));

    private async Task SimulateAsyncLog(string s)
    {
        await Task.Delay(1);
        Logs.Add($"Logged: {s}");
    }
    private List<string> Logs = new();

    [SetUp]
    public void Setup() => Logs = new List<string>();

   
    [Test]
    public async Task Async_FluentChain_TapAsync_ThenAsync_CaptureValues()
    {
        // TapAsync for side-effect, ThenAsync for transform, sync Then for business rule
        var final = await AsyncStart(10)
            .TapAsync(async x => await SimulateAsyncLog($"Initial: {x}"))
            .ThenAsync(x => AddAsync(x, 5))
            .TapAsync(async x => await SimulateAsyncLog($"After add: {x}"))
            .Then(x => x * 3)
            .TapAsync(async x => await SimulateAsyncLog($"After multiply: {x}"))
            .ThenAsync(ToStringAsync);

        Assert.That(final.IsSuccess, Is.True);
        Assert.That(final.Value, Is.EqualTo("Num:45"));
        Assert.That(Logs, Is.EqualTo(new List<string>
        {
            "Logged: Initial: 10",
            "Logged: After add: 15",
            "Logged: After multiply: 45"
        }));
    }

    [Test]
    public async Task Async_FluentChain_TapAndTapAsync_StopsOnFailure()
    {
        var final = await AsyncFail("bad start")
            .TapAsync(async x => await SimulateAsyncLog($"Initial: {x}"))
            .ThenAsync(x => AddAsync(x, 5))
            .TapAsync(async x => await SimulateAsyncLog($"After add: {x}"));

        Assert.That(final.IsSuccess, Is.False);
        Assert.That(final.Error!.Message, Is.EqualTo("bad start"));
        Assert.That(Logs, Is.Empty);
    }

    [Test]
    public async Task Async_FluentChain_TapSync_SideEffect()
    {
        var final = await AsyncStart(7)
            .TapAsync(x => { Logs.Add($"Sync log: {x}"); return Task.CompletedTask; })
            .Then(x => x + 1)
            .TapAsync(async x => await SimulateAsyncLog($"Async log: {x}"))
            .Then(x => x+1);

        Assert.That(final.IsSuccess, Is.True);
        Assert.That(final.Value, Is.EqualTo(9));
        Assert.That(Logs, Is.EqualTo(new List<string>
        {
            "Sync log: 7",
            "Logged: Async log: 8"
        }));
    }

    [Test]
    public async Task Async_Chain_TapAsync_Exception_ReturnsError()
    {
        async Task ThrowingAsync(int x)
        {
            await Task.Delay(1);
            throw new InvalidOperationException("tap failed");
        }

        var final = await AsyncStart(5)
            .TapAsync(ThrowingAsync)
            .Then(x => x + 100); // Should not execute

        Assert.That(final.IsSuccess, Is.False);
        Assert.That(final.Error!.Message, Is.EqualTo("tap failed"));
        Assert.That(final.Error!.Code, Is.EqualTo("InvalidOperationException"));
    }

    [Test]
    public async Task Async_Chain_Tap_SyncException_ReturnsError()
    {
        var final = await AsyncStart(3)
            .TapAsync(x => throw new ApplicationException("tap error"))
            .Then(x => x + 2); // Should not execute

        Assert.That(final.IsSuccess, Is.False);
        Assert.That(final.Error!.Message, Is.EqualTo("tap error"));
        Assert.That(final.Error!.Code, Is.EqualTo("ApplicationException"));
    }

    [Test]
    public async Task Async_Chain_MultiStep_CaptureIntermediateValues()
    {
        int? capturedAfterAdd = null;
        string? capturedAsString = null;

        // Chain to final, capture intermediate after await
        var temp = await AsyncStart(4)
            .ThenAsync(x => AddAsync(x, 10))
            .TapAsync(x => { capturedAfterAdd = x; return Task.CompletedTask; })
            .ThenAsync(x => ToStringAsync(x));

        temp.Tap(x => capturedAsString = x); // capture on the result

        Assert.That(temp.IsSuccess, Is.True);
        Assert.That(temp.Value, Is.EqualTo("Num:14"));
        Assert.That(capturedAfterAdd, Is.EqualTo(14));
        Assert.That(capturedAsString, Is.EqualTo("Num:14"));
    }

    [Test]
    public async Task Async_Chain_All_Taps_Called_WhenNoError()
    {
        var sideEffects = new List<string>();

        var final = await AsyncStart(1)
            .TapAsync(x => { sideEffects.Add($"sync: {x}"); return Task.CompletedTask; })
            .TapAsync(async x => { await Task.Delay(1); sideEffects.Add($"async: {x}"); })
            .Then(x => x + 1)
            .TapAsync(x => { sideEffects.Add($"sync2: {x}"); return Task.CompletedTask; })
            .TapAsync(async x => { await Task.Delay(1); sideEffects.Add($"async2: {x}"); });

        Assert.That(final.IsSuccess, Is.True);
        Assert.That(final.Value, Is.EqualTo(2));
        Assert.That(sideEffects, Is.EqualTo(new List<string>
        {
            "sync: 1",
            "async: 1",
            "sync2: 2",
            "async2: 2"
        }));
    }

    [Test]
    public async Task Async_Chain_Tap_SkipOnFailure()
    {
        var log = new List<string>();

        var final = await AsyncFail("fail here")
            .TapAsync(x => { log.Add($"tap: {x}"); return Task.CompletedTask; })
            .TapAsync(async x => { await Task.Delay(1); log.Add($"tapAsync: {x}"); });

        Assert.That(final.IsSuccess, Is.False);
        Assert.That(log, Is.Empty);
    }
}