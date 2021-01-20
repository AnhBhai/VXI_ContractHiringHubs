using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;

namespace VXIContractHiringHubs
{
    class TrainingSystem
    {
        [HarmonyPatch(typeof(StarSystem), "GeneratePilots")]
        public static class StarSystem_GeneratePilots_Patch
        {
            public static void Prefix(StarSystem __instance, ref int count)
            {
                try
                {
                    if (__instance.Tags.Contains("planet_other_hiringhub"))
                    {
                        count += 6;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }
    }
}
