using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeNet.Packets.Multisport;

namespace TimeNetSync
{
    internal class Result
    {
        public string Name { get; set; }
        /// <summary>
        /// Represents the time recorded, in seconds
        /// </summary>
        public double Time { get; set; }
        public int Bib { get; set; }
        public int Rank { get; set; }

        public Result(ResultlistCompetitor competitor)
        {
            Name = competitor.Name;
            Bib = Int32.Parse(competitor.Bib);
            Rank = Int32.Parse(competitor.Rank);
            Time = ParseTimeString(competitor.Time);
        }

        private double ParseTimeString(string time)
        {
            TimeSpan result;
            if (TimeSpan.TryParseExact(time, @"m\:ss\.ff", null, out result))
                return result.TotalSeconds;
            else if (TimeSpan.TryParseExact(time, @"s\.ff", null, out result))
                return result.TotalSeconds;
            else
                return 0;
        }
    }
}
