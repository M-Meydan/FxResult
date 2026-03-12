using FxResult.Core;

namespace FxResult.UnitTest;

[TestFixture]
public class ErrorTests
{
    [Test]
    public void WithContext_WhenNullArgs_DoesNotOverwriteExistingSourceAndCaller()
    {
        var error = new Error("E", "m", Source: "S", Caller: "C");

        var next = error.WithContext(source: null, caller: null);

        Assert.Multiple(() =>
        {
            Assert.That(next.Source, Is.EqualTo("S"));
            Assert.That(next.Caller, Is.EqualTo("C"));
        });
    }

    [Test]
    public void ImplicitOperator_FromString_UsesCodeAsMessage()
    {
        Error error = "SOME_CODE";

        Assert.Multiple(() =>
        {
            Assert.That(error.Code, Is.EqualTo("SOME_CODE"));
            Assert.That(error.Message, Is.EqualTo("SOME_CODE"));
        });
    }

    [Test]
    public void ImplicitOperator_FromTuple_WhenMessageProvided_UsesProvidedMessage()
    {
        Error error = ("MY_CODE", "custom message");

        Assert.Multiple(() =>
        {
            Assert.That(error.Code, Is.EqualTo("MY_CODE"));
            Assert.That(error.Message, Is.EqualTo("custom message"));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void ImplicitOperator_FromTuple_WhenMessageMissing_FallsBackToCode(string? message)
    {
        Error error = ("MY_CODE", message);

        Assert.Multiple(() =>
        {
            Assert.That(error.Code, Is.EqualTo("MY_CODE"));
            Assert.That(error.Message, Is.EqualTo("MY_CODE"));
        });
    }

    [Test]
    public void HasException_ReturnsTrue_WhenExceptionIsPresent()
    {
        var error = new Error("E", "m", Exception: new InvalidOperationException("boom"));
        Assert.That(error.HasException, Is.True);
    }

    [Test]
    public void HasException_ReturnsFalse_WhenExceptionIsNull()
    {
        var error = new Error("E", "m");
        Assert.That(error.HasException, Is.False);
    }

    [Test]
    public void Location_ReturnsFileLineAndCaller_WhenAllPresent()
    {
        var error = new Error("E", "m", FilePath: @"C:\src\MyService.cs", LineNumber: 42, Caller: "DoWork");
        Assert.That(error.Location, Is.EqualTo($"MyService.cs:42 \u2192 DoWork"));
    }

    [Test]
    public void Location_ReturnsFileAndLine_WhenCallerIsNull()
    {
        var error = new Error("E", "m", FilePath: @"C:\src\MyService.cs", LineNumber: 42);
        Assert.That(error.Location, Is.EqualTo("MyService.cs:42"));
    }

    [Test]
    public void Location_ReturnsCaller_WhenFilePathAndLineAreNull()
    {
        var error = new Error("E", "m", Caller: "DoWork");
        Assert.That(error.Location, Is.EqualTo("DoWork"));
    }

    [Test]
    public void Location_ReturnsNull_WhenAllLocationFieldsAreNull()
    {
        var error = new Error("E", "m");
        Assert.That(error.Location, Is.Null);
    }

    [Test]
    public void WithContext_OverwritesExistingSourceAndCaller()
    {
        var error = new Error("E", "m", Source: "OldSource", Caller: "OldCaller");

        var next = error.WithContext(source: "NewSource", caller: "NewCaller");

        Assert.Multiple(() =>
        {
            Assert.That(next.Source, Is.EqualTo("NewSource"));
            Assert.That(next.Caller, Is.EqualTo("NewCaller"));
        });
    }
}
