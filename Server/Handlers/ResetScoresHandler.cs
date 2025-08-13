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
    public class ResetScoresHandler : IMessageHandler
    {
        private readonly ScoreData _scoreData;
        private readonly IWebSocketServer _server;

        public ResetScoresHandler(ScoreData scoreData, IWebSocketServer server)
        {
            _scoreData = scoreData;
            _server = server;
        }

        public async Task HandleMessageAsync(WebSocket socket, string message)
        {
            try
            {
                _scoreData.TeamAScore = 0;
                _scoreData.TeamBScore = 0;
                _scoreData.Counter = 0;
                _scoreData.Leaderboard.Clear();

                await BroadcastUpdate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling reset scores: {ex.Message}");
            }
        }

        private async Task BroadcastUpdate()
        {
            var json = JsonSerializer.Serialize(_scoreData);
            await _server.BroadcastAsync(json);
        }
    }
}
