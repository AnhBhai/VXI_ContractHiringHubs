using System.Collections.Generic;
using BattleTech;

namespace VXIContractManagement
{
    public class ModSettings
    {
        public bool Debug = false;
        public string modDirectory;

        public int MercGuildContractRefresh = 10;

        public int MercFactionPilotPct = 100;
        public int MajorFactionPilotPct = 50;
        public int MinorFactionPilotPct = 30;
        public int RegionalFactionPilotPct = 15;
        public int MercContracts = 8;
        public int MajorContracts = 4;
        public int MinorContracts = 2;
        public int RegionalContracts = 1;

        public Dictionary<string, string> MercenaryGuilds = new Dictionary<string, string>();
        public Dictionary<string, string> MajorFactionCapitals = new Dictionary<string, string>();
        public Dictionary<string, string> MinorFactionCapitals = new Dictionary<string, string>();
        public Dictionary<string, string> RegionalFactions = new Dictionary<string, string>();
    }
}
