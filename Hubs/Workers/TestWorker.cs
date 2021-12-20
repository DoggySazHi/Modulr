using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Modulr.Hubs.Models;

namespace Modulr.Hubs.Workers;

public class TestWorker : BackgroundService
{
    private readonly IHubContext<TestQueryHub, ITestClient> _testHub;

    public TestWorker(IHubContext<TestQueryHub, ITestClient> testHub)
    {
        _testHub = testHub;
    }
        
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task SendUpdate(string connectionID, string message)
    {
        await _testHub.Clients.Client(connectionID).ReceiveUpdate(message);
    }
}