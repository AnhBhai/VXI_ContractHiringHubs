using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Harmony;
using BattleTech;
using BattleTech.UI;
using BattleTech.Framework;
using fastJSON;
using HBS;
using HBS.Logging;
using Localize;
using UnityEngine.Events;
using VXIContractManagement;
using Helpers;
using static Helpers.GlobalMethods;

namespace VXIContractHiringHubs
{
    public static class MercGuild
    {
        public static int ContractCounter = 0;
        
        public static Dictionary<FactionValue, List<StarSystem>> SystemsByFaction = new Dictionary<FactionValue, List<StarSystem>>();
        public static Dictionary<FactionValue, List<StarSystem>> SystemsByFactionUnused = new Dictionary<FactionValue, List<StarSystem>>();

        public static Dictionary<FactionValue, List<StarSystem>> GetExistingSystemsByFaction(SimGameState simGame, List<FactionValue> blackList = null)
        {
            Dictionary<FactionValue, List<StarSystem>> finalList = new Dictionary<FactionValue, List<StarSystem>>();

            List<FactionValue> unfilteredFactionList;
            if (blackList == null)
                unfilteredFactionList = FactionEnumeration.FactionList;
            else
                unfilteredFactionList = FactionEnumeration.FactionList.Where(x => !blackList.Contains(x)).ToList();

            unfilteredFactionList = unfilteredFactionList.Distinct<FactionValue>().ToList();

            foreach (FactionValue factionValue in unfilteredFactionList)
            {
                List<StarSystem> filteredStarSystems = simGame.StarSystems.FindAll(f => f.OwnerValue.Name == factionValue.Name);

                if (filteredStarSystems.Count > 0 && !finalList.ContainsKey(factionValue))
                {
                    if (factionValue.Name.Equals("Locals"))
                    {
                        if (finalList.ContainsKey(FactionEnumeration.GetAuriganPiratesFactionValue()))
                            finalList[FactionEnumeration.GetAuriganPiratesFactionValue()].AddRange(filteredStarSystems);
                        else
                            finalList.Add(FactionEnumeration.GetAuriganPiratesFactionValue(), filteredStarSystems);
                    }
                    else
                    {
                        finalList.Add(factionValue, filteredStarSystems);
                    }
                }
            }

            return finalList;
        }

        public static KeyValuePair<FactionValue, List<StarSystem>> RetrieveFactionSystems(SimGameState simGame, List<FactionValue> blackList, bool limitDuplicates = true)
        {
            if (limitDuplicates)
            {
                KeyValuePair<FactionValue, List<StarSystem>> targetFactionUnused = SystemsByFactionUnused.GetRandomElement(simGame.NetworkRandom);
                if (SystemsByFactionUnused.Keys.Count() <= 1)
                {
                    // Refresh Unused Systems (unlikely to be needed unless the contract settings are increased)
                    SystemsByFactionUnused = GetExistingSystemsByFaction(simGame, blackList);

                }
                else
                {
                    SystemsByFactionUnused.Remove(targetFactionUnused.Key);
                }

                return targetFactionUnused;
            }

            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = SystemsByFaction.GetRandomElement(simGame.NetworkRandom);
            return targetFaction;
        }

