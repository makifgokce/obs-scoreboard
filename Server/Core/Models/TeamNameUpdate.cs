using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Models
{
    public class TeamNameUpdate : ClientMessage
    {
        [JsonPropertyName("Team")]
        public string Team { get; set; } = "";
        [JsonPropertyName("Name")]
        public string Name { get; set; } = "";
    }
}
