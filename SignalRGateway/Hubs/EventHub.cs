using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SignalRGateway.Hubs
{
    public class EventHub : Hub
    {
        public async Task RegisterApplication(string guid)
        {
            await Groups.Add(Context.ConnectionId, guid);
            await Clients.Group(guid).ClientUpdate("Application Connected and is sending events!");
        }

        public async Task RegisterWatcher(string guidWatching)
        {
            string message = string.Format("Team Member is watching");
            await Groups.Add(Context.ConnectionId, guidWatching);
            await Clients.Group(guidWatching).ClientUpdate(message);
        }

        

        public async Task SendUpdate(string guid, string message, string type)
        {
            await this.Clients.Group(guid).LogMessage(message, type);
        }


    }
}