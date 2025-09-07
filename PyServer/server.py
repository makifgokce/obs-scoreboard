import asyncio
import json
import websockets
from typing import Dict, List, Set, Any, Optional
from dataclasses import dataclass, asdict
from datetime import datetime
import uuid

server = None
# Veri modelleri
@dataclass
class LeaderboardEntry:
    Id: str
    Name: str
    Score: int
    Color: str

@dataclass
class ScoreData:
    TeamAName: str = "Takım A"
    TeamBName: str = "Takım B"
    TeamAScore: int = 0
    TeamBScore: int = 0
    TeamAColor:str = "#2196F3"
    TeamBColor:str = "#FF5722"
    CounterName: str = "Sayaç"
    Counter: int = 0
    Leaderboard: List[LeaderboardEntry] = None

    def __post_init__(self):
        if self.Leaderboard is None:
            self.Leaderboard = []

    def update_score(self, team: str, score: int, color: str) -> None:
        if team == "A":
            self.TeamAScore  = score
            self.TeamAColor  = color
            print(f"Takım A skoru güncellendi: {score}")
        elif team == "B":
            self.TeamBScore  = score
            self.TeamBColor  = color
            print(f"Takım B skoru güncellendi: {score}")

    def update_counter(self, value: int) -> None:
        self.Counter = value
        print(f"Sayaç güncellendi: {value}")

    def update_leaderboard(self, action_type: str, entry: LeaderboardEntry) -> None:
        if action_type == "LeaderboardAdd":
            self.Leaderboard.append(entry)
            print(f"Lider tablosuna eklendi: {entry.Name} - {entry.Score} - {entry.Color}")
        elif action_type == "LeaderboardUpdate":
            existing = next((e for e in self.Leaderboard if e.Id == entry.Id), None)
            if existing:
                existing.Name = entry.Name
                existing.Score = entry.Score
                existing.Color = entry.Color
                print(f"Lider tablosu güncellendi: {entry.Name} - {entry.Score}")
        elif action_type == "LeaderboardRemove":
            self.Leaderboard = [e for e in self.Leaderboard if e.Id != entry.Id]
            print(f"Lider tablosundan kaldırıldı: {entry.Id}")

    def reset_scores(self) -> None:
        self.TeamAName: str = "Takım A"
        self.TeamBName: str = "Takım B"
        self.TeamAScore: int = 0
        self.TeamBScore: int = 0
        self.TeamAColor:str = "#2196F3"
        self.TeamBColor:str = "#FF5722"
        self.CounterName: str = "Sayaç"
        self.Counter: int = 0
        print("Tüm skorlar sıfırlandı")

    def to_dict(self) -> Dict[str, Any]:
        return {
            "TeamAName": self.TeamAName,
            "TeamBName": self.TeamBName,
            "TeamAScore": self.TeamAScore,
            "TeamBScore": self.TeamBScore,
            "TeamAColor": self.TeamAColor,
            "TeamBColor": self.TeamBColor,
            "CounterName": self.CounterName,
            "Counter": self.Counter,
            "Leaderboard": [asdict(entry) for entry in self.Leaderboard]
        }

# Sunucu sınıfı
class WebSocketServer:
    def __init__(self, host: str, port: int):
        self.host = host
        self.port = port
        self.clients: Set[websockets.ServerConnection] = set()
        self.score_data = ScoreData()

    async def register_client(self, websocket: websockets.ServerConnection, client_type: str) -> None:
        self.clients.add(websocket)
        print(f"{client_type} istemcisi bağlandı. Toplam istemci: {len(self.clients)}")
        await self.send_to_client(websocket, self.score_data.to_dict())

    async def unregister_client(self, websocket: websockets.ServerConnection) -> None:
        if websocket in self.clients:
            self.clients.remove(websocket)
            print(f"İstemci bağlantısı kesildi. Toplam istemci: {len(self.clients)}")

    async def send_to_client(self, websocket: websockets.ServerConnection, data: Dict[str, Any]) -> None:
        try:
            await websocket.send(json.dumps(data))
        except websockets.exceptions.ConnectionClosed:
            await self.unregister_client(websocket)

    async def broadcast(self, data: Dict[str, Any]) -> None:
        if self.clients:
            await asyncio.gather(
                *[self.send_to_client(client, data) for client in self.clients],
                return_exceptions=True
            )

    async def handle_message(self, websocket: websockets.ServerConnection, message: str) -> None:
        try:
            data = json.loads(message)
            action = data.get("Action")

            if action == "UpdateScore":
                team = data.get("Team")
                score = data.get("Score")
                color = data.get("Color")
                if team and score is not None:
                    self.score_data.update_score(team, score, color)
                    await self.broadcast(self.score_data.to_dict())

            elif action == "UpdateCounter":
                value = data.get("Value")
                name = data.get("Name")
                if value is not None:
                    self.score_data.update_counter(value)
                if name is not None:
                    self.score_data.CounterName = name
                await self.broadcast(self.score_data.to_dict())

            elif action == "UpdateLeaderboard":
                action_type = data.get("ActionType")
                entry_data = data.get("Entry")
                if action_type and entry_data:
                    entry = LeaderboardEntry(
                        Id=entry_data.get("Id", self.generate_id()),
                        Name=entry_data.get("Name", ""),
                        Score=entry_data.get("Score", 0),
                        Color=entry_data.get("Color", 0)
                    )
                    self.score_data.update_leaderboard(action_type, entry)
                    await self.broadcast(self.score_data.to_dict())

            elif action == "ResetScores":
                self.score_data.reset_scores()
                await self.broadcast(self.score_data.to_dict())

            elif action == "UpdateTeamName":
                team = data.get("Team")
                name = data.get("Name")
                if(team == "A"):
                    self.score_data.TeamAName = name
                else:
                    self.score_data.TeamBName = name

                print(f"Takım  {team} adı {name} olarak değiştirildi")
                await self.broadcast(self.score_data.to_dict())
            elif action == "Ping":
                # Ping-pong mekanizması
                await self.broadcast(self.score_data.to_dict())

        except json.JSONDecodeError:
            print(f"Geçersiz JSON formatı: {message}")
        except Exception as e:
            print(f"Mesaj işlenirken hata: {e}")

    def generate_id(self) -> str:
        return str(uuid.uuid4())

    async def client_handler(self, websocket: websockets.ServerConnection) -> None:
        client_type = "unknown"
        try:
            client_type = websocket.request.path
            await self.register_client(websocket, client_type)

            async for message in websocket:
                await self.handle_message(websocket, message)

        except websockets.exceptions.ConnectionClosed:
            pass
        finally:
            await self.unregister_client(websocket)

    async def start(self) -> None:
        print(f"WebSocket sunucusu başlatılıyor. ws://{self.host}:{self.port}")
        async with websockets.serve(self.client_handler, self.host, self.port):
            print("Sunucu çalışıyor...")
            await asyncio.Future()

if __name__ == "__main__":
    server = WebSocketServer("localhost", 1453)
    try:
        asyncio.run(server.start())
    except KeyboardInterrupt:
        print("Sunucu durduruldu.")