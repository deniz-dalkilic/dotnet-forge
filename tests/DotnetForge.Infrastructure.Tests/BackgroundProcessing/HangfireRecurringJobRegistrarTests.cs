using System.Linq.Expressions;
using DotnetForge.Infrastructure.BackgroundProcessing;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Infrastructure.Tests.BackgroundProcessing;

[TestClass]
public sealed class HangfireRecurringJobRegistrarTests
{
    [TestMethod]
    public async Task RegisterAsync_RegistersOnlyEnabledDefinitions()
    {
        var enabledDefinition = new RecordingRecurringJobDefinition("enabled-job", isEnabled: true);
        var disabledDefinition = new RecordingRecurringJobDefinition("disabled-job", isEnabled: false);
        var scheduler = new RecordingRecurringJobScheduler();

        var registrar = new HangfireRecurringJobRegistrar(
            new IRecurringJobDefinition[] { enabledDefinition, disabledDefinition },
            scheduler,
            Microsoft.Extensions.Options.Options.Create(new HangfireOptions()),
            NullLogger<HangfireRecurringJobRegistrar>.Instance);

        await registrar.RegisterAsync();

        CollectionAssert.AreEqual(new[] { "enabled-job" }, scheduler.RegisteredJobIds);
        Assert.AreEqual(1, enabledDefinition.RegisterCallCount);
        Assert.AreEqual(0, disabledDefinition.RegisterCallCount);
    }

    [TestMethod]
    public async Task RegisterAsync_RespectsCancellationBeforeRegisteringNextDefinition()
    {
        var firstDefinition = new RecordingRecurringJobDefinition("first", isEnabled: true);
        var secondDefinition = new RecordingRecurringJobDefinition("second", isEnabled: true);
        var scheduler = new RecordingRecurringJobScheduler();

        var registrar = new HangfireRecurringJobRegistrar(
            new IRecurringJobDefinition[] { firstDefinition, secondDefinition },
            scheduler,
            Microsoft.Extensions.Options.Options.Create(new HangfireOptions()),
            NullLogger<HangfireRecurringJobRegistrar>.Instance);

        using var cancellationTokenSource = new CancellationTokenSource();
        firstDefinition.BeforeRegister = () => cancellationTokenSource.Cancel();

        try
        {
            await registrar.RegisterAsync(cancellationTokenSource.Token);
            Assert.Fail("Expected OperationCanceledException to be thrown.");
        }
        catch (OperationCanceledException)
        {
            // expected
        }

        CollectionAssert.AreEqual(new[] { "first" }, scheduler.RegisteredJobIds);
        Assert.AreEqual(1, firstDefinition.RegisterCallCount);
        Assert.AreEqual(0, secondDefinition.RegisterCallCount);
    }

    private sealed class RecordingRecurringJobDefinition : IRecurringJobDefinition
    {
        private readonly bool _isEnabled;

        public RecordingRecurringJobDefinition(string recurringJobId, bool isEnabled)
        {
            RecurringJobId = recurringJobId;
            _isEnabled = isEnabled;
        }

        public string RecurringJobId { get; }

        public int RegisterCallCount { get; private set; }

        public Action? BeforeRegister { get; set; }

        public bool IsEnabled(HangfireOptions options) => _isEnabled;

        public void Register(IRecurringJobScheduler scheduler, HangfireOptions options)
        {
            BeforeRegister?.Invoke();
            RegisterCallCount++;

            scheduler.AddOrUpdate<object>(
                RecurringJobId,
                _ => NoOp(),
                options.RecurringJobs.HeartbeatCron);
        }

        private static void NoOp()
        {
        }
    }

    private sealed class RecordingRecurringJobScheduler : IRecurringJobScheduler
    {
        public List<string> RegisteredJobIds { get; } = new();

        public void AddOrUpdate<TJob>(
            string recurringJobId,
            Expression<Action<TJob>> methodCall,
            string cronExpression)
            where TJob : class
        {
            RegisteredJobIds.Add(recurringJobId);
        }
    }
}
