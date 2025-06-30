using NUnit.Framework;
using FxResult.Core;
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
}
