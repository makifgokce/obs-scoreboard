using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Interfaces
{
    public interface IWebSocketServer
    {
        Task StartAsync();
        void RegisterHandler(string messageType, IMessageHandler handler);
        Task BroadcastAsync(string message);
        Task SendToClientAsync(WebSocket socket, string message);
    }
}
