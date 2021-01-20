using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Harmony;
using HBS.Collections;
using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using BattleTech.Framework;
using BattleTech.Data;
using BattleTech.DataObjects;
using UnityEngine;
using BattleTech.Portraits;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.Save.Test;
using BattleTech.Serialization;
using BattleTech.StringInterpolation;
using BattleTech.UI.Tooltips;
using fastJSON;
using HBS;
using HBS.Logging;
using Localize;
using UnityEngine.Events;
using VXIContractManagement;
using DG.Tweening;

namespace VXIContractHiringHubs
{
    public static class MercGuild
    {
        public static DateTime DateHubUpdate = new DateTime(1978, 3, 2);
        public static SaveMercGuildInfo MercGuildInfo = new SaveMercGuildInfo();
        public static double PercentageExpenses = 0.25;
        public static int ContractCounter = 0;
        public static bool IsMercGuildWorld = false;
        public static bool IsGenInitContracts = false;
        public static Dictionary<FactionValue, List<StarSystem>> SystemsByFaction = new Dictionary<FactionValue, List<StarSystem>>();
        public static Dictionary<FactionValue, List<StarSystem>> SystemsByFactionUnused = new Dictionary<FactionValue, List<StarSystem>>();

        private static bool RetrieveMercGuilds(SimGameState simGame)
        {
            //Core.Settings.factionCapitals.ContainsKey(planetChange.SystemName)
            bool bFactionExists = false;
            bool bSystemExists = false;

            Logger.Log("Merc Guilds");
            foreach (KeyValuePair<string, string> eachContractSystem in Core.Settings.MercenaryGuilds)
            {
                if (simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key))
                {
                    StarSystem theSystem = simGame.StarSystems.Find(x => x.Name == eachContractSystem.Key);
                    bFactionExists = (theSystem.OwnerValue.Name == eachContractSystem.Value);
                    bSystemExists = simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key);
                }
                else
                {
                    bFactionExists = false;
                    bSystemExists = false;
                }

                Logger.Log("Faction Capital: " + eachContractSystem.Key + "[ " + bSystemExists.ToString() + " ]");
                Logger.Log("Faction Name: " + eachContractSystem.Value + "[ " + bFactionExists.ToString() + " ]");
            }

