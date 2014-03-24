using log4net.Appender;
using log4net.Core;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LogicStructure.WatchR.Appenders
{
    public class WatchrAppender : AppenderSkeleton
    {
        protected readonly Lazy<HubConnection> hubConnection = new Lazy<HubConnection>(() =>
        {
            return new HubConnection("http://michaeltroth.com/watchr");
        });
        protected IHubProxy eventHub;
        protected bool canProcess = false;
        protected readonly Guid? appId = null;
        protected Timer reconnectTimer = new Timer(5 * 1000);

        public WatchrAppender()
        {
            var appIdSetting = ConfigurationManager.AppSettings["watchr.appid"];

            // check local configuration to enusure appid is specified
            if (appIdSetting != null)
            {
                Guid appIdTemp = Guid.Empty;
                if (Guid.TryParse(appIdSetting, out appIdTemp))
                {
                    canProcess = true;
                    appId = appIdTemp;
                }
            }

            // create the connection.
            if (canProcess)
            {
                eventHub = hubConnection.Value.CreateHubProxy("EventHub");
                hubConnection.Value.Start().ContinueWith(StartConnection()).Wait();
                hubConnection.Value.Reconnecting += Value_Reconnecting;
                hubConnection.Value.Closed += Value_Closed;
                hubConnection.Value.Reconnected += Value_Reconnected;
                reconnectTimer.Elapsed += reconnectTimer_Elapsed;
            }

            if (canProcess && appId.HasValue && hubConnection.Value.State == ConnectionState.Connected)
            {
                eventHub.Invoke("RegisterApplication", appId).ContinueWith(RegisterApplication());
            }
        }

        void Value_Reconnected()
        {
            canProcess = true;
            reconnectTimer.Enabled = false;
            if (canProcess && appId.HasValue && hubConnection.Value.State == ConnectionState.Connected)
            {
                eventHub.Invoke("RegisterApplication", appId).ContinueWith(RegisterApplication());
            }
        }

        void reconnectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            hubConnection.Value.Start().ContinueWith(StartConnection()).Wait();
        }

        void Value_Closed()
        {
            reconnectTimer.Enabled = true;
        }

        void Value_Reconnecting()
        {
            hubConnection.Value.Start().ContinueWith(StartConnection()).Wait();
        }



        private Action<Task> StartConnection()
        {
            return task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("Can not talk to remote resource! Aborting. Check that you can connect to watchr.net");
                    canProcess = false;
                }
                else
                {
                    Console.WriteLine("Connected");
                    reconnectTimer.Enabled = false;
                    canProcess = true;
                }

            };
        }

        private Action<Task> RegisterApplication()
        {
            return task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("there was an issue {0}", task.Exception.GetBaseException());
                    canProcess = false;
                }
                else
                {
                    Console.WriteLine("Send Succeeded!");
                }
            }
                                    ;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (hubConnection.Value.State == ConnectionState.Connected || hubConnection.Value.State == ConnectionState.Reconnecting)
            {
                try
                {
                    eventHub.Invoke("SendUpdate", appId, this.RenderLoggingEvent(loggingEvent), loggingEvent.Level.Name.ToLower()).Wait();
                    System.Threading.Thread.Sleep(2000);
                }
                catch (AggregateException ae)
                {
                    canProcess = false;
                    Console.WriteLine("Service has gone offline! Sending events has been discontinued for now");
                }
            }

        }
    }
}