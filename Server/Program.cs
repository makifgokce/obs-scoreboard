using ScoreboardServer.Core.Interfaces;
using ScoreboardServer.Core.Models;
using ScoreboardServer.Core.Services;
using ScoreboardServer.Handlers;

namespace ScoreboardServer
{
    public class Program
    {
        private static int _totalConnections = 0;
        private static int _activeConnections = 0;
        static async Task Main(string[] args)
        {
            Console.WriteLine("WebSocket sunucusu başlatılıyor...");

            try
            {
                // Bağımlılıkları oluştur
                var scoreData = new ScoreData();
                IWebSocketServer server = new WebSocketServer("http://localhost:1453/", scoreData);

                // Mesaj işleyicileri kaydet
                server.RegisterHandler("UpdateScore", new ScoreUpdateHandler(scoreData, server));
                server.RegisterHandler("UpdateCounter", new CounterUpdateHandler(scoreData, server));
                server.RegisterHandler("UpdateLeaderboard", new LeaderboardUpdateHandler(scoreData, server));
                server.RegisterHandler("ResetScores", new ResetScoresHandler(scoreData, server));
                server.RegisterHandler("Ping", new PingHandler(scoreData, server));
                server.RegisterHandler("UpdateTeamName", new TeamNameHandler(scoreData, server));

                // Sunucuyu başlat
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sunucu başlatılırken hata: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.ReadKey();
            }
        }
        public static void IncrementConnectionCount()
        {
            _totalConnections++;
            _activeConnections++;
            Console.WriteLine($"Yeni bağlantı. Toplam: {_totalConnections}, Aktif: {_activeConnections}");
        }

        public static void DecrementConnectionCount()
        {
            _activeConnections--;
            Console.WriteLine($"Bağlantı sonlandırıldı. Toplam: {_totalConnections}, Aktif: {_activeConnections}");
        }
    }
}
