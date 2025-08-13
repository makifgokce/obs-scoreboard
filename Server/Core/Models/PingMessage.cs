using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Models
{
    public class PingMessage : ClientMessage
    {
        [JsonPropertyName("Message")]
        public string Message { get; set; } = "Pong";
    }
}
