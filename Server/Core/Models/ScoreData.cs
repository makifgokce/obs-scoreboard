using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreboardServer.Core.Models
{
    public class ScoreData
    {
        public string TeamAName { get; set; }
        public int TeamAScore { get; set; } = 0;
        public string TeamAColor { get; set; } = "#2196F3";
        public string TeamBName { get; set; }
        public int TeamBScore { get; set; } = 0;
        public string TeamBColor { get; set; } = "#FF5722";
        public string CounterName { get; set; } = "Sayaç";
        public int Counter { get; set; } = 0;
        public List<LeaderboardEntry> Leaderboard { get; set; } = new List<LeaderboardEntry>();

    }
}
