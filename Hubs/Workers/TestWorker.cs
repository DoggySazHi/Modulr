using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modulr.Hubs.Models;

namespace Modulr.Hubs.Workers
{
    public class TestWorker : BackgroundService
    {
        private readonly ILogger<TestWorker> _logger;
        private readonly IHubContext<TestQueryHub, ITestClient> _clockHub;

        public TestWorker(ILogger<TestWorker> logger, IHubContext<TestQueryHub, ITestClient> clockHub)
        {
            _logger = logger;
            _clockHub = clockHub;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var r = new Random();
            while (!stoppingToken.IsCancellationRequested)
            {
                await _clockHub.Clients.All.ReceiveUpdate(r.NextDouble() + "");
                await Task.Delay(1000, stoppingToken);
            }
        }

        public async Task SendUpdate(string connectionID, string message)
        {
            await _clockHub.Clients.Client(connectionID).ReceiveUpdate(message);
        }
    }
}