using ScoreboardServer.Core.Interfaces;
using ScoreboardServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScoreboardServer.Handlers
{
    public class ScoreUpdateHandler : IMessageHandler
    {
        private readonly ScoreData _scoreData;
        private readonly IWebSocketServer _server;

        public ScoreUpdateHandler(ScoreData scoreData, IWebSocketServer server)
        {
            _scoreData = scoreData;
            _server = server;
        }

        public async Task HandleMessageAsync(WebSocket socket, string message)
        {
            try
            {
                var update = JsonSerializer.Deserialize<ScoreUpdate>(message);
                if (update != null)
                {
                    if (update.Team == "A") _scoreData.TeamAScore = update.Score;
                    else if (update.Team == "B") _scoreData.TeamBScore = update.Score;

                    await BroadcastUpdate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling score update: {ex.Message}");
            }
        }

        private async Task BroadcastUpdate()
        {
            var json = JsonSerializer.Serialize(_scoreData);
            await _server.BroadcastAsync(json);
        }
    }
}
