using System.Linq;
using System.Collections.Generic;
using DemoInfo;

namespace csgo_analytics.DemoExtensions
{
    public static class DemoExtensions
    {
        public static ICollection<string> ReadPlayersName(this DemoParser DemoParser){
            return DemoParser.PlayingParticipants.Select(p => p.Name).ToList();;
        }
    }
}