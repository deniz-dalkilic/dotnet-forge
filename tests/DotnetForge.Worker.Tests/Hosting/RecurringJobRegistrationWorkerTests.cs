using DotnetForge.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Worker.Tests.Hosting;

[TestClass]
public sealed class RecurringJobRegistrationWorkerTests
{
    [TestMethod]
    public async Task StartAsync_TriggersRecurringJobRegistration()
    {
        var registrar = new RecordingRecurringJobRegistrar();
        var worker = new RecurringJobRegistrationWorker(registrar, NullLogger<RecurringJobRegistrationWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        Assert.AreEqual(1, registrar.RegisterCallCount);
    }

    private sealed class RecordingRecurringJobRegistrar : DotnetForge.Infrastructure.BackgroundProcessing.IRecurringJobRegistrar
    {
        public int RegisterCallCount { get; private set; }

        public Task RegisterAsync(CancellationToken cancellationToken = default)
        {
            RegisterCallCount++;
            return Task.CompletedTask;
        }
    }
}
