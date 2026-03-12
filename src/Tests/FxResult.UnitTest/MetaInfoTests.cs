using NUnit.Framework;
using FxResult.Core;
using FxResult.Core.Meta;
using FxResult.ResultExtensions.Helpers;
using System;

namespace FxResult.UnitTest;

[TestFixture]
public class MetaInfoTests
{
    [Test]
    public void MetaInfo_Defaults_AreCorrect()
    {
        var meta = new MetaInfo();
        Assert.That(meta.Pagination, Is.Null);
        Assert.That(meta.Additional, Is.Empty);
    }

    [Test]
    public void MetaInfo_Additional_IsImmutable()
    {
        var meta = new MetaInfo();
        var updated = meta with { Additional = meta.Additional.Add("key", 123) };
        Assert.That(updated.Additional["key"], Is.EqualTo(123));
        Assert.That(meta.Additional.ContainsKey("key"), Is.False);
    }

    [Test]
    public void WithMeta_ShouldReplaceMeta_ForSuccessAndFailure()
    {
        var meta1 = new MetaInfo(correlationId: "c1");
        var meta2 = new MetaInfo(correlationId: "c2");

        var successResult = Result<int>.Success(1, meta1).WithMeta(meta2);
        var failureResult = Result<int>.Fail(new Error("E", "m"), meta1).WithMeta(meta2);

        Assert.Multiple(() =>
        {
            Assert.That(successResult.Meta, Is.EqualTo(meta2));
            Assert.That(failureResult.Meta, Is.EqualTo(meta2));
        });
    }

    [Test]
    public void WithMetaData_ShouldAddAdditionalData()
    {
        var result = Result<int>.Success(1);
        var next = result.WithMetaData("k", 123);

        Assert.That(next.Meta.Additional.ContainsKey("k"), Is.True);
        Assert.That(next.Meta.Additional["k"], Is.EqualTo(123));
    }

    [Test]
    public void WithTrace_Tuples_AddsEntries()
    {
        var meta = new MetaInfo().WithTrace(("corr", (object?)"abc"), ("code", (object?)200));

        Assert.Multiple(() =>
        {
            Assert.That(meta.Trace["corr"], Is.EqualTo("abc"));
            Assert.That(meta.Trace["code"], Is.EqualTo(200));
        });
    }

    [Test]
    public void WithTrace_KeyValuePairs_AddsEntries()
    {
        var entries = new[]
        {
            new KeyValuePair<string, object?>("k1", "v1"),
            new KeyValuePair<string, object?>("k2", 42)
        };

        var meta = new MetaInfo().WithTrace(entries);

        Assert.Multiple(() =>
        {
            Assert.That(meta.Trace["k1"], Is.EqualTo("v1"));
            Assert.That(meta.Trace["k2"], Is.EqualTo(42));
        });
    }

    [Test]
    public void WithTrace_DoesNotOverwriteExistingKeys()
    {
        var meta = new MetaInfo().WithTrace(("k", (object?)"first"));
        var updated = meta.WithTrace(("k", (object?)"second"));

        Assert.That(updated.Trace["k"], Is.EqualTo("first"));
    }

    [Test]
    public void BuildLogScope_MergesTraceAndBusinessContext()
    {
        var meta = new MetaInfo().WithTrace(("corr", (object?)"abc"));
        var biz = new Dictionary<string, object?> { ["agreementId"] = "A1" };

        var scope = meta.BuildLogScope(biz);

        Assert.Multiple(() =>
        {
            Assert.That(scope["corr"], Is.EqualTo("abc"));
            Assert.That(scope["agreementId"], Is.EqualTo("A1"));
        });
    }

    [Test]
    public void BuildLogScope_WithNullBusinessContext_ReturnsTraceOnly()
    {
        var meta = new MetaInfo().WithTrace(("k", (object?)"v"));

        var scope = meta.BuildLogScope();

        Assert.That(scope.Count, Is.EqualTo(1));
        Assert.That(scope["k"], Is.EqualTo("v"));
    }

    [Test]
    public void BuildLogScope_WithEmptyMeta_ReturnsEmptyDictionary()
    {
        var meta = new MetaInfo();

        var scope = meta.BuildLogScope();

        Assert.That(scope, Is.Empty);
    }

    [Test]
    public void MetaInfo_Trace_DefaultsToEmpty()
    {
        var meta = new MetaInfo();
        Assert.That(meta.Trace, Is.Empty);
    }

    [Test]
    public void MetaInfo_CorrelationId_IsSetByConstructor()
    {
        var meta = new MetaInfo(correlationId: "test-corr");
        Assert.That(meta.CorrelationId, Is.EqualTo("test-corr"));
    }
}
