using DotnetForge.Application.Greetings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Application.Tests.Greetings;

[TestClass]
public sealed class GreetingApplicationServiceTests
{
    private readonly GreetingApplicationService _service = new(new GreetingRequestValidator());

    [TestMethod]
    public async Task CreateGreetingAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        var result = await _service.CreateGreetingAsync(new GreetingRequest("Deniz"));

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Error);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("Deniz", result.Value.Name);
        StringAssert.Contains(result.Value.Message, "Deniz");
    }

    [TestMethod]
    public async Task CreateGreetingAsync_ReturnsValidationFailure_WhenRequestIsInvalid()
    {
        var result = await _service.CreateGreetingAsync(new GreetingRequest(string.Empty));

        Assert.IsTrue(result.IsFailure);
        Assert.IsNotNull(result.Error);
        Assert.AreEqual("validation.failed", result.Error.Code);
        Assert.IsNotNull(result.ValidationErrors);
        Assert.IsTrue(result.ValidationErrors.ContainsKey("Name"));
    }

    [TestMethod]
    public async Task CreateGreetingAsync_TrimsNameThroughDomainInteraction()
    {
        var result = await _service.CreateGreetingAsync(new GreetingRequest("  Deniz  "));

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("Deniz", result.Value.Name);
        Assert.AreEqual("Hello, Deniz!", result.Value.Message);
    }
}
