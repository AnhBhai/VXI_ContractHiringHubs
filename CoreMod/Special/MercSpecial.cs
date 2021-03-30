using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using Helpers;

namespace VXIContractHiringHubs
{
    static class MercSpecial
    {
        public static bool isReformation = false;
        public static void ReformationAct(SimGameState simGame)
        {
            // Set ComStar Reformation as Employer on Clan Worlds
            Dictionary<FactionValue, List<StarSystem>> systemsByFaction = MercGuild.GetExistingSystemsByFaction(simGame);

            foreach (KeyValuePair<FactionValue, List<StarSystem>> keyValuePair in systemsByFaction)
            {
                if (keyValuePair.Key.IsClan)
                {
                    foreach (StarSystem starSystems in keyValuePair.Value)
                    {
                        if (!starSystems.Def.ContractEmployerIDList.Contains("ComStarRef"))
                            starSystems.Def.ContractEmployerIDList.Add("ComStarRef");
                    }
                }
            }

            if (simGame.StarSystems.Exists(x => x.Name.Contains("Tukayyid")))
            {
                StarSystem theSystem = simGame.StarSystems.Find(x => x.Name.Contains("Tukayyid"));

                if (!theSystem.Def.ContractEmployerIDList.Contains("ComStarRef"))
                    theSystem.Def.ContractEmployerIDList.Add("ComStarRef");
            }
            isReformation = true;
        }
    }
}
