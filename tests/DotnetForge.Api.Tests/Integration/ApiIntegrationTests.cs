using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
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
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
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
    public async Task CreateGreeting_ReturnsGreetingPayload_WhenRequestIsValid()
    {
        var response = await _client!.PostAsJsonAsync("/api/greetings", new { name = "Deniz" });
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        StringAssert.Contains(content, "Hello, Deniz!");
    }

    [TestMethod]
    public async Task CreateGreeting_ReturnsValidationProblem_WhenRequestIsInvalid()
    {
        var response = await _client!.PostAsJsonAsync("/api/greetings", new { name = "" });
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(content);
        Assert.AreEqual("Validation failed", document.RootElement.GetProperty("title").GetString());
        Assert.IsTrue(document.RootElement.TryGetProperty("errors", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("correlationId", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("traceId", out _));
        Assert.AreEqual("validation.failed", document.RootElement.GetProperty("errorCode").GetString());
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

    [TestMethod]
    public async Task ValidationException_ReturnsProblemDetailsWith400()
    {
        var response = await _client!.GetAsync("/__diagnostics/errors/validation");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(content);
        Assert.AreEqual(400, document.RootElement.GetProperty("status").GetInt32());
        Assert.AreEqual("Validation failed", document.RootElement.GetProperty("title").GetString());
        Assert.IsTrue(document.RootElement.TryGetProperty("errors", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("correlationId", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("traceId", out _));
    }

    [TestMethod]
    public async Task UnexpectedException_ReturnsProblemDetailsWith500()
    {
        var response = await _client!.GetAsync("/__diagnostics/errors/unexpected");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(content);
        Assert.AreEqual(500, document.RootElement.GetProperty("status").GetInt32());
        Assert.AreEqual("Unexpected server error", document.RootElement.GetProperty("title").GetString());
        Assert.IsTrue(document.RootElement.TryGetProperty("correlationId", out _));
        Assert.IsTrue(document.RootElement.TryGetProperty("traceId", out _));
    }
}
