using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Models
{
    public class ScoreUpdate : ClientMessage
    {
        [JsonPropertyName("Team")]
        public string Team { get; set; }
        [JsonPropertyName("Color")]
        public string Color { get; set; }

        [JsonPropertyName("Score")]
        public int Score { get; set; }
    }
}
