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
    public class LeaderboardUpdateHandler : IMessageHandler
    {
        private readonly ScoreData _scoreData;
        private readonly IWebSocketServer _server;

        public LeaderboardUpdateHandler(ScoreData scoreData, IWebSocketServer server)
        {
            _scoreData = scoreData;
            _server = server;
        }

        public async Task HandleMessageAsync(WebSocket socket, string message)
        {
            try
            {
                var update = JsonSerializer.Deserialize<LeaderboardUpdate>(message);
                if (update != null)
                {
                    if (update.ActionType == "LeaderboardAdd")
                    {
                        _scoreData.Leaderboard.Add(update.Entry);
                    }
                    else if (update.ActionType == "LeaderboardUpdate")
                    {
                        var existing = _scoreData.Leaderboard.FirstOrDefault(e => e.Id == update.Entry.Id);
                        if (existing != null)
                        {
                            existing.Name = update.Entry.Name;
                            existing.Color = update.Entry.Color;
                            existing.Score = update.Entry.Score;
                        }
                    }
                    else if (update.ActionType == "LeaderboardRemove")
                    {
                        _scoreData.Leaderboard.RemoveAll(e => e.Id == update.Entry.Id);
                    }

                    await BroadcastUpdate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling leaderboard update: {ex.Message}");
            }
        }

        private async Task BroadcastUpdate()
        {
            var json = JsonSerializer.Serialize(_scoreData);
            await _server.BroadcastAsync(json);
        }
    }
}
