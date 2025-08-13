using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Models
{
    public class LeaderboardUpdate : ClientMessage
    {
        [JsonPropertyName("ActionType")]
        public string ActionType { get; set; } // "LeaderboardAdd", "LeaderboardUpdate", "LeaderboardRemove"

        [JsonPropertyName("Entry")]
        public LeaderboardEntry Entry { get; set; }
    }
}