        // BUILD Salvage/Paymment defaults for different factions
        // INTEREST SimGameState.StartLanceConfiguration
        public static void UpdateMercGuild(SimGameState simGame)
        {
            try
            {
                DateTime currentDate = simGame.CurrentDate;
                
                int noGenMissions = 0;
                int totalMissions = 0;
                bool clearFirst = false;
                int deploymentMissions = 0;

                List<FactionValue> blackList = new List<FactionValue>();
                blackList.Add(GenerateContractFactions.GetFactionValueFromString("ComStar"));
                blackList.Add(GenerateContractFactions.GetFactionValueFromString("NoFaction"));

                SystemsByFaction = GetExistingSystemsByFaction(simGame, blackList);
                SystemsByFactionUnused = GetExistingSystemsByFaction(simGame, blackList);

                int lastDeploy = Main.Settings.ReduceDeployChanceDays - Math.Min(Main.Settings.ReduceDeployChanceDays, simGame.CurrentDate.Subtract(InfoClass.DeploymentInfo.DateDeploymentEnd).Days);

                Log.Info("Game Date: " + currentDate.ToString("yyyy-MM-dd") + ":: DateHubUpdate: " + InfoClass.MercGuildInfo.DateHubUpdate.ToString("yyyy-MM-dd") + "(" + InfoClass.MercGuildInfo.IsGenInitContracts + ")");
                if (currentDate > InfoClass.MercGuildInfo.DateHubUpdate.AddDays(Main.Settings.MercGuildContractRefresh) && InfoClass.MercGuildInfo.IsGenInitContracts)
                {
                    string currentSystem = simGame.CurSystem.Name;
                    string currentOwner = simGame.CurSystem.OwnerValue.Name;
                    string longDesc = $"We'll be behind enemy lines here, we'll get generous salvage on this contract and our employer will cover jump costs and {ContractHiringHubs.PercentageExpenses * 100}% of our operating costs during travel. ";

                    Log.Info("**** Running for " + currentSystem + " [ " + currentOwner + " ] ****");
                    if (ContainsKeyValue(Main.Settings.MercenaryGuilds, currentSystem, currentOwner))
                    {
                        for (int i = 0; totalMissions < Main.Settings.MercContracts && i < Main.Settings.MercContracts * 2; i++)
                        {
                            clearFirst = i == 0;
                            //thisOwner = Core.Settings.MajorFactionCapitals.GetRandomElement();
                            //StarSystem theSystem = simGame.StarSystems.Find(x => x.Name == thisOwner.Key);

                            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = RetrieveFactionSystems(simGame, blackList);
                            StarSystem targetSystem = targetFaction.Value.GetRandomElement<StarSystem>(simGame.NetworkRandom);

                            GenerateContract generateContract = new GenerateContract();
                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 1.0f;
                            generateContract.BuffSalvage = true;
                            generateContract.strictTargetReq = generateContract.strictOwnerReq = true;
                            generateContract.LongDescriptionStart = longDesc;
                            Log.Info("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");

                            if (simGame.NetworkRandom.Int(0, 100 + 1) + lastDeploy <= Main.Settings.MercDeploymentsPct && deploymentMissions < Main.Settings.MercDeploymentsMax)
                            {
                                deploymentMissions = MercDeployment.MercGuildDeployment(generateContract, simGame, targetFaction.Key, targetSystem, deploymentMissions);
                            }
                            
                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GenerateProceduralContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Main.Settings.MercContracts)
                            InfoClass.MercGuildInfo.DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    else if (ContainsKeyValue(Main.Settings.MajorFactionCapitals, currentSystem, currentOwner))
                    {
                        for (int i = 0; totalMissions < Main.Settings.MajorContracts && i < Main.Settings.MajorContracts * 2; i++)
                        {
                            clearFirst = i == 0;
                            string tmpDeployFaction = tmpDeployFaction = FactionEnumeration.ProceduralContractFactionList.Where(f => f.IsGreatHouse).ToList().ConvertAll<string>(x => x.Name).GetRandomElement();

                            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = RetrieveFactionSystems(simGame, blackList);
                            StarSystem targetSystem = targetFaction.Value.GetRandomElement<StarSystem>(simGame.NetworkRandom);

                            GenerateContract generateContract = new GenerateContract();
                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 1.0f;
                            generateContract.LongDescriptionStart = longDesc;
                            Log.Info("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");

                            if (simGame.NetworkRandom.Int(0, 100 + 1) + lastDeploy <= Main.Settings.MajorDeploymentsPct && deploymentMissions < Main.Settings.MajorDeploymentsMax)
                            {
                                deploymentMissions = MercDeployment.MercGuildDeployment(generateContract, simGame, targetFaction.Key, targetSystem, deploymentMissions);
                            }

                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GenerateProceduralContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Main.Settings.MajorContracts)
                            InfoClass.MercGuildInfo.DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    else if (ContainsKeyValue(Main.Settings.MinorFactionCapitals, currentSystem, currentOwner))
                    {
                        for (int i = 0; totalMissions < Main.Settings.MinorContracts && i < Main.Settings.MinorContracts * 2; i++)
                        {
                            clearFirst = i == 0;
                            string tmpDeployFaction = tmpDeployFaction = FactionEnumeration.ProceduralContractFactionList.Where(f => f.IsGreatHouse).ToList().ConvertAll<string>(x => x.Name).GetRandomElement();

                            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = RetrieveFactionSystems(simGame, blackList);
                            StarSystem targetSystem = targetFaction.Value.GetRandomElement<StarSystem>(simGame.NetworkRandom);

                            GenerateContract generateContract = new GenerateContract();
                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 1.0f;
                            generateContract.LongDescriptionStart = longDesc;
                            Log.Info("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");

                            if (simGame.NetworkRandom.Int(0, 100 + 1) + lastDeploy <= Main.Settings.MinorDeploymentsPct && deploymentMissions < Main.Settings.MinorDeploymentsMax)
                            {
                                deploymentMissions = MercDeployment.MercGuildDeployment(generateContract, simGame, targetFaction.Key, targetSystem, deploymentMissions);
                            }

                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GenerateProceduralContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Main.Settings.MajorContracts)
                            InfoClass.MercGuildInfo.DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    else if (ContainsKeyValue(Main.Settings.RegionalFactions, currentSystem, currentOwner))
                    {
                        for (int i = 0; totalMissions < Main.Settings.RegionalContracts && i < Main.Settings.RegionalContracts * 2; i++)
                        {
                            clearFirst = i == 0;
                            string tmpDeployFaction = tmpDeployFaction = FactionEnumeration.ProceduralContractFactionList.Where(f => f.IsGreatHouse).ToList().ConvertAll<string>(x => x.Name).GetRandomElement();

                            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = RetrieveFactionSystems(simGame, blackList);
                            StarSystem targetSystem = targetFaction.Value.GetRandomElement<StarSystem>(simGame.NetworkRandom);

                            GenerateContract generateContract = new GenerateContract();
                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 1.0f;
                            generateContract.LongDescriptionStart = longDesc;
                            Log.Info("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");

                            if (simGame.NetworkRandom.Int(0, 100 + 1) + lastDeploy <= Main.Settings.RegionalDeploymentsPct && deploymentMissions < Main.Settings.RegionalDeploymentsMax)
                            {
                                deploymentMissions = MercDeployment.MercGuildDeployment(generateContract, simGame, targetFaction.Key, targetSystem, deploymentMissions);
                            }

                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GenerateProceduralContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Main.Settings.MajorContracts)
                            InfoClass.MercGuildInfo.DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    SystemsByFactionUnused = SystemsByFaction = new Dictionary<FactionValue, List<StarSystem>>();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        // SetNegotiatedValues (overwrite during Merc Guild Contract)
        [HarmonyPatch(typeof(Contract), "SetNegotiatedValues")]
        public static class Contract_SetNegotiatedValues_Patch
        {
            public static void Prefix(Contract __instance, ref float cbill, ref float salvage)
            {
                string seedGUID = __instance.Override.travelSeed + __instance.encounterObjectGuid;
                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGUID))
                {
                    cbill = 0.5f;
                    salvage = 0.5f;
                }
            }
            public static void Postfix(ref Contract __instance)
            {
                string seedGUID = __instance.Override.travelSeed + __instance.encounterObjectGuid;
                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGUID))
                {
                    Traverse.Create(__instance).Property("PercentageContractValue").SetValue(NonGlobalTravelContracts.GuidContralDetails[seedGUID].Salary);
                    Traverse.Create(__instance).Property("PercentageContractSalvage").SetValue(NonGlobalTravelContracts.GuidContralDetails[seedGUID].Salvage);
                }
            }
        }

            //    [HarmonyPatch(typeof(SimGameState), "OnBreadcrumbArrival")]
            //public static class SimGameState_OnBreadcrumbArrival_Patch
            //{
            //    public static void Postfix(SimGameState __instance)
            //    {
            //        try
            //        {
            //            Contract travelContract = __instance.ActiveTravelContract;
            //            if (travelContract != null)
            //            {
            //                foreach (SimGameEventResult simGameResultAction in travelContract.Override.OnContractSuccessResults)
            //                {
            //                    foreach (SimGameResultAction action in simGameResultAction.Actions)
            //                    {
            //                        if (action.Type == SimGameResultAction.ActionType.System_StartNonProceduralContract && action.additionalValues == null)
            //                        {
            //                            if (MercGuildInfo.Map != "")
            //                            {
            //                                MercGuildInfo.GetMercGuildActions(out action.value, out action.additionalValues);
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //        catch (Exception e)
            //        {
            //            Log.Error(e);
            //        }
            //    }
            //}

            
        //[HarmonyPatch(typeof(SimGameState), "Dehydrate")]
        //public static class SimGameState_Dehydrate_Patch
        //{
        //    public static void Prefix(SimGameState __instance, SerializableReferenceContainer references)
        //    {
        //        try
        //        {
        //            Contract travelContract = Traverse.Create(__instance).Field("activeBreadcrumb").GetValue<Contract>();
        //            if (travelContract != null)
        //                MercGuildInfo.UpdateMercGuildContract(travelContract);

        //            MercGuildInfo.DateHubUpdate = UpdateContracts.DateHubUpdate;
        //            references.AddItem<SaveMercGuildInfo>("MercGuildInfo", MercGuildInfo);

        //            //references.AddItem<Contract>("activeBreadcrumb", this.activeBreadcrumb);
        //            //if (simGame.ActiveTravelContract != null)
        //            //{
        //            //    Contract travelContract = simGame.ActiveTravelContract;
        //            //    if (!travelContract.CanNegotiate)
        //            //    {
        //            //        foreach (SimGameEventResult result in travelContract.Override.OnContractSuccessResults)
        //            //        {
        //            //            foreach (SimGameResultAction action in result.Actions)
        //            //            {
        //            //                SimGameResultAction tempAction = new SimGameResultAction(); 
        //            //                if (action.Type == SimGameResultAction.ActionType.System_StartNonProceduralContract)
        //            //                {                                        
        //            //                    tempAction.Type = SimGameResultAction.ActionType.System_StartNonProceduralContract;
        //            //                    foreach (string value in action.additionalValues)
        //            //                    {
        //            //                        MercGuildInfo.Action.additionalValues.Add(value);
        //            //                    }
        //            //                }
        //            //            }
        //            //        }
        //            //    }
        //            //}
        //            //MercGuildInfo.DateHubUpdate = UpdateContracts.DateHubUpdate;
        //            //globalReferences.AddItem<SaveMercGuildInfo>("MercGuildInfo", MercGuildInfo);

        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e);
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(SimGameState), "Rehydrate")]
        //public static class SimGameState_Rehydrate_Patch
        //{
        //    public static void Postfix(GameInstanceSave gameInstanceSave)
        //    {
        //        try
        //        {
        //            SerializableReferenceContainer globalReferences = gameInstanceSave.GlobalReferences;
        //            globalReferences.ResetOperateOnAllForAll();

