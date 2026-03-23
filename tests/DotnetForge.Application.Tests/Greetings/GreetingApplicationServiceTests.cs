using DotnetForge.Application.Greetings;
using DotnetForge.Domain.Greetings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Application.Tests.Greetings;

[TestClass]
public sealed class GreetingApplicationServiceTests
{
    private readonly InMemoryGreetingRepository _repository = new();
    private readonly InMemoryGreetingCache _cache = new();
    private GreetingApplicationService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _repository.Clear();
        _cache.Clear();
        _service = new GreetingApplicationService(new GreetingRequestValidator(), _repository, _cache);
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
        Assert.AreEqual(1, _cache.SetCount);
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
    public async Task GetGreetingByIdAsync_ReturnsGreeting_WhenGreetingExistsOutsideCache()
    {
        var created = await _service.CreateGreetingAsync(new GreetingRequest("Deniz"));
        _cache.Clear();

        var result = await _service.GetGreetingByIdAsync(created.Value!.Id);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(created.Value.Id, result.Value.Id);
        Assert.AreEqual(1, _repository.GetByIdCount);
    }

    [TestMethod]
    public async Task GetGreetingByIdAsync_UsesCache_WhenGreetingIsAlreadyCached()
    {
        var created = await _service.CreateGreetingAsync(new GreetingRequest("Deniz"));
        _repository.ResetReadCount();

        var firstRead = await _service.GetGreetingByIdAsync(created.Value!.Id);
        var secondRead = await _service.GetGreetingByIdAsync(created.Value.Id);

        Assert.IsTrue(firstRead.IsSuccess);
        Assert.IsTrue(secondRead.IsSuccess);
        Assert.AreEqual(0, _repository.GetByIdCount);
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

        public int GetByIdCount { get; private set; }

        public Task AddAsync(Greeting greeting, CancellationToken cancellationToken = default)
        {
            _greetings[greeting.Id] = greeting;
            return Task.CompletedTask;
        }

        public Task<Greeting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GetByIdCount++;
            _greetings.TryGetValue(id, out var greeting);
            return Task.FromResult(greeting);
        }

        public void Clear()
        {
            _greetings.Clear();
            GetByIdCount = 0;
        }

        public void ResetReadCount()
        {
            GetByIdCount = 0;
        }
    }

    private sealed class InMemoryGreetingCache : IGreetingCache
    {
        private readonly Dictionary<Guid, GreetingResponse> _cache = [];

        public int SetCount { get; private set; }

        public Task<GreetingResponse?> GetOrCreateAsync(
            Guid id,
            Func<CancellationToken, Task<GreetingResponse?>> factory,
            CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(id, out var cached))
            {
                return Task.FromResult<GreetingResponse?>(cached);
            }

            return CreateAndStoreAsync(id, factory, cancellationToken);
        }

        public Task SetAsync(GreetingResponse response, CancellationToken cancellationToken = default)
        {
            _cache[response.Id] = response;
            SetCount++;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _cache.Remove(id);
            return Task.CompletedTask;
        }

        public void Clear()
        {
            _cache.Clear();
            SetCount = 0;
        }

        private async Task<GreetingResponse?> CreateAndStoreAsync(
            Guid id,
            Func<CancellationToken, Task<GreetingResponse?>> factory,
            CancellationToken cancellationToken)
        {
            var response = await factory(cancellationToken);
            if (response is not null)
            {
                _cache[id] = response;
            }

            return response;
        }
    }
}