            return true; // Returns faction to add to System 
        }

        private static bool RetrieveMajorCapitals(SimGameState simGame)
        {
            //Core.Settings.factionCapitals.ContainsKey(planetChange.SystemName)
            bool bFactionExists = false;
            bool bSystemExists = false;

            Logger.Log("Major Capitals");
            foreach (KeyValuePair<string, string> eachContractSystem in Core.Settings.MajorFactionCapitals)
            {
                if (simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key))
                {
                    StarSystem theSystem = simGame.StarSystems.Find(x => x.Name == eachContractSystem.Key);
                    bFactionExists = (theSystem.OwnerValue.Name == eachContractSystem.Value);
                    bSystemExists = simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key);
                }
                else
                {
                    bFactionExists = false;
                    bSystemExists = false;
                }

                Logger.Log("Faction Capital: " + eachContractSystem.Key + "[ " + bSystemExists.ToString() + " ]");
                Logger.Log("Faction Name: " + eachContractSystem.Value + "[ " + bFactionExists.ToString() + " ]");
            }

            return true; // Returns faction to add to System 
        }

        private static bool RetrieveMinorCapitals(SimGameState simGame)
        {
            //Core.Settings.factionCapitals.ContainsKey(planetChange.SystemName)
            bool bFactionExists = false;
            bool bSystemExists = false;

            Logger.Log("Minor Capitals");
            foreach (KeyValuePair<string, string> eachContractSystem in Core.Settings.MinorFactionCapitals)
            {
                if (simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key))
                {
                    StarSystem theSystem = simGame.StarSystems.Find(x => x.Name == eachContractSystem.Key);
                    bFactionExists = (theSystem.OwnerValue.Name == eachContractSystem.Value);
                    bSystemExists = simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key);
                }
                else
                {
                    bFactionExists = false;
                    bSystemExists = false;
                }

                Logger.Log("Faction Capital: " + eachContractSystem.Key + "[ " + bSystemExists.ToString() + " ]");
                Logger.Log("Faction Name: " + eachContractSystem.Value + "[ " + bFactionExists.ToString() + " ]");
            }

            return true; // Returns faction to add to System 
        }

        private static bool RetrieveRegionalCapitals(SimGameState simGame)
        {
            //Core.Settings.factionCapitals.ContainsKey(planetChange.SystemName)
            bool bFactionExists = false;
            bool bSystemExists = false;

            Logger.Log("Regional Capitals");
            foreach (KeyValuePair<string, string> eachContractSystem in Core.Settings.RegionalFactions)
            {
                if (simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key))
                {
                    StarSystem theSystem = simGame.StarSystems.Find(x => x.Name == eachContractSystem.Key);
                    bFactionExists = (theSystem.OwnerValue.Name == eachContractSystem.Value);
                    bSystemExists = simGame.StarSystems.Exists(x => x.Name == eachContractSystem.Key);
                }
                else
                {
                    bFactionExists = false;
                    bSystemExists = false;
                }

                Logger.Log("Faction Capital: " + eachContractSystem.Key + "[ " + bSystemExists.ToString() + " ]");
                Logger.Log("Faction Name: " + eachContractSystem.Value + "[ " + bFactionExists.ToString() + " ]");
            }

            return true; // Returns faction to add to System 
        }

        public static bool ContainsKeyValue(Dictionary<string, string> dictionary,
                             string expectedKey, string expectedValue)
        {
            string actualValue;
            if (!dictionary.TryGetValue(expectedKey, out actualValue))
            {
                return false;
            }
            return actualValue == expectedValue;
        }

        public static Dictionary<FactionValue, List<StarSystem>> GetExistingSystemsByFaction(SimGameState simGame, List<StarSystem> blackList = null)
        {
            Dictionary<FactionValue, List<StarSystem>> finalList = new Dictionary<FactionValue, List<StarSystem>>();

            List<StarSystem> systemValues = simGame.StarSystems.FindAll(f => f.OwnerValue.IsRealFaction);

            List<FactionValue> unfilteredFactionList = FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsRealFaction);

            foreach (FactionValue factionValue in unfilteredFactionList)
            {
                List<StarSystem> filteredStarSystems = new List<StarSystem>();
                foreach (StarSystem starSystem in systemValues)
                {
                    if (starSystem.OwnerValue == factionValue)
                    {
                        filteredStarSystems.Add(starSystem);
                    }
                }
                if (filteredStarSystems.Count > 0)
                {
                    finalList.Add(factionValue, filteredStarSystems);
                }
            }

            return finalList;
        }

        public static KeyValuePair<FactionValue, List<StarSystem>> RetrieveFactionSystems(SimGameState simGame, List<StarSystem> blackList = null, bool limitDuplicates = true)
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
        public static void UpdateTheContracts(SimGameState simGame)
        {
            try
            {
                DateTime currentDate = simGame.CurrentDate;
                
                int noGenMissions = 0;
                int totalMissions = 0;
                bool clearFirst = false;

                List<StarSystem> blackList = simGame.StarSystems.FindAll(x => x.OwnerValue.Name == "ComStar");

                SystemsByFaction = GetExistingSystemsByFaction(simGame, blackList);
                SystemsByFactionUnused = GetExistingSystemsByFaction(simGame, blackList);


                //Logger.Log("Retrieve Merc Guilds and Major, Minor and Regional Capitals");
                //bRetrieved = RetrieveMercGuilds(simGame);
                //bRetrieved = RetrieveMajorCapitals(simGame);
                //bRetrieved = RetrieveMinorCapitals(simGame);
                //bRetrieved = RetrieveRegionalCapitals(simGame);

                Logger.Log("Game Date: " + currentDate.ToString("yyyy-MM-dd") + ":: DateHubUpdate: " + DateHubUpdate.ToString("yyyy-MM-dd") + "(" + IsGenInitContracts + ")");
                if (currentDate > DateHubUpdate.AddDays(7) && IsGenInitContracts)
                {
                    string currentSystem = simGame.CurSystem.Name;
                    string currentOwner = simGame.CurSystem.OwnerValue.Name;
                    string longDesc = $"We'll be behind enemy lines here, we'll get generous salvage on this contract and our employer will cover jump costs and {PercentageExpenses * 100}% of our operating costs during travel. ";

                    Logger.Log("**** Running for " + currentSystem + " [ " + currentOwner + " ] ****");
                    if (ContainsKeyValue(Core.Settings.MercenaryGuilds, currentSystem, currentOwner))
                    {
                        MercGuildInfo.IsMercGuildWorld = IsMercGuildWorld = true;
                        for (int i = 0; totalMissions < Core.Settings.MercContracts && i < Core.Settings.MercContracts * 2; i++)
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
                            Logger.Log("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");
                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GeneratePotentialContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Core.Settings.MercContracts)
                            DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    else if (ContainsKeyValue(Core.Settings.MajorFactionCapitals, currentSystem, currentOwner))
                    {
                        MercGuildInfo.IsMercGuildWorld = IsMercGuildWorld = false;
                        for (int i = 0; totalMissions < Core.Settings.MajorContracts && i < Core.Settings.MajorContracts * 2; i++)
                        {
                            clearFirst = i == 0;

                            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = RetrieveFactionSystems(simGame, blackList);
                            StarSystem targetSystem = targetFaction.Value.GetRandomElement<StarSystem>(simGame.NetworkRandom);

                            GenerateContract generateContract = new GenerateContract();
                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 1.0f;
                            generateContract.LongDescriptionStart = longDesc;
                            Logger.Log("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");
                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GeneratePotentialContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Core.Settings.MajorContracts)
                            MercGuildInfo.DateHubUpdate = DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    else if (ContainsKeyValue(Core.Settings.MinorFactionCapitals, currentSystem, currentOwner))
                    {
                        MercGuildInfo.IsMercGuildWorld = IsMercGuildWorld = false;
                        for (int i = 0; totalMissions < Core.Settings.MinorContracts && i < Core.Settings.MinorContracts * 2; i++)
                        {
                            clearFirst = i == 0;

                            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = RetrieveFactionSystems(simGame, blackList);
                            StarSystem targetSystem = targetFaction.Value.GetRandomElement<StarSystem>(simGame.NetworkRandom);

                            GenerateContract generateContract = new GenerateContract();
                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 1.0f;
                            generateContract.LongDescriptionStart = longDesc;
                            Logger.Log("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");
                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GeneratePotentialContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Core.Settings.MajorContracts)
                            MercGuildInfo.DateHubUpdate = DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    else if (ContainsKeyValue(Core.Settings.RegionalFactions, currentSystem, currentOwner))
                    {
                        MercGuildInfo.IsMercGuildWorld = IsMercGuildWorld = false;
                        for (int i = 0; totalMissions < Core.Settings.RegionalContracts && i < Core.Settings.RegionalContracts * 2; i++)
                        {
                            clearFirst = i == 0;

                            KeyValuePair<FactionValue, List<StarSystem>> targetFaction = RetrieveFactionSystems(simGame, blackList);
                            StarSystem targetSystem = targetFaction.Value.GetRandomElement<StarSystem>(simGame.NetworkRandom);

                            GenerateContract generateContract = new GenerateContract();
                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 1.0f;
                            generateContract.LongDescriptionStart = longDesc;
                            Logger.Log("*** Create contract for " + targetSystem.Name + " [ " + targetFaction.Key.Name + " ] ***");
                            generateContract.ContractTarget = GenerateContractFactions.SetProceduralFactionID(targetFaction.Key);
                            GenerateContractFactions.setContractFactionsBasedOnSystems(generateContract, simGame, simGame.CurSystem, targetSystem);

                            noGenMissions = generateContract.GeneratePotentialContracts(simGame, targetSystem, clearFirst, false, false);
                            totalMissions = noGenMissions;
                        }
                        if (totalMissions >= Core.Settings.MajorContracts)
                            MercGuildInfo.DateHubUpdate = DateHubUpdate = currentDate;

                        totalMissions = 0;
                    }
                    MercGuildInfo.IsMercGuildWorld = IsMercGuildWorld = false;
                    SystemsByFactionUnused = GetExistingSystemsByFaction(simGame, blackList);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /* ActiveTravelContract
         * CreateBreakContractWarning - overwrite prefix
         * Dehydrate (save data without files)
         * GenerateSalvage (Priority Salvage)
         */

        // Reset all travel contracts and penalise costs
        [HarmonyPatch(typeof(SimGameState), "OnBreadcrumbCancelledByUser")]
        public static class SimGameState_OnBreadcrumbCancelledByUser_Patch
        {
            static void Prefix(ref SimGameState __instance)
            {
                try
                {
                    if (__instance.HasTravelContract)
                    {
                        string seedGuid = __instance.ActiveTravelContract.Override.travelSeed + __instance.ActiveTravelContract.encounterObjectGuid;
                        if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
                        {
                            __instance.AddFunds(-MercGuildInfo.EmployerCosts); // Removing costs covered by employer
                            MercGuildInfo.EmployerCosts = 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        // Reset all travel contracts
        [HarmonyPatch(typeof(SimGameState), "ResolveCompleteContract")]
        public static class SimGameState_ResolveCompleteContract_Patch
        {
            static void Prefix(ref SimGameState __instance)
            {
                try
                {
                    MercGuildInfo.EmployerCosts = 0;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        //[HarmonyPatch(typeof(SimGameState), "GetExpenditures", typeof(bool))] - EXAMPLE
        //[HarmonyAfter(new string[] { "com.github.mcb5637.BTSimpleMechAssembly" })]

        // StartBreadcrumb(option for setting monthly expenses)
        [HarmonyPatch(typeof(SimGameState), "DeductQuarterlyFunds")]
        public static class SimGameState_DeductQuarterlyFunds_Patch
        {
            static void Postfix(ref SimGameState __instance)
            {
                try
                {
                    if (__instance.HasTravelContract)
                    {
                        string seedGuid = __instance.ActiveTravelContract.Override.travelSeed + __instance.ActiveTravelContract.encounterObjectGuid;
                        if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
                        {
                            Logger.Log("(PRE)  Quarterly Funds Covered: " + __instance.Funds + "[" + MercGuildInfo.EmployerCosts + "]");

                            int expenditures = __instance.GetExpenditures(false);
                            int employerPayment = (int)(expenditures * PercentageExpenses + 1);

                            MercGuildInfo.EmployerCosts += employerPayment;
                            __instance.AddFunds(employerPayment);

                            Logger.Log("(POST) Quarterly Funds Covered: " + __instance.Funds + "[" + MercGuildInfo.EmployerCosts + "]");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }


        // SalvagePotential
        //[HarmonyPatch(typeof(SGContractsListItem), "Init")]
        //public static class SGContractsListItem_Init_Patch
        //{
        //    public static void Prefix(SGContractsListItem __instance, ref Contract contract, SimGameState sim)
        //    {
        //        try
        //        {
        //            /* EXISTING CALC
        //             * this.FinalSalvageCount = num7;
        //             * this.FinalPrioritySalvageCount = Math.Min(8, Mathf.FloorToInt((float)num7 * constants.Salvage.PrioritySalvageModifier));
        //             */

        //            if (contract.encounterObjectGuid != null && contract.Override != null)
        //            {
        //                string seedGuid = contract.Override.travelSeed + contract.encounterObjectGuid;
        //                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
        //                {
        //                    int num = contract.SalvagePotential;
        //                    int num2 = Mathf.FloorToInt((float)num * 0.75f);

        //                    //contract.SalvagePotential *= 3;
        //                    Traverse.Create(contract).Property("SalvagePotential").SetValue(contract.SalvagePotential * 3);

        //                    //__instance.setFieldText(this.contractMaxSalvage, string.Format("{0} / {1}", num2, num));
        //                    //LocalizableText tmpText = new LocalizableText();
        //                    //LocalizableText tmpText = (LocalizableText)Traverse.Create(__instance).Property("contractMaxSalvage").GetValue();
        //                    //tmpText.SetText("{0} / {1}", new object[] { num2, num });

        //                    //Traverse.Create(__instance).Property("contractMaxSalvage").SetValue(tmpText);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Error(e);
        //        }
        //    }
        //    public static void Postfix(SGContractsListItem __instance, ref Contract contract, SimGameState sim)
        //    {
        //        try
        //        {
        //            /* EXISTING CALC
        //             * this.FinalSalvageCount = num7;
        //             * this.FinalPrioritySalvageCount = Math.Min(8, Mathf.FloorToInt((float)num7 * constants.Salvage.PrioritySalvageModifier));
        //             */

        //            if (contract.encounterObjectGuid != null && contract.Override != null)
        //            {
        //                string seedGuid = contract.Override.travelSeed + contract.encounterObjectGuid;
        //                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
        //                {
        //                    int num = contract.SalvagePotential;
        //                    int num2 = Mathf.FloorToInt((float)num * 0.75f);

        //                    //contract.SalvagePotential *= 3;
        //                    Traverse.Create(contract).Property("SalvagePotential").SetValue(13);

        //                    //__instance.setFieldText(this.contractMaxSalvage, string.Format("{0} / {1}", num2, num));
        //                    //LocalizableText tmpText = new LocalizableText();
        //                    //LocalizableText tmpText = (LocalizableText)Traverse.Create(__instance).Property("contractMaxSalvage").GetValue();
        //                    //tmpText.SetText("{0} / {1}", new object[] { num2, num });

        //                    //Traverse.Create(__instance).Property("contractMaxSalvage").SetValue(tmpText);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Error(e);
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(Briefing), "SetContractInfo")]
        //public static class Briefing_SetContractInfo_Patch
        //{
        //    public static void Postfix(Briefing __instance, Contract contract, SimGameState Sim)
        //    {
        //        try
        //        {
        //            /* EXISTING CALC
        //             * this.FinalSalvageCount = num7;
        //             * this.FinalPrioritySalvageCount = Math.Min(8, Mathf.FloorToInt((float)num7 * constants.Salvage.PrioritySalvageModifier));
        //             */

        //            if (contract.encounterObjectGuid != null && contract.Override != null)
        //            {
        //                string seedGuid = contract.Override.travelSeed + contract.encounterObjectGuid;
        //                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
        //                {
        //                    int num = contract.SalvagePotential;
        //                    int num2 = Mathf.FloorToInt((float)num * 0.75f);

        //                    //__instance.contractSalvageField.SetText("{0} / {1}", new object[] { num2, num });
        //                    LocalizableText tmpText = (LocalizableText)Traverse.Create(__instance).Property("contractSalvageField").GetValue();
        //                    tmpText.SetText("{0} / {1}", new object[] { num2, num });

        //                    Traverse.Create(__instance).Property("contractSalvageField").SetValue(tmpText);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Error(e);
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(LanceContractDetailsWidget), "PopulateContract")]
        //public static class LanceContractDetailsWidget_PopulateContract_Patch
        //{
        //    public static void Postfix(LanceContractDetailsWidget __instance, Contract contract)
        //    {
        //        try
        //        {
        //            /* EXISTING CALC
        //             * this.FinalSalvageCount = num7;
        //             * this.FinalPrioritySalvageCount = Math.Min(8, Mathf.FloorToInt((float)num7 * constants.Salvage.PrioritySalvageModifier));
        //             */

        //            if (contract.encounterObjectGuid != null && contract.Override != null)
        //            {
        //                string seedGuid = contract.Override.travelSeed + contract.encounterObjectGuid;
        //                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
        //                {
        //                    int num = contract.SalvagePotential;
        //                    int num2 = Mathf.FloorToInt((float)num * 0.75f);

        //                    //__instance.MetaMaxSalvageField.SetText("{0} / {1}", new object[] { num2, num });
        //                    LocalizableText tmpText = new LocalizableText();
        //                    tmpText.SetText("{0} / {1}", new object[] { num2, num });

        //                    Traverse.Create(__instance).Property("MetaMaxSalvageField").SetValue(tmpText);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Error(e);
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(Contract), "GenerateSalvage")]
        //[HarmonyAfter(new string[] { "com.github.mcb5637.BTSimpleMechAssembly" })]
        //public static class Contract_GenerateSalvage_Patch
        //{
        //    public static void Postfix(ref Contract __instance)
        //    {
        //        try
        //        {
        //            /* EXISTING CALC
        //             * this.FinalSalvageCount = num7;
        //             * this.FinalPrioritySalvageCount = Math.Min(8, Mathf.FloorToInt((float)num7 * constants.Salvage.PrioritySalvageModifier));
        //             */

        //            if (__instance.encounterObjectGuid != null && __instance.Override != null)
        //            {
        //                string seedGuid = __instance.Override.travelSeed + __instance.encounterObjectGuid;
        //                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
        //                {
        //                    Traverse.Create(__instance).Property("FinalPrioritySalvageCount").SetValue(__instance.FinalSalvageCount * 0.50);
        //                }
        //            }

        //            //PropertyInfo property = typeof(int).GetProperty("FinalPrioritySalvageCount");
        //            //property.DeclaringType.GetProperty("FinalPrioritySalvageCount");
        //            //property.SetValue(__instance, __instance.FinalSalvageCount * 0.50, BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Error(e);
        //        }
        //    }
        //}

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
            //            Logger.Error(e);
            //        }
            //    }
            //}

            [HarmonyPatch(typeof(GameInstanceSave), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(GameInstance), typeof(SaveReason) })]
        public static class GameInstanceSave_Constructor_Patch
        {
            static void Postfix(GameInstanceSave __instance, GameInstance gameInstance, SaveReason saveReason)
            {
                try
                {
                    Helper.SaveState(__instance.InstanceGUID, __instance.SaveTime);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }

            }
        }

        [HarmonyPatch(typeof(GameInstance), "Load")]
        public static class GameInstance_Load_Patch
        {
            static void Prefix(GameInstanceSave save)
            {
                try
                {
                    Helper.LoadState(save.InstanceGUID, save.SaveTime);
                    DateHubUpdate = MercGuildInfo.DateHubUpdate;
                    IsGenInitContracts = MercGuildInfo.IsGenInitContracts;
                    IsMercGuildWorld = MercGuildInfo.IsMercGuildWorld;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(SGLoadSavedGameScreen), "DeleteSlotAction")]
        public static class SGLoadSavedGameScreen_DeleteSlotAction_Patch
        {
            static string tmpInstanceGUID = "";
            static DateTime tmpSaveTime = new DateTime();
            static void Prefix(SlotModel slot)
            {
                try
                {
                    //SET startCount = slot.count
                    tmpInstanceGUID = slot.InstanceGUID;
                    tmpSaveTime = slot.SaveTime;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            static void Postfix(SlotModel slot)
            {
                // IF startCount > slot.count
                Helper.DeleteState(tmpInstanceGUID, tmpSaveTime);
                Logger.Log($".DeleteState({tmpInstanceGUID}, {tmpSaveTime});");
            }
        }

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
        //            Logger.Error(e);
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
        //            Logger.Error(e);
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
        //            Logger.Log("Refreshing Contract from Save: " + contractData.ContractName);

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

        //            Logger.Log("Load From Save: EncounterGUID: " + contractData.EncounterGuid);
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

        [HarmonyPatch(typeof(StarSystem), "GenerateInitialContracts")]
        public static class StarSystem_GenerateInitialContracts_Patch
        {
            public static void Postfix(StarSystem __instance)
            {
                try
                {
                    MercGuildInfo.IsGenInitContracts = IsGenInitContracts = true;
                    SimGameState simGame = __instance.Sim;
                    DateHubUpdate = simGame.CurrentDate.AddDays(0 - Core.Settings.MercGuildContractRefresh);
                    MercGuildInfo.DateHubUpdate = DateHubUpdate;
                    UpdateHubs.UpdateTheHubs(simGame);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

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
        //            Logger.Error(e);
        //        }
        //    }
        //}

        [HarmonyPatch(typeof(StarSystem), "OnSystemExit")]
        public static class StarSystem_OnSystemExit_Patch
        {
            public static void Postfix(StarSystem __instance)
            {
                MercGuildInfo.IsGenInitContracts = IsGenInitContracts = false;
            }
        }

        // Change display type of procedural contracts with disabled negotiations
        [HarmonyPatch(typeof(SGContractsWidget), "AddContract")]
        public static class SGContractsWidget_AddContract_Patch
        {
            static bool isOpportunityMission = false;

            public static void Prefix(SGContractsWidget __instance, Contract contract)
            {
                try
                {
                    if (!contract.CanNegotiate)
                    {
                        //isOpportunityMission = true;
                        Logger.Log($"[SGContractsWidget_AddContract_PREFIX] {contract.Override.ID} is a non-negotiable Merc Guild mission");

                        // Temporarily set contractDisplayStyle
                        contract.Override.contractDisplayStyle = ContractDisplayStyle.BaseCampaignStory;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            public static void Postfix(SGContractsWidget __instance, Contract contract)
            {
                // Reset contractDisplayStyle
                if (isOpportunityMission)
                {
                    //contract.Override.contractDisplayStyle = ContractDisplayStyle.BaseCampaignNormal;
                }
            }
        }
    }
}
