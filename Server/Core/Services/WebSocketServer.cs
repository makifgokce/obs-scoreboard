using ScoreboardServer.Core.Interfaces;
using ScoreboardServer.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Services
{
    public class WebSocketServer : IWebSocketServer
    {
        private readonly HttpListener _listener;
        private readonly ConcurrentDictionary<WebSocket, string> _clients = new ConcurrentDictionary<WebSocket, string>();
        private readonly ConcurrentDictionary<string, IMessageHandler> _handlers = new ConcurrentDictionary<string, IMessageHandler>();
        private readonly ScoreData _scoreData;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(10, 10);

        public WebSocketServer(string url, ScoreData scoreData)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _scoreData = scoreData;
        }
        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine($"Sunucu başlatıldı. {_listener.Prefixes.First()}");

            while (true)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    // Her bağlantıyı ayrı bir task'ta işle
                    _ = Task.Run(() => ProcessWebSocketRequest(context));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"İstek işlenirken hata: {ex.Message}");
                }
            }
        }

        public void RegisterHandler(string messageType, IMessageHandler handler)
        {
            _handlers[messageType] = handler;
        }

        public async Task BroadcastAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var deadClients = new List<WebSocket>();

            // _clients.ToList() ile kopya oluştur ve null kontrolü yap
            foreach (var client in _clients.ToList())
            {
                if (client.Key != null && client.Key.State == WebSocketState.Open)
                {
                    try
                    {
                        await client.Key.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"İstemciye mesaj gönderilirken hata: {ex.Message}");
                        deadClients.Add(client.Key);
                    }
                }
                else
                {
                    deadClients.Add(client.Key);
                }
            }

            // Ölü bağlantıları temizle (null kontrolü ile)
            foreach (var deadClient in deadClients)
            {
                if (deadClient != null)
                {
                    _clients.TryRemove(deadClient, out _);
                    Console.WriteLine("Ölü bağlantı temizlendi");
                }
            }
        }

        public async Task SendToClientAsync(WebSocket socket, string message)
        {
            // Null kontrolü ekle
            if (socket == null)
            {
                Console.WriteLine("SendToClientAsync: Socket parametresi null");
                return;
            }

            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"İstemciye mesaj gönderilirken hata: {ex.Message}");

                    // Hata durumunda istemciyi listeden güvenli bir şekilde çıkar
                    try
                    {
                        _clients.TryRemove(socket, out _);
                        Console.WriteLine("Hata nedeniyle istemci listeden çıkarıldı");
                    }
                    catch (Exception removeEx)
                    {
                        Console.WriteLine($"İstemci listeden çıkarılırken hata: {removeEx.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Socket açık değil, durum: {socket.State}");

                // Kapalı veya kapatılmış bağlantıları listeden çıkar
                try
                {
                    _clients.TryRemove(socket, out _);
                    Console.WriteLine("Kapalı bağlantı listeden çıkarıldı");
                }
                catch (Exception removeEx)
                {
                    Console.WriteLine($"Kapalı bağlantı listeden çıkarılırken hata: {removeEx.Message}");
                }
            }
        }

        private async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            await _connectionLock.WaitAsync();
            WebSocket socket = null; // Değişkeni başlangıçta null olarak tanımla
            try
            {
                // CORS başlıklarını ekle
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                // Preflight isteğini kontrol et
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                    return;
                }

                var wsContext = await context.AcceptWebSocketAsync(null);
                socket = wsContext.WebSocket;

                if (socket == null)
                {
                    Console.WriteLine("WebSocket oluşturulamadı");
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                    return;
                }

                // İstemci türünü belirle (scoreboard veya control)
                var clientType = context.Request.Url.LocalPath;
                _clients.TryAdd(socket, clientType);

                Console.WriteLine($"{clientType} istemcisi bağlandı. Toplam istemci: {_clients.Count}");

                // Mevcut verileri gönder
                await SendToClientAsync(socket, JsonSerializer.Serialize(_scoreData));

                // Mesajları dinle
                await ReceiveMessages(socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket bağlantısı kurulurken hata: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex}");

                if (socket != null)
                {
                    try
                    {
                        _clients.TryRemove(socket, out _);
                        if (socket.State == WebSocketState.Open)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Sunucu hatası", CancellationToken.None);
                        }
                    }
                    catch (Exception closeEx)
                    {
                        Console.WriteLine($"Bağlantı kapatılırken hata: {closeEx.Message}");
                    }
                }

                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ReceiveMessages(WebSocket socket)
        {
            var buffer = new byte[1024];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        var message = JsonSerializer.Deserialize<ClientMessage>(json);

                        if (message != null && _handlers.TryGetValue(message.Action, out var handler))
                        {
                            await handler.HandleMessageAsync(socket, json);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("İstemci bağlantıyı kapattı");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mesaj işlenirken hata: {ex.Message}");
                Console.WriteLine($"Hata detayı: {ex}");
                return;
            }
            finally
            {
                // Bağlantıyı kapat
                if (socket != null) // Null kontrolü ekle
                {
                    try
                    {
                        // İstemciyi sözlükten güvenli bir şekilde kaldır
                        if (_clients.ContainsKey(socket))
                        {
                            _clients.TryRemove(socket, out _);
                        }

                        if (socket.State == WebSocketState.Open)
                        {
                            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        }
                        else if (socket.State == WebSocketState.Aborted)
                        {
                            Console.WriteLine("Bağlantı zaten kapatılmış (Aborted)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Bağlantı kapatılırken hata: {ex.Message}");
                    }
                }
                Console.WriteLine($"İstemci bağlantısı sonlandırıldı. Toplam istemci: {_clients.Count}");
            }
        }
    }
}
