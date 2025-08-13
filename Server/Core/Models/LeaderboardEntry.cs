using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Models
{
    public class LeaderboardEntry
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Color { get; set; } = "#2196F3";
        public int Score { get; set; } = 0;
    }
}
