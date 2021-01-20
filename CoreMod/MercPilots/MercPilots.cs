using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using Helpers;

namespace VXIContractHiringHubs
{
    static class MercPilots
    {
        public static void UpdateMercPilots(SimGameState simGame)
        {
            InfoClass.MercPilotInfo.IsGenInitPilots = true;
        }

            [HarmonyPatch(typeof(StarSystem), "GeneratePilots")]
        public static class StarSystem_GeneratePilots_Patch
        {
            public static void Prefix(StarSystem __instance, ref int count)
            {
                try
                {
                    //if (__instance.Tags.Contains("planet_other_hiringhub"))
                    //{
                    //    count += 6;
                    //}
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