        //            if (globalReferences.HasItem("MercGuildInfo"))
        //            {
        //                MercGuildInfo = globalReferences.GetItem<SaveMercGuildInfo>("MercGuildInfo");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e);
        //        }
        //    }
        //}


        [HarmonyPatch(typeof(SimGameState), "AddPredefinedContract2")]
        public static class SimGameState_AddPredefinedContract2_Patch
        {
            public static void Postfix(SimGameState __instance, ref Contract __result)
            {
                if (__instance.HasTravelContract)
                {

                    string seedGuid = __instance.ActiveTravelContract.Override.travelSeed + __result.encounterObjectGuid;
                    if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
                    {
                        __result.Override.travelSeed = __instance.ActiveTravelContract.Override.travelSeed;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(SimGameState), "ParseNonProceduralContractActionData")]
        public static class SimGameState_ParseNonProceduralContractActionData_Patch
        {
            public static void Prefix(ref string actionValue, ref string[] additionalValues, SimGameState __instance)
            {
                if (__instance.HasTravelContract)
                {
                    string seedGuid = __instance.ActiveTravelContract.Override.travelSeed + __instance.ActiveTravelContract.encounterObjectGuid;
                    if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid) && additionalValues == null)
                    {
                        NonGlobalTravelContracts.GetMercGuildActions(seedGuid, ref actionValue, ref additionalValues); ;
                    }
                }
            }
        }


