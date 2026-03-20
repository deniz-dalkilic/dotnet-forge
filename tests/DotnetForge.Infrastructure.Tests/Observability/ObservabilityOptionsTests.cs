using DotnetForge.Infrastructure.Observability;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Infrastructure.Tests.Observability;

[TestClass]
public sealed class ObservabilityOptionsTests
{
    [TestMethod]
    public void Defaults_ExposeSafeLocalDevelopmentBaseline()
    {
        var options = new ObservabilityOptions();

        Assert.IsTrue(options.Seq.Enabled);
        Assert.AreEqual("http://localhost:5341", options.Seq.ServerUrl);
        CollectionAssert.Contains(options.Redaction.SensitiveHeaders, "Authorization");
        Assert.IsFalse(options.OpenTelemetry.Otlp.Enabled);
    }
}
