using FxResults.Core;
using FxResults.Extensions;

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FxResults.UnitTest.TapTests;

[TestFixture]
public class TapExtensionsTests
{

    private Result<int> GetUserId(string username) =>
        username switch
        {
            "admin" => Result.Success(1001),
            "bob" => Result.Success(1002),
            _ => Result.Fail<int>("User not found")
        };

    private Result<string> GetUserName(int userId) =>
        userId switch
        {
            1001 => Result.Success("Admin User"),
            1002 => Result.Success("Bob Builder"),
            _ => Result.Fail<string>("Unknown ID")
        };

    private Result<bool> IsUserActive(int userId) =>
        userId == 1001 ? true :
        userId == 1002 ? false :
        Result.Fail<bool>("User not found");

    [Test]
    public void Tap_ExecutesAction_OnSuccess()
    {
        var log = new List<string>();
        var result = GetUserId("admin")
            .Tap(id => log.Add($"Fetched user ID: {id}"));

        Assert.That(log, Has.Count.EqualTo(1));
        Assert.That(log[0], Is.EqualTo("Fetched user ID: 1001"));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1001));
    }

    [Test]
    public void Tap_DoesNotExecute_OnFailure()
    {
        var log = new List<string>();
        var result = GetUserId("ghost")
            .Tap(id => log.Add($"Fetched user ID: {id}"));

        Assert.That(log, Is.Empty);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("User not found"));
    }

    [Test]
    public void Tap_ActionThrows_ReturnsErrorResult()
    {
        var result = GetUserId("admin")
            .Tap(id => throw new ApplicationException("tap error"));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("tap error"));
        Assert.That(result.Error!.Code, Is.EqualTo("ApplicationException"));
    }

    [Test]
    public void Tap_DoesNotAlterResult()
    {
        var result = GetUserId("admin")
            .Tap(id => { });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1001));
    }

    [Test]
    public void Tap_CapturesResult_WithOutParameter_Success()
    {
        var result = GetUserId("admin")
            .Tap(out Result<int> captured);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1001));
    }

    [Test]
    public void Tap_CapturesResult_WithOutParameter_Failure()
    {
        var result = GetUserId("missinguser")
            .Tap(out Result<int> captured);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("User not found"));
    }

    [Test]
    public void Tap_CapturesValue_WithOutParameter_Success()
    {
        var result = GetUserName(1001)
            .Tap(out var captured);

        Assert.That(captured.Value, Is.EqualTo("Admin User"));
        Assert.That(result.Value, Is.EqualTo("Admin User"));
    }

    [Test]
    public void Tap_CapturesValue_WithOutParameter_Failure()
    {
        var result = GetUserName(0)
            .Tap(out var captured);

        Assert.That(captured.IsSuccess, Is.False);
        Assert.That(captured.Error!.Message, Is.EqualTo("Unknown ID"));
        Assert.That(result.IsSuccess, Is.False);
    }


    [Test]
    public void Tap_MultipleSideEffects_AllAreCalled()
    {
        var log = new List<string>();
        var result = GetUserId("admin")
            .Tap(id => log.Add($"First: {id}"))
            .Then(GetUserName)
            .Tap(name => log.Add($"Second: {name}"));

        Assert.That(log, Is.EqualTo(new List<string> { "First: 1001", "Second: Admin User" }));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("Admin User"));
    }

    [Test]
    public void Tap_CanBeChainedWithOutCaptures()
    {
        var result = GetUserId("admin")
            .Tap(out var step1)
            .Then(GetUserName)
            .Tap(out var step2);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(step1.Value, Is.EqualTo(1001));
        Assert.That(step2.Value, Is.EqualTo("Admin User"));
        Assert.That(result.Value, Is.EqualTo("Admin User"));
    }

    [Test]
    public void Tap_WithOut_And_Action_BothWork()
    {
        var log = new List<string>();
        var result = GetUserId("admin")
            .Tap(out var captured)
            .Tap(id => log.Add($"Tapped: {id}"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(1001));
        Assert.That(log, Has.Count.EqualTo(1));
        Assert.That(log[0], Is.EqualTo("Tapped: 1001"));
    }

    [Test]
    public void Tap_ExceptionInOneTapDoesNotAffectOtherTaps()
    {
        var log = new List<string>();
        var result = GetUserId("admin")
            .Tap(id => log.Add("Before fail"))
            .Tap(id => throw new Exception("bad tap"))
            .Tap(id => log.Add("After fail"));

        Assert.That(log, Is.EqualTo(new List<string> { "Before fail" }));
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("bad tap"));
    }

    [Test]
    public void Tap_Chained_SideEffects_Capture_MultiBranch()
    {
        var log = new List<string>();
        var audit = new List<string>();
        string? capturedUserName = null;
        int? capturedId = null;

        var result = GetUserId("admin")
            .Tap(id =>
            {
                log.Add($"Fetched user ID: {id}");
                capturedId = id;
            })
            .Then(GetUserName)
            .Tap(name =>
            {
                capturedUserName = name;
                audit.Add($"Audited user: {name}");
            })
            .Then(name => name.Length)
            .Tap(len => log.Add($"User name length: {len}"));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("Admin User".Length));
        Assert.That(capturedUserName, Is.EqualTo("Admin User"));
        Assert.That(capturedId, Is.EqualTo(1001));
        Assert.That(log, Is.EqualTo(new List<string> { "Fetched user ID: 1001", "User name length: 10" }));
        Assert.That(audit, Is.EqualTo(new List<string> { "Audited user: Admin User" }));
    }

    [Test]
    public void Tap_MultiStep_ErrorBranch_StillCapturesPrior()
    {
        var log = new List<string>();
        int? capturedId = null;

        var result = GetUserId("bob")
            .Tap(id => capturedId = id)
            .Then(GetUserName)
            .Tap(name => log.Add($"User name: {name}"))
            .ThenTry<string,int>(x=> throw new Exception("fail after name"));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("fail after name"));
        Assert.That(capturedId, Is.EqualTo(1002));
        Assert.That(log, Is.EqualTo(new List<string> { "User name: Bob Builder" }));
    }

    [Test]
    public void Tap_Failure_Branching_TapNotCalled()
    {
        var log = new List<string>();
        var result = GetUserId("ghost")
            .Tap(id => log.Add($"Should not run for: {id}"))
            .Then(GetUserName)
            .Tap(name => log.Add($"Name: {name}"));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(log, Is.Empty);
    }

    [Test]
    public void Tap_Complex_Branch_Capture_MidFlow()
    {
        var log = new List<string>();
        var capturedResult2 = Result.Fail<string>("error");
        string? capturedName = null;

        var result = GetUserId("admin")
            .Then(GetUserName)
            .Tap(out var capturedResult)
            .Tap(name =>
            {
                capturedName = name;
                log.Add($"Got name: {name}");
                capturedResult2 = capturedResult;
            })
            .Then(name => name.StartsWith("A") ? Result.Fail<string>("Name blocked") : name.ToUpper())
            .Tap(name => log.Add($"Upper: {name}"));

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error!.Message, Is.EqualTo("Name blocked"));
        Assert.That(log, Is.EqualTo(new List<string> { "Got name: Admin User" }));
        Assert.That(capturedResult2.IsSuccess, Is.True);
        Assert.That(capturedResult.Value, Is.EqualTo("Admin User"));
        Assert.That(capturedName, Is.EqualTo("Admin User"));
    }

    [Test]
    public void Tap_Nested_Steps_And_MultiError_Paths()
    {
        var auditLog = new List<string>();
        var errorLog = new List<string>();
        var userIds = new[] { "admin", "ghost", "bob" };

        foreach (var user in userIds)
        {
            var result = GetUserId(user)
                .Tap(id => auditLog.Add($"Fetched: {user}:{id}"))
                .Then(IsUserActive)
                .Tap(active => auditLog.Add($"Active: {user}:{active}"))
                .Then(active => active ? Result.Success("Allowed") : Result.Fail<string>("Inactive"))
                .Tap(x => auditLog.Add($"Final: {user}:{x}"));

            if (!result.IsSuccess)
                errorLog.Add($"{user}: {result.Error!.Message}");
        }

        Assert.That(auditLog, Is.EqualTo(new List<string>
        {
            "Fetched: admin:1001", "Active: admin:True", "Final: admin:Allowed",
            "Fetched: bob:1002",   "Active: bob:False"
        }));
        Assert.That(errorLog, Is.EqualTo(new List<string>
        {
            "ghost: User not found",
            "bob: Inactive"
        }));
    }

    [Test]
    public void Tap_SideEffect_DoesNotChangeResult()
    {
        var result = GetUserId("bob")
            .Tap(id => { })
            .Then(GetUserName);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("Bob Builder"));
    }

    [Test]
    public void Tap_CanCaptureAndLogIntermediateResult()
    {
        var log = new List<string>();
        var result = GetUserId("bob")
            .Tap(out var captured)
            .Tap(id => log.Add($"Intermediate: {id}"))
            .Then(GetUserName);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(captured.Value, Is.EqualTo(1002));
        Assert.That(log, Is.EqualTo(new List<string> { "Intermediate: 1002" }));
        Assert.That(result.Value, Is.EqualTo("Bob Builder"));
    }
}