        //[HarmonyPatch(typeof(SimGameState), "AddContract")]
        //public static class SimGameState_AddContract_Patch
        //{
        //    public static bool Prefix(SimGameState.AddContractData contractData, SimGameState __instance, ref Contract __result)
        //    {
        //        if (__instance.CurSystem.ID != contractData.TargetSystem)
        //        {
        //            Log.Info("Refreshing Contract from Save: " + contractData.ContractName);

        //            GenerateContract generateContract = new GenerateContract(contractData);
        //            generateContract.SalaryPct = 0.5f;
        //            generateContract.SalvagePct = 1.0f;
        //            generateContract.BuffSalvage = true;
        //            //generateContract.strictTargetReq = generateContract.strictOwnerReq = true;
        //            generateContract.LongDescriptionStart = "We'll be behind enemy lines here, salvage is completely ours, until the local reinforcements turn up, that is. ";

        //            generateContract.IsGlobal = false;
        //            generateContract.IsNegotiable = false;

        //            StarSystem theSystem = __instance.StarSystems.Find(x => x.Name == contractData.TargetSystem);

        //            ContractOverride contractOverride = __instance.DataManager.ContractOverrides.Get(contractData.ContractName).Copy();
        //            ContractTypeValue contractTypeValue = contractOverride.ContractTypeValue;
        //            List<MapAndEncounters> releasedMapsAndEncountersByContractTypeAndOwnership = MetadataDatabase.Instance.GetReleasedMapsAndEncountersByContractTypeAndOwnership(contractTypeValue.ID, false);
        //            MapAndEncounters mapAndEncounters = releasedMapsAndEncountersByContractTypeAndOwnership[0];

