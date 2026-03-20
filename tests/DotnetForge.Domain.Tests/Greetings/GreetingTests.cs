using DotnetForge.Domain.Greetings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Domain.Tests.Greetings;

[TestClass]
public sealed class GreetingTests
{
    [TestMethod]
    public void Create_NormalizesNameAndBuildsMessage()
    {
        var now = new DateTimeOffset(2026, 3, 20, 12, 0, 0, TimeSpan.Zero);

        var greeting = Greeting.Create("  Deniz  ", now);

        Assert.AreNotEqual(Guid.Empty, greeting.Id);
        Assert.AreEqual("Deniz", greeting.Name);
        Assert.AreEqual("Hello, Deniz!", greeting.Message);
        Assert.AreEqual(now, greeting.CreatedAtUtc);
    }

    [TestMethod]
    public void Create_PreservesInternalSpacingWhileTrimmingEdges()
    {
        var greeting = Greeting.Create("  Deniz Dalkilic  ", DateTimeOffset.UtcNow);

        Assert.AreEqual("Deniz Dalkilic", greeting.Name);
        Assert.AreEqual("Hello, Deniz Dalkilic!", greeting.Message);
    }
}
