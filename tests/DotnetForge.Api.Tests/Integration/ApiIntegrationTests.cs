using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Api.Tests.Integration;

[TestClass]
public sealed class ApiIntegrationTests
{
    private static WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        _factory = new WebApplicationFactory<Program>();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _client = _factory!.CreateClient();
    }

    [TestMethod]
    public async Task GetRoot_ReturnsOkJson()
    {
        var response = await _client!.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var payload = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(payload, "DotnetForge.Api");
    }

    [TestMethod]
    public async Task GetPing_ReturnsPong()
    {
        var response = await _client!.GetAsync("/ping");

        Assert.IsTrue(response.IsSuccessStatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(payload.ToLowerInvariant(), "pong");
    }

    [TestMethod]
    public async Task IncomingCorrelationId_IsReturnedInResponseHeader()
    {
        const string correlationId = "integration-test-correlation-id";
        using var request = new HttpRequestMessage(HttpMethod.Get, "/ping");
        request.Headers.Add("X-Correlation-ID", correlationId);

        var response = await _client!.SendAsync(request);

        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.IsTrue(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        CollectionAssert.Contains(values.ToList(), correlationId);
    }

    [TestMethod]
    public async Task MissingCorrelationId_GeneratesAndReturnsHeader()
    {
        var response = await _client!.GetAsync("/ping");

        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.IsTrue(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.IsFalse(string.IsNullOrWhiteSpace(values.FirstOrDefault()));
    }
}
