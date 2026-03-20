using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotnetForge.Application.Greetings;
using DotnetForge.Domain.Greetings;
using DotnetForge.Infrastructure.BackgroundProcessing;
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
    private static ForgeApiFactory? _factory;
    private HttpClient? _client;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _)
    {
        _factory = new ForgeApiFactory();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _factory!.ResetState();
        _client = _factory.CreateClient();
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
    public async Task CreateGreeting_RoundTripsGreetingThroughReadEndpoint()
    {
        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/greetings")
        {
            Content = JsonContent.Create(new { name = "Deniz" })
        };
        createRequest.Headers.Add("X-Correlation-ID", "roundtrip-correlation-id");

        var createResponse = await _client!.SendAsync(createRequest);
        var createdPayload = await createResponse.Content.ReadFromJsonAsync<GreetingContract>();
        var readResponse = await _client.GetAsync($"/api/greetings/{createdPayload!.Id}");
        var readPayload = await readResponse.Content.ReadFromJsonAsync<GreetingContract>();

        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.AreEqual($"/api/greetings/{createdPayload.Id}", createResponse.Headers.Location?.OriginalString);
        Assert.AreEqual("roundtrip-correlation-id", createResponse.Headers.GetValues("X-Correlation-ID").Single());
        Assert.AreEqual(HttpStatusCode.OK, readResponse.StatusCode);
        Assert.IsNotNull(readPayload);
        Assert.AreEqual(createdPayload.Id, readPayload.Id);
        Assert.AreEqual("Deniz", readPayload.Name);
        Assert.AreEqual("Hello, Deniz!", readPayload.Message);
    }

    [TestMethod]
    public async Task GetGreetingById_ReturnsNotFoundProblem_WithCorrelationMetadata()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/greetings/{Guid.NewGuid()}");
        request.Headers.Add("X-Correlation-ID", "missing-greeting-correlation-id");

        var response = await _client!.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(content);
        Assert.AreEqual("missing-greeting-correlation-id", document.RootElement.GetProperty("correlationId").GetString());
        Assert.AreEqual("greetings.not_found", document.RootElement.GetProperty("errorCode").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("traceId").GetString()));
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
    public async Task FireAndForgetJobEndpoint_QueuesJobAndReturnsAccepted()
    {
        const string correlationId = "integration-job-correlation-id";
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/jobs/greetings/fire-and-forget")
        {
            Content = JsonContent.Create(new { greeting = "Hello from integration test" })
        };
        request.Headers.Add("X-Correlation-ID", correlationId);

        var response = await _client!.SendAsync(request);
        var payload = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        StringAssert.Contains(payload, "fire-and-forget");
        StringAssert.Contains(payload, correlationId);

        Assert.AreEqual(1, _factory!.Dispatcher.EnqueuedJobs.Count);
        Assert.AreEqual(correlationId, _factory.Dispatcher.EnqueuedJobs[0].CorrelationId);
    }

    [TestMethod]
    public async Task ScheduledJobEndpoint_QueuesJobAndClampsDelay_WhenRequestExceedsUpperBound()
    {
        var response = await _client!.PostAsJsonAsync("/api/jobs/greetings/scheduled", new
        {
            greeting = "Hello later",
            delaySeconds = 4000
        });

        Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
        Assert.AreEqual(1, _factory!.Dispatcher.ScheduledJobs.Count);
        Assert.AreEqual(TimeSpan.FromHours(1), _factory.Dispatcher.ScheduledJobs[0].Delay);
    }

    [TestMethod]
    public async Task JobEndpoints_ReturnProblemDetails_WhenQueuingIsDisabled()
    {
        using var disabledFactory = new ForgeApiFactory(new Dictionary<string, string?>
        {
            ["Hangfire:QueueJobsViaApi"] = "false"
        });
        using var client = disabledFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/jobs/greetings/fire-and-forget", new { greeting = "Hello" });
        var content = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(content);
        Assert.AreEqual("Background job queuing is disabled", document.RootElement.GetProperty("title").GetString());
        Assert.IsTrue(document.RootElement.TryGetProperty("correlationId", out _));
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

    private sealed class ForgeApiFactory : WebApplicationFactory<Program>
    {
        private readonly IReadOnlyDictionary<string, string?> _overrides;

        public ForgeApiFactory(IReadOnlyDictionary<string, string?>? overrides = null)
        {
            _overrides = overrides ?? new Dictionary<string, string?>();
        }

        public TestGreetingRepository Repository { get; } = new();

        public FakeBackgroundJobDispatcher Dispatcher { get; } = new();

        public void ResetState()
        {
            Repository.Clear();
            Dispatcher.Clear();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Database:ApplyMigrationsOnStartup"] = "false",
                    ["Hangfire:EnableDashboard"] = "false"
                };

                foreach (var pair in _overrides)
                {
                    settings[pair.Key] = pair.Value;
                }

                configurationBuilder.AddInMemoryCollection(settings);
            });
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IGreetingRepository>();
                services.AddSingleton(Repository);
                services.AddScoped<IGreetingRepository>(serviceProvider => serviceProvider.GetRequiredService<TestGreetingRepository>());

                services.RemoveAll<IBackgroundJobDispatcher>();
                services.AddSingleton(Dispatcher);
                services.AddSingleton<IBackgroundJobDispatcher>(serviceProvider => serviceProvider.GetRequiredService<FakeBackgroundJobDispatcher>());
            });
        }
    }

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

        public void Clear() => _greetings.Clear();
    }

    private sealed class FakeBackgroundJobDispatcher : IBackgroundJobDispatcher
    {
        public List<(string Greeting, string CorrelationId)> EnqueuedJobs { get; } = [];

        public List<(string Greeting, string CorrelationId, TimeSpan Delay)> ScheduledJobs { get; } = [];

        public string EnqueueGreeting(string greeting, string correlationId)
        {
            EnqueuedJobs.Add((greeting, correlationId));
            return Guid.NewGuid().ToString("n");
        }

        public string ScheduleGreeting(string greeting, string correlationId, TimeSpan delay)
        {
            ScheduledJobs.Add((greeting, correlationId, delay));
            return Guid.NewGuid().ToString("n");
        }

        public void Clear()
        {
            EnqueuedJobs.Clear();
            ScheduledJobs.Clear();
        }
    }
}
