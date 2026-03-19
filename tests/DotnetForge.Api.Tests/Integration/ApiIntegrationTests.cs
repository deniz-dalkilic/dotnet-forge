using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotnetForge.Application.Greetings;
using DotnetForge.Domain.Greetings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:ApplyMigrationsOnStartup"] = "false"
                    });
                });
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IGreetingRepository>();
                    services.AddSingleton<TestGreetingRepository>();
                    services.AddScoped<IGreetingRepository>(serviceProvider =>
                        serviceProvider.GetRequiredService<TestGreetingRepository>());
                });
            });
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _client = _factory!.CreateClient();
        _factory.Services.GetRequiredService<TestGreetingRepository>().Clear();
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

        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        StringAssert.Contains(content, "Hello, Deniz!");
    }

    [TestMethod]
    public async Task GetGreetingById_ReturnsGreetingPayload_WhenGreetingExists()
    {
        var createResponse = await _client!.PostAsJsonAsync("/api/greetings", new { name = "Deniz" });
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<GreetingContract>();

        var response = await _client.GetAsync($"/api/greetings/{createdPayload!.Id}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(content, createdPayload.Id.ToString());
    }

    [TestMethod]
    public async Task GetGreetingById_ReturnsNotFoundProblem_WhenGreetingDoesNotExist()
    {
        var response = await _client!.GetAsync($"/api/greetings/{Guid.NewGuid()}");
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(content);
        Assert.AreEqual("greetings.not_found", document.RootElement.GetProperty("errorCode").GetString());
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

    private sealed record GreetingContract(Guid Id, string Name, string Message, DateTimeOffset CreatedAtUtc);

    private sealed class TestGreetingRepository : IGreetingRepository
    {
        private readonly Dictionary<Guid, Greeting> _greetings = [];

        public Task AddAsync(Greeting greeting, CancellationToken cancellationToken = default)
        {
            _greetings[greeting.Id] = greeting;
            return Task.CompletedTask;
        }

        public Task<Greeting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _greetings.TryGetValue(id, out var greeting);
            return Task.FromResult(greeting);
        }

        public void Clear()
        {
            _greetings.Clear();
        }
    }
}
