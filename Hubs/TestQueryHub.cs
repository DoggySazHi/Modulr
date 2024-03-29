﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Modulr.Hubs.Models;

namespace Modulr.Hubs;

public class TestQueryHub : Hub<ITestClient>
{
    public async Task Update(string output)
    {
        await Clients.Caller.ReceiveUpdate(output);
    }
        
    public string GetConnectionId() => Context.ConnectionId;
}