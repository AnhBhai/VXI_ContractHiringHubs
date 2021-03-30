using System;
using System.Collections.Generic;
using Harmony;
using BattleTech;
using BattleTech.UI;
using VXIContractManagement;
using Helpers;
using static Helpers.ContractHiringHub_Save;
using static Helpers.GlobalMethods;
using BattleTech.Framework;
using UnityEngine;

namespace VXIContractHiringHubs
{
    public static class ContractHiringHubs
    {
        public static double PercentageExpenses = 0.25;
        public static DateTime MonthlyUpdate = new DateTime(3000, 3, 9);
        public static bool PauseAfterJump = false;
        //public static int BlockedTravelState = 0;
        public static bool PauseTimer = false;

        public static void UpdateTheHubs(SimGameState simGame)
        {
            try
            {
                if (InfoClass.DeploymentInfo.IsDeployment && InfoClass.DeploymentInfo.IsGenInitContracts)
                {
                    Log.Info("Update the deployment");
                    
                    MercDeployment.UpdateDeployment(simGame);
                }
                else if (InfoClass.MercGuildInfo.IsGenInitContracts)
                {
                    Log.Info("Update the contracts");
                    MercGuild.UpdateMercGuild(simGame);
                }

                if (InfoClass.MercPilotInfo.IsGenInitPilots)
                {
                    Log.Info("Update the Pilots"); // Update the Pilots
                    MercPilots.UpdateMercPilots(simGame);
                }

                if (simGame.CurrentDate >= MonthlyUpdate && simGame.CurrentDate.Year >= 3052 && simGame.CurrentDate.Month >= 6 && !MercSpecial.isReformation)
                {
                    Log.Info("Update the Reformation Act");
                    MercSpecial.ReformationAct(simGame);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        #region Test Functions
        [HarmonyPatch(typeof(SimGameState), "SetSimRoomState")]
        class SimGameState_SetSimRoomState_Patch
        {
            public static bool Prefix(SimGameState __instance, DropshipLocation state)
            {
                //if (state == DropshipLocation.CMD_CENTER && Input.GetKey(KeyCode.LeftShift))
                //{
                //    List<string> listString = new List<string>();
                //    listString.Add("ThreeWayBattle_TrainingDay");

                //    GenerateContract generateContract = new GenerateContract();
                //    generateContract.MaxContracts = __instance.CurSystem.SystemContracts.Count + 1;
                //    generateContract.MaxDifficulty = 10;
                //    generateContract.MinDifficulty = 1;
                //    GenerateContractFactions.setContractFactionsBasedOnRandom(generateContract, __instance, __instance.CurSystem);

                //    Dictionary<int, string> dictionary = new Dictionary<int, string>();

                //    dictionary.Add(0, "pilot_recruit_R1_01a");
                //    dictionary.Add(1, "pilot_recruit_R2_01a");
                //    dictionary.Add(2, "pilot_recruit_R3_01a");

                //    generateContract.PlayerTeamOverride.Add(0, dictionary);

                //    generateContract.BuildProceduralContracts(__instance, null, false, listString);
                //}

                //if (state == DropshipLocation.HIRING && Input.GetKey(KeyCode.LeftShift))
                //{
                //    MercPilots.UpdateMercPilots(__instance);
                //}

                //if (state == DropshipLocation.BARRACKS && Input.GetKey(KeyCode.LeftShift))
                //{
                //    List<FactionValue> factionValue = new List<FactionValue>();

                //    factionValue.AddRange(FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsClan));

                //    MercPilots.EndClanMission(__instance, factionValue.GetRandomElement());
                //}

                if (state == DropshipLocation.NAVIGATION && Input.GetKey(KeyCode.LeftAlt))
                {
                    PauseAfterJump = !PauseAfterJump;
                    PauseNotification.Show("Pause After Jump", $"Holding at Jump Point, after jump set to {PauseAfterJump}", __instance.GetCrewPortrait(SimGameCrew.Crew_Sumire), "", false);
                    return false;
                }

                return true;
            }
        }
        #endregion

        #region Reset at end of MercGuild or Deployment
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
                            __instance.AddFunds(-InfoClass.MercGuildInfo.EmployerCosts); // Removing costs covered by employer
                            InfoClass.MercGuildInfo.ClearInfo();
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        // Reset all travel contracts
        [HarmonyPatch(typeof(SimGameState), "ResolveCompleteContract")]
        public static class SimGameState_ResolveCompleteContract_Patch
        {
            public static void Prefix(ref SimGameState __instance)
            {
                try
                {
                    //if (InfoClass.DeploymentInfo.IsDeployment)
                    //{
                        
                    //}
                    InfoClass.MercGuildInfo.ClearInfo();

                    if (__instance.CompletedContract != null)
                    {
                        List<UnitResult> playerUnitResults = __instance.CompletedContract.PlayerUnitResults;
                        List<Pilot> list = new List<Pilot>(__instance.PilotRoster);
                        list.Add(__instance.Commander);
                        foreach (UnitResult unitResult in playerUnitResults)
                        {
                            foreach (Pilot pilot in list)
                            {
                                if (unitResult.pilot.pilotDef.Description.Id == pilot.pilotDef.Description.Id)
                                {
                                    MercPilots.LastUsedMechs(__instance, pilot, unitResult.mech.Name);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            public static void Postfix(ref SimGameState __instance)
            {
                try
                {
                    FactionValue targetValue = __instance.CompletedContract.Override.targetTeam.FactionValue;
                    FactionValue tgtAllyValue = __instance.CompletedContract.Override.targetsAllyTeam.FactionValue;
                    
                    if (targetValue.IsClan)
                    {
                        bool result = MercPilots.EndClanMission(__instance, targetValue);

                        if (result && tgtAllyValue.IsClan)
                            MercPilots.EndClanMission(__instance, tgtAllyValue);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        #endregion

        #region During MercGuild Travel or Deployment
        /* ActiveTravelContract - To Be Done
         * CreateBreakContractWarning - overwrite prefix
         * Dehydrate (save data without files)
         * GenerateSalvage (Priority Salvage)
         */

        //[HarmonyPatch(typeof(SimGameState), "GetExpenditures", typeof(bool))] - EXAMPLE
        //[HarmonyAfter(new string[] { "com.github.mcb5637.BTSimpleMechAssembly" })]
        // StartBreadcrumb(option for setting monthly expenses)
        [HarmonyPatch(typeof(SimGameState), "DeductQuarterlyFunds")]
        public static class SimGameState_DeductQuarterlyFunds_Patch
        {
            public static void Postfix(ref SimGameState __instance)
            {
                try
                {
                    if (__instance.HasTravelContract)
                    {
                        string seedGuid = __instance.ActiveTravelContract.Override.travelSeed + __instance.ActiveTravelContract.encounterObjectGuid;
                        if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
                        {
                            Log.Info("PRE-) MercGuild Funds Covered: " + __instance.Funds + "[" + InfoClass.MercGuildInfo.EmployerCosts + "]");

                            int expenditures = __instance.GetExpenditures(false);
                            int employerPayment = (int)(expenditures * PercentageExpenses + 1);

                            InfoClass.MercGuildInfo.EmployerCosts += employerPayment;
                            __instance.AddFunds(employerPayment);

                            Log.Info("(POST) MercGuild Funds Covered: " + __instance.Funds + "[" + InfoClass.MercGuildInfo.EmployerCosts + "]");
                        }
                    }
                    else if (InfoClass.DeploymentInfo.IsDeployment)
                    {
                        Log.Info("(PRE-) Deployment Funds Covered: " + __instance.Funds + "[" + InfoClass.MercGuildInfo.EmployerCosts + "]");

                        int expenditures = __instance.GetExpenditures(false);
                        int employerPayment = (int)(expenditures * PercentageExpenses + 1);

                        InfoClass.MercGuildInfo.EmployerCosts += employerPayment;
                        __instance.AddFunds(employerPayment);

                        Log.Info("(POST) Deployment Funds Covered: " + __instance.Funds + "[" + InfoClass.MercGuildInfo.EmployerCosts + "]");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        //[HarmonyPatch(typeof(SGRoomController_CmdCenter), "StartContractScreen")]
        [HarmonyPatch(typeof(SimGameState), "GeneratePotentialContracts")]
        public static class SimGameState_GeneratePotentialContracts_Patch
        {
            static bool Prefix(SimGameState __instance, Action onContractGenComplete)
            {
                try
                {
                    if (InfoClass.DeploymentInfo.IsDeployment || InfoClass.MercGuildInfo.IsDeployment)
                    {
                        int maxPerWave = Main.Settings.ActiveDeploymentContracts;
                        string factionID = InfoClass.DeploymentInfo.DeploymentFactionID;
                        if (Main.Settings.DeploymentChoiceMax.ContainsKey(__instance.GetReputation(GenerateContractFactions.GetFactionValueFromString(factionID)).ToString()))
                        {
                            maxPerWave = Main.Settings.DeploymentChoiceMax[__instance.GetReputation(GenerateContractFactions.GetFactionValueFromString(factionID)).ToString()][1];
                        }
                        if (InfoClass.DeploymentInfo.NoWaveContracts >= maxPerWave)
                        {
                            __instance.CurSystem.ResetContracts();
                            InfoClass.DeploymentInfo.NoWaveContracts = 0;
                        }

                        if (onContractGenComplete != null)
                        {
                            Traverse.Create(__instance.RoomManager.CmdCenterRoom).Property("holdForNewContract").SetValue(false);
                            SGContractsWidget contractsWidget = GetInstanceField(typeof(SGRoomController_CmdCenter), __instance.RoomManager.CmdCenterRoom, "contractsWidget") as SGContractsWidget;
                            contractsWidget.ListContracts(__instance.GetAllCurrentlySelectableContracts(true));
                        }
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(SGRoomController_CmdCenter), "StartContractScreen")]
        [HarmonyPatch(typeof(StarSystem), "GenerateInitialContracts")]
        public static class StarSystem_GenerateInitialContracts_Patch
        {
            static bool Prefix(StarSystem __instance)
            {
                try
                {
                    if (InfoClass.DeploymentInfo.IsDeployment)
                    {
                        InfoClass.DeploymentInfo.IsGenInitContracts = true;
                        ContractHiringHubs.UpdateTheHubs(__instance.Sim);
                        Traverse.Create(__instance.Sim.RoomManager.CmdCenterRoom).Property("holdForNewContract").SetValue(false);
                        SGContractsWidget contractsWidget = GetInstanceField(typeof(SGRoomController_CmdCenter), __instance.Sim.RoomManager.CmdCenterRoom, "contractsWidget") as SGContractsWidget;
                        contractsWidget.ListContracts(__instance.Sim.GetAllCurrentlySelectableContracts(true));
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                return true;
            }

            public static void Postfix(StarSystem __instance)
            {
                try
                {
                    InfoClass.MercGuildInfo.IsGenInitContracts = true;
                    InfoClass.DeploymentInfo.IsGenInitContracts = true;
                    InfoClass.MercGuildInfo.DateHubUpdate = __instance.Sim.CurrentDate.AddDays(-1 - Main.Settings.MercGuildContractRefresh);

                    ContractHiringHubs.UpdateTheHubs(__instance.Sim);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        //[HarmonyPatch(typeof(SimGameState), "FillMapEncounterContractData")]
        //public static class SimGameState_FillMapEncounterContractData_Patch
        //{ 
        //    public static void Postfix(StarSystem __instance)
        //    {
        //        try
        //        {
        //            InfoClass.MercGuildInfo.IsGenInitContracts = true;
        //            InfoClass.DeploymentInfo.IsGenInitContracts = true;
        //            InfoClass.MercGuildInfo.DateHubUpdate = __instance.Sim.CurrentDate.AddDays(0 - Main.Settings.MercGuildContractRefresh);

        //            //ContractHiringHubs.UpdateTheHubs(__instance.Sim);
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e);
        //        }
        //    }
        //}

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
                        //SimGameState simGame = GetInstanceField(typeof(SGContractsWidget), __instance, "Sim") as SimGameState;
                        string seedGUID = contract.Override.travelSeed + contract.encounterObjectGuid;
                        if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGUID))
                        {
                            //isOpportunityMission = true;
                            Log.Info($"[SGContractsWidget_AddContract_PREFIX] {contract.Override.ID} is a non-negotiable Merc Guild mission");

                            // Temporarily set contractDisplayStyle
                            contract.Override.contractDisplayStyle = ContractDisplayStyle.BaseCampaignStory;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
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

        [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
        public static class SimGameState_Rehydrate_Patch
        {
            static void Postfix(SimGameState __instance)
            {
                try
                {
                    if (InfoClass.DeploymentInfo.IsDeployment)
                    {
                        foreach (Contract contract in __instance.CurSystem.SystemContracts)
                        {
                            if (InfoClass.DeploymentInfo.DCInfo.ContainsKey(contract.Override.ID))
                            {
                                DeploymentContractInfo dcInfo = InfoClass.DeploymentInfo.DCInfo[contract.Override.ID];
                                contract.Override.shortDescription = dcInfo.EmployerShortDesc;
                                contract.Override.longDescription = dcInfo.DariusLongDesc;

                                if (!dcInfo.TargetOverride.Equals(""))
                                    contract.Override.targetTeam.faction = dcInfo.TargetOverride;
                                if (!dcInfo.EmployerOverride.Equals(""))
                                    contract.Override.employerTeam.faction = dcInfo.EmployerOverride;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        #endregion

        #region Event based triggers
        [HarmonyPatch(typeof(Contract), "GenerateSalvage")]
        //[HarmonyAfter(new string[] { "com.github.mcb5637.BTSimpleMechAssembly" })]
        public static class Contract_GenerateSalvage_Patch
        {
            public static void Prefix(Contract __instance, List<UnitResult> enemyMechs, List<VehicleDef> enemyVehicles, List<UnitResult> lostUnits)
            {
                try
                {
                    SimGameState simGame = __instance.BattleTechGame.Simulation;

                    Log.Info("Build stats post Battle and Launch any Deployment");
                    if (InfoClass.MercGuildInfo.IsDeployment && !InfoClass.DeploymentInfo.IsDeployment)
                    {
                        Log.Info("Launching Deployment");
                        //MercDeployDictionary.BuildDeployDictionaries();
                        MercDeployment.MercDeployment_Start(simGame, __instance.Override.employerTeam.FactionValue.Name);
                        NonGlobalTravelContracts.GuidContralDetails.Clear();
                    }
                    else if (InfoClass.DeploymentInfo.IsDeployment)
                    {
                        Log.Info("Building stats post Battle");
                        MercDeployment.RetrieveContractToUpdate(simGame, __instance.Override.ID, __instance.TheMissionResult);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(SimGameState), "CreateBreakContractWarning")]
        public static class SimGameState_CreateBreakContractWarning_Patch
        {
            public static void Prefix(SimGameState __instance)
            {
                try
                {
                    if (__instance.HasTravelContract)
                    {
                        string seedGuid = __instance.ActiveTravelContract.Override.travelSeed + __instance.ActiveTravelContract.encounterObjectGuid;
                        if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
                        {
                            __instance.ActiveTravelContract.Override.contractDisplayStyle = ContractDisplayStyle.BaseCampaignNormal;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
            public static void Postfix(SimGameState __instance)
            {
                try
                {
                    if (__instance.HasTravelContract)
                    {
                        string seedGuid = __instance.ActiveTravelContract.Override.travelSeed + __instance.ActiveTravelContract.encounterObjectGuid;
                        if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGuid))
                        {
                            __instance.ActiveTravelContract.Override.contractDisplayStyle = ContractDisplayStyle.BaseCampaignStory;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(SGNavigationScreen), "OnTravelCourseAccepted")]
        public static class SGNavigationScreen_OnTravelCourseAccepted_Patch
        {
            static bool Prefix(SGNavigationScreen __instance)
            {
                try
                {
                    if (InfoClass.DeploymentInfo.IsDeployment)
                    {
                        UIManager uiManager = (UIManager)AccessTools.Field(typeof(SGNavigationScreen), "uiManager").GetValue(__instance);
                        SimGameState simState = (SimGameState)AccessTools.Field(typeof(SGNavigationScreen), "simState").GetValue(__instance);
                        Action cleanup = delegate () {
                            uiManager.ResetFader(UIManagerRootType.PopupRoot);
                            simState.Starmap.Screen.AllowInput(true);
                        };
                        int daysRemaining = InfoClass.DeploymentInfo.DateDeploymentEnd.Subtract(simState.CurrentDate).Days;
                        string primaryButtonText = "Break Deployment";
                        string message = $"WARNING: This action will break your current deployment contract.{Environment.NewLine}" +
                            $"Your reputation* with {InfoClass.DeploymentInfo.DeploymentFactionID} (-{daysRemaining} rep) and the MRB (-{daysRemaining*2} rep) will take a hit.{Environment.NewLine}" +
                            $"Additionally all expenses covered by {InfoClass.DeploymentInfo.DeploymentFactionID} to the value of {InfoClass.DeploymentInfo.EmployerCosts} C-Bills plus your mission bonuses* will be patially forfeited.{Environment.NewLine}" +
                            $"{Environment.NewLine}(* Based on {daysRemaining} days left on the contract)";
                        PauseNotification.Show("Navigation Change", message, simState.GetCrewPortrait(SimGameCrew.Crew_Darius), string.Empty, true, delegate {
                            cleanup();
                            MercDeployment.MercDeployment_Cancel(simState, daysRemaining);
                            simState.Starmap.SetActivePath();
                            simState.SetSimRoomState(DropshipLocation.SHIP);
                        }, primaryButtonText, cleanup, "Cancel");
                        simState.Starmap.Screen.AllowInput(false);
                        uiManager.SetFaderColor(uiManager.UILookAndColorConstants.PopupBackfill, UIManagerFader.FadePosition.FadeInBack, UIManagerRootType.PopupRoot, true);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(StarSystem), "OnSystemExit")]
        public static class StarSystem_OnSystemExit_Patch
        {
            public static void Prefix(StarSystem __instance)
            {
                SimGameState simGame = __instance.Sim;

                //Contract travelContract = Traverse.Create(simGame).Field("activeBreadcrumb").GetValue<Contract>();

                if (simGame.HasTravelContract)
                {
                    Contract travelContract = simGame.ActiveTravelContract;
                    string seedGUID = travelContract.Override.travelSeed + travelContract.encounterObjectGuid;
                    if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGUID))
                    {
                        SimGameResultAction action = new SimGameResultAction();
                        NonGlobalTravelContracts.GetMercGuildActions(seedGUID, ref action.value, ref action.additionalValues);
                        float tmpSalary = NonGlobalTravelContracts.GuidContralDetails[seedGUID].Salary;
                        float tmpSalvage = NonGlobalTravelContracts.GuidContralDetails[seedGUID].Salvage;
                        bool isDeployment = NonGlobalTravelContracts.GuidContralDetails[seedGUID].IsDeployment;

                        NonGlobalTravelContracts.GuidContralDetails.Clear();
                        NonGlobalTravelContracts.AddTravelContract(seedGUID, action, tmpSalary, tmpSalvage, isDeployment); // Created for MercDeployments);
                        InfoClass.MercGuildInfo.IsDeployment = isDeployment;
                    }
                }
                else
                {
                    NonGlobalTravelContracts.GuidContralDetails.Clear();
                }
            }

            public static void Postfix(StarSystem __instance)
            {
                InfoClass.MercGuildInfo.IsGenInitContracts = false;
                InfoClass.MercPilotInfo.IsGenInitPilots = false;
                InfoClass.DeploymentInfo.IsGenInitContracts = false;
            }
        }

        [HarmonyPatch(typeof(StarSystem), "OnSystemChange")]
        public static class StarSystem_OnSystemChange_Patch
        {
            public static void Prefix(StarSystem __instance)
            {
                if (PauseAfterJump)
                {
                    __instance.Sim.SetTimeMoving(false, true);
                    __instance.Sim.PauseTimer();

                    PauseTimer = true;
                    Log.Info($"Pausing after jump");
                }
            }
        }

        [HarmonyPatch(typeof(SGTimePlayPause), "ToggleTime")]
        public static class SGTimePlayPause_ToggleTime_Patch
        {
            public static void Prefix(SGTimePlayPause __instance)
            {
                if (PauseAfterJump && PauseTimer)
                {
                    SimGameState simGame = GetInstanceField(typeof(SGTimePlayPause), __instance, "simState") as SimGameState;
                    simGame.ResumeTimer();

                    PauseTimer = false;
                    Log.Info($"Allowing Resume Timer");
                }
            }
        }


        //[HarmonyPatch(typeof(SGNavigationScreen), "OnTravelButtonClicked")]
        //public static class SGNavigationScreen_OnTravelButtonClicked_Patch
        //{
        //    public static bool StartBurn = false;
        //    public static bool Prefix(SGNavigationScreen __instance)
        //    {
        //        SimGameState simGame = GetInstanceField(typeof(SGNavigationScreen), __instance, "simState") as SimGameState;
        //        if (PauseAfterJump && PauseTimer && simGame.TravelState == SimGameTravelStatus.TRANSIT_FROM_JUMP)
        //        {
        //            simGame.TravelManager.SetTravelState(SimGameTravelStatus.IN_SYSTEM, true);

        //            StartBurn = true;
        //        }
        //    }

        //    public static void Postfix(SGNavigationScreen __instance)
        //    {
        //        SimGameState simGame = GetInstanceField(typeof(SGNavigationScreen), __instance, "simState") as SimGameState;
        //        if (StartBurn)
        //        {
        //            //simGame.TravelManager.SetTravelState(SimGameTravelStatus.TRANSIT_FROM_JUMP, true);
        //            StartBurn = false;
        //            simGame.ResumeTimer();
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(SGTravelManager), "HandleNextTravelStep")]
        // public static class SGTravelManager_HandleNextTravelStep_Patch
        // {
        //     public static bool Prefix(SGTravelManager __instance)
        //     {
        //         SimGameState simGame = GetInstanceField(typeof(SGTravelManager), __instance, "simState") as SimGameState;
        //         if (BlockedTravelState >= 0 && PauseAfterJump && simGame.Starmap.GetDestinationSystem() == simGame.Starmap.GetNextSystemInTravel() && __instance.TravelState == SimGameTravelStatus.WARMING_ENGINES)
        //         {
        //             //__instance.SetTravelState(SimGameTravelStatus.WARMING_ENGINES, false);
        //             //simGame.SetTravelTime(simGame.Starmap.CurPlanet.Cost, null);
        //             //simGame.Starmap.CurPlanet.Cost.AddFunds(-this.simState.Constants.Finances.JumpShipCost, null, true, true);

        //             BlockedTravelState = 1;
        //             Log.Info($"Blocking TRANSIT_FROM_JUMP");
        //             return false;

        //         }
        //         else if (PauseAfterJump)
        //         {
        //             BlockedTravelState = 0;

        //             Log.Info($"Unblocking TRANSIT_FROM_JUMP");
        //         }
        //         return true;
        //     }
        // }

        [HarmonyPatch(typeof(StarSystem), "GeneratePilots")]
        public static class StarSystem_GeneratePilots_Patch
        {
            public static void Postfix(StarSystem __instance)
            {
                try
                {
                    InfoClass.MercPilotInfo.IsGenInitPilots = true;
                    UpdateTheHubs(__instance.Sim);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
        #endregion

        #region Periodic Time Based Events
        [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
        public static class SimGameState_DaysPassed_Patch
        {
            static void Postfix(SimGameState __instance)
            {
                try
                {
                    UpdateTheHubs(__instance);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        //[HarmonyPatch(typeof(SimGameState), "StopPlayMode")]
        //public static class SimGameState_StopPlayMode_Patch
        //{
        //    public static void Postfix(SimGameState __instance)
        //    {
        //        try
        //        {
        //            UpdateTheHubs(__instance);
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e);
        //        }
        //    }
        //}
        #endregion

        //[HarmonyPatch(typeof(SGSystemViewPopulator), "UpdateRoutedSystem")]
        //public static class SGSystemViewPopulator_UpdateRoutedSystem_Patch
        //{
        //    static void Postfix(SGSystemViewPopulator __instance)
        //    {
        //        try
        //        {
        //            StarSystem starSystem = (StarSystem)AccessTools.Field(typeof(SGSystemViewPopulator), "starSystem").GetValue(__instance);
        //            List<LocalizableText> systemDescriptionFields = GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemDescriptionFields") as List<LocalizableText>;
        //            List<SGNavigationActiveFactionWidget> systemActiveFactionWidget = GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemActiveFactionWidget") as List<SGNavigationActiveFactionWidget>;
        //            //string SystemDesc = starSystem.Def.Description.Details;

        //            //string jumpTravel = starSystem.JumpDistance.ToString();
        //            __instance.SetField(systemDescriptionFields, $"[JUMP DISTANCE (IN-SYSTEM): {starSystem.JumpDistance}DAYS]{Environment.NewLine}{Environment.NewLine}{starSystem.Def.Description.Details}");
        //            //__instance.SetField(GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemTravelTime") as List<LocalizableText>, $"{starSystem.Sim.Starmap.ProjectedTravelTime} (Incl. {jumpTravel}Days Insystem travel)");
        //            //__instance.SetField(GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemNameFields") as List<LocalizableText>, $"{starSystem.Name}{Environment.NewLine}[Insystem travel is {jumpTravel}DAYS]");

        //            if (systemActiveFactionWidget != null)
        //            {
        //                List<string> systemFactions = new List<string>();
        //                systemFactions.AddRange(starSystem.Def.ContractEmployerIDList);
        //                systemFactions.AddRange(starSystem.Def.ContractTargetIDList.Where(x => !starSystem.Def.ContractEmployerIDList.Contains(x) && x != FactionEnumeration.GetAuriganDirectorateFactionValue().Name));
        //                systemActiveFactionWidget.ForEach(delegate (SGNavigationActiveFactionWidget widget)
        //                {
        //                    widget.ActivateFactions(systemFactions, starSystem.Def.OwnerValue.Name);
        //                });
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e);
        //        }
        //    }
        //}

        //// When starting a new career
        //[HarmonyPatch(typeof(SGCharacterCreationCareerBackgroundSelectionPanel), "Done")]
        //public class SGCharacterCreationCareerBackgroundSelectionPanel_Done
        //{
        //    public static void Prefix()
        //    {

        //    }
        //}

        //#region When loading from save that has Timeline set then set start date for updating the map to current date
        //[HarmonyPatch(typeof(SimGameState), "Init")]
        //public static class SimGameState_Init_Patch
        //{
        //    public static void Postfix(SimGameState __instance)
        //    {
        //        // Load from Date from save
        //    }
        //}

        //[HarmonyPatch(typeof(SimGameState), "InitFromSave")]
        //public static class SimGameState_InitFromSave_Patch
        //{
        //    public static void Postfix(SimGameState __instance)
        //    {

        //    }
        //}
        //#endregion
    }

    //public class SaveDeployments
    //{
    //    public bool Deployment = false;
    //    public Faction DeploymentEmployer = Faction.INVALID_UNSET;
    //    public Faction DeploymentTarget = Faction.INVALID_UNSET;
    //    public int DeploymentDifficulty = 1;
    //    public float DeploymentNegotiatedSalvage = 1;
    //    public float DeploymentNegotiatedPayment = 0;
    //    public int DeploymentSalary = 100000;
    //    public int DeploymentSalvage = 0;
    //    public int DeploymentLenght = 0;
    //    public int DeploymentRemainingDays = 0;
    //    public Dictionary<string, int> AlreadyRaised = new Dictionary<string, int>();
    //    public int MissionsDoneCurrentMonth = 0;

    //    public SaveDeployments(bool Deployment, Faction DeploymentEmployer,
    //            Faction DeploymentTarget, int DeploymentDifficulty, float DeploymentNegotiatedSalvage,
    //            float DeploymentNegotiatedPayment, int DeploymentSalary, int DeploymentSalvage, Dictionary<string, int> AlreadyRaised, int DeploymentLenght, int DeploymentRemainingDays, int MissionsDoneCurrentMonth)
    //    {

    //        this.Deployment = Deployment;
    //        this.DeploymentEmployer = DeploymentEmployer;
    //        this.DeploymentTarget = DeploymentTarget;
    //        this.DeploymentDifficulty = DeploymentDifficulty;
    //        this.DeploymentNegotiatedSalvage = DeploymentNegotiatedSalvage;
    //        this.DeploymentNegotiatedPayment = DeploymentNegotiatedPayment;
    //        this.DeploymentSalary = DeploymentSalary;
    //        this.DeploymentSalvage = DeploymentSalvage;
    //        this.AlreadyRaised = AlreadyRaised;
    //        this.DeploymentLenght = DeploymentLenght;
    //        this.DeploymentRemainingDays = DeploymentRemainingDays;
    //        this.MissionsDoneCurrentMonth = MissionsDoneCurrentMonth;
    //    }
    //}

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
    //            Log.Error(e);
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
    //            Log.Error(e);
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
    //            Log.Error(e);
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
    //            Log.Error(e);
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
    //            Log.Error(e);
    //        }
    //    }
    //}
}
