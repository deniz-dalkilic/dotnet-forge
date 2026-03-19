using DotnetForge.Application.Greetings;
using DotnetForge.Domain.Greetings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Application.Tests.Greetings;

[TestClass]
public sealed class GreetingApplicationServiceTests
{
    private readonly InMemoryGreetingRepository _repository = new();
    private GreetingApplicationService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repository.Clear();
        _service = new GreetingApplicationService(new GreetingRequestValidator(), _repository);
    }

    [TestMethod]
    public async Task CreateGreetingAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        var result = await _service.CreateGreetingAsync(new GreetingRequest("Deniz"));

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Error);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual("Deniz", result.Value.Name);
        StringAssert.Contains(result.Value.Message, "Deniz");
        Assert.AreEqual(1, _repository.Count);
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
        Assert.AreEqual(0, _repository.Count);
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

    [TestMethod]
    public async Task GetGreetingByIdAsync_ReturnsGreeting_WhenGreetingExists()
    {
        var created = await _service.CreateGreetingAsync(new GreetingRequest("Deniz"));

        var result = await _service.GetGreetingByIdAsync(created.Value!.Id);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(created.Value.Id, result.Value.Id);
    }

    [TestMethod]
    public async Task GetGreetingByIdAsync_ReturnsNotFound_WhenGreetingDoesNotExist()
    {
        var result = await _service.GetGreetingByIdAsync(Guid.NewGuid());

        Assert.IsTrue(result.IsFailure);
        Assert.IsNotNull(result.Error);
        Assert.AreEqual("greetings.not_found", result.Error.Code);
    }

    private sealed class InMemoryGreetingRepository : IGreetingRepository
    {
        private readonly Dictionary<Guid, Greeting> _greetings = [];

        public int Count => _greetings.Count;

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