        //            Log.Info("Load From Save: EncounterGUID: " + contractData.EncounterGuid);
        //            if (contractData.EncounterGuid == "")
        //            {
        //                List<EncounterLayer_MDD> list = new List<EncounterLayer_MDD>(); 
        //                foreach (EncounterLayer_MDD encounterLayer_MDD in mapAndEncounters.Encounters)
        //                {
        //                    if (encounterLayer_MDD.ContractTypeRow.ContractTypeID == (long)contractTypeValue.ID)
        //                    {
        //                        list.Add(encounterLayer_MDD);
        //                    }
        //                }
        //                if (list.Count <= 0)
        //                {
        //                    throw new Exception("Map does not contain any encounters of type: " + contractTypeValue.Name);
        //                }
        //                contractData.EncounterGuid = list[__instance.NetworkRandom.Int(0, list.Count)].EncounterLayerGUID;
        //            }

        //            GameContext gameContext = new GameContext(__instance.Context);
        //            gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, theSystem);

        //            __result = generateContract.CreateCustomTravelContract(__instance, contractData, contractOverride, gameContext, mapAndEncounters.Map.BiomeSkinEntry);
                    
        //            return false;
        //        }
        //        return true;
        //    }
        //}


        //[HarmonyPatch(typeof(SimGameState), "StartBreadcrumb")]
        //public static class SimGameState_StartBreadcrumb_Patch
        //{
        //    public static void Prefix(SimGameState __instance, ref int cost)
        //    {
        //        try
        //        {
        //            // PrioritySalvageModifier

        //            MercGuildInfo.IsGenInitContracts = IsGenInitContracts = false;

        //            SimGameState simGame = __instance;
        //            if (simGame.potentialTravelContract != null)
        //            {
        //                string seedGUID = simGame.potentialTravelContract.Override.travelSeed + simGame.potentialTravelContract.encounterObjectGuid;
        //                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGUID))
        //                {
        //                    //SimGameConstants constants = simGame.Constants;
        //                    simGame.Constants.Salvage.PrioritySalvageModifier = 0.75f;
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e);
        //        }
        //    }
        //}
    }
}
