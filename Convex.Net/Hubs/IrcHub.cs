using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Convex.Client.Services;
using Convex.Event;
using Convex.IRC.Component.Event;
using Microsoft.AspNetCore.SignalR;

namespace Convex.Client.Hubs
{
    public class IrcHub : Hub {

        public async Task UpdateClients(params string[] args) {
            await Clients.All.SendAsync("ReceiveUpdate", string.Join(' ', args));
        }
    }
}
