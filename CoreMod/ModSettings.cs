using System.Collections.Generic;
using BattleTech;

namespace VXIContractHiringHubs
{
    public class ModSettings
    {
        public bool Debug = false;
        public string modDirectory;

        public int LengthDeploymentDays = 30;
        public int DeploymentContractRefresh = 5;
        public int ActiveDeploymentContracts = 5;
        public int DeploymentRepBonus = 10;
        public int DeploymentContractBonus = 100000;
        public int DeploymentAllyPct = 25;
        public int DeploymentSpecialPct = 10;

        public int HonoredMax = 6;
        public int FriendlyMax = 5;
        public int LikedMax = 5;
        public int IndifferentMax = 5;
        public int DislikedMax = 4;
        public int LoathedMax = 2;
        public int HatedMax = 3;

        public Dictionary<string, List<int>> DeploymentChoiceMax = new Dictionary<string, List<int>>();

        public int MercGuildContractRefresh = 10;
        public int ReduceDeployChanceDays = 30; // After Beta: 365 days (1 year)

        public int MercFactionPilotPct = 100;
        public int MajorFactionPilotPct = 50;
        public int MinorFactionPilotPct = 30;
        public int RegionalFactionPilotPct = 15;

        public int MercDeploymentsPct = 50;
        public int MajorDeploymentsPct = 40;
        public int MinorDeploymentsPct = 25;
        public int RegionalDeploymentsPct = 10;

        public int MercDeploymentsMax = 3;
        public int MajorDeploymentsMax = 2;
        public int MinorDeploymentsMax = 1;
        public int RegionalDeploymentsMax = 1;

        public int MercContracts = 8;
        public int MajorContracts = 4;
        public int MinorContracts = 2;
        public int RegionalContracts = 1;

        public List<string> MercDeployCategory = new List<string>();
        public Dictionary<string, int> MercDeploySubType = new Dictionary<string, int>();
        public Dictionary<string, int> MercDeployEthic = new Dictionary<string, int>();
        
        public Dictionary<string, string> MercenaryGuilds = new Dictionary<string, string>();
        public Dictionary<string, string> MajorFactionCapitals = new Dictionary<string, string>();
        public Dictionary<string, string> MinorFactionCapitals = new Dictionary<string, string>();
        public Dictionary<string, string> RegionalFactions = new Dictionary<string, string>();

        public Dictionary<string, List<string>> AlliedFactions = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> EnemyFactions = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> SpecialFactions = new Dictionary<string, List<string>>();
        
                    
    }
}
