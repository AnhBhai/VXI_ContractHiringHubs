using System;
using System.Collections.Generic;
using BattleTech.UI.TMProWrapper;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Harmony;
using HBS.Collections;
using BattleTech;
using BattleTech.UI;
using BattleTech.Serialization;
using VXIContractManagement;
using Helpers;
using static Helpers.ContractHiringHub_Save;
using static Helpers.GlobalMethods;
using System.Collections;
using BattleTech.Framework;

namespace VXIContractHiringHubs
{
    public static class ContractHiringHubs
    {
        public static double PercentageExpenses = 0.25;
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

                if (!InfoClass.MercPilotInfo.IsGenInitPilots)
                {
                    Log.Info("Update the Pilots"); // Update the Pilots
                    MercPilots.UpdateMercPilots(simGame);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

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
                    if (InfoClass.DeploymentInfo.IsDeployment)
                    {
                        
                    }
                    InfoClass.MercGuildInfo.ClearInfo();
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
                        if (InfoClass.DeploymentInfo.NoWaveContracts >= Main.Settings.ActiveDeploymentContracts)
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

        //[HarmonyPatch(typeof(StarSystem), "OnInitialContractFetched")]
        //public static class StarSystem_OnInitialContractFetched_Patch
        //{
        //    public static void Postfix(StarSystem __instance)
        //    {
        //        try
        //        {
        //            Log.Info("Initial Contracts Fetched so Update the Hubs");
        //            ContractHiringHubs.UpdateTheHubs(__instance.Sim);
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Error(e);
        //        }
        //    }
        //}

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
                        //isOpportunityMission = true;
                        Log.Info($"[SGContractsWidget_AddContract_PREFIX] {contract.Override.ID} is a non-negotiable Merc Guild mission");

                        // Temporarily set contractDisplayStyle
                        contract.Override.contractDisplayStyle = ContractDisplayStyle.BaseCampaignStory;
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

        [HarmonyPatch(typeof(Contract), "GenerateSalvage")]
        //[HarmonyAfter(new string[] { "com.github.mcb5637.BTSimpleMechAssembly" })]
        public static class Contract_GenerateSalvage_Patch
        {
            public static void Prefix(Contract __instance, List<UnitResult> enemyMechs, List<VehicleDef> enemyVehicles, List<UnitResult> lostUnits)
            {
                try
                {
                    Log.Info("Build stats post Battle and Launch any Deployment");
                    if (InfoClass.MercGuildInfo.IsDeployment && !InfoClass.DeploymentInfo.IsDeployment)
                    {
                        MercDeployment.MercDeployment_Start(__instance.BattleTechGame.Simulation, __instance.Override.employerTeam.FactionValue.Name);
                        NonGlobalTravelContracts.GuidContralDetails.Clear();
                    }
                    else if (InfoClass.DeploymentInfo.IsDeployment)
                    {
                        MercDeployment.RetrieveContractToUpdate(__instance.BattleTechGame.Simulation, __instance.Override.ID, __instance.TheMissionResult);
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
                //MercDeployment.MercDeployment_Start(__instance.Sim);
            }
        }

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

        [HarmonyPatch(typeof(SGSystemViewPopulator), "UpdateRoutedSystem")]
        public static class SGSystemViewPopulator_UpdateRoutedSystem_Patch
        {
            static void Postfix(SGSystemViewPopulator __instance)
            {
                try
                {
                    StarSystem starSystem = (StarSystem)AccessTools.Field(typeof(SGSystemViewPopulator), "starSystem").GetValue(__instance);
                    List<LocalizableText> systemDescriptionFields = GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemDescriptionFields") as List<LocalizableText>;
                    List<SGNavigationActiveFactionWidget> systemActiveFactionWidget = GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemActiveFactionWidget") as List<SGNavigationActiveFactionWidget>;
                    //string SystemDesc = starSystem.Def.Description.Details;

                    //string jumpTravel = starSystem.JumpDistance.ToString();
                    __instance.SetField(systemDescriptionFields, $"[JUMP DISTANCE (IN-SYSTEM): {starSystem.JumpDistance}DAYS]{Environment.NewLine}{Environment.NewLine}{starSystem.Def.Description.Details}");
                    //__instance.SetField(GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemTravelTime") as List<LocalizableText>, $"{starSystem.Sim.Starmap.ProjectedTravelTime} (Incl. {jumpTravel}Days Insystem travel)");
                    //__instance.SetField(GetInstanceField(typeof(SGSystemViewPopulator), __instance, "SystemNameFields") as List<LocalizableText>, $"{starSystem.Name}{Environment.NewLine}[Insystem travel is {jumpTravel}DAYS]");

                    if (systemActiveFactionWidget != null)
                    {
                        List<string> systemFactions = new List<string>();
                        systemFactions.AddRange(starSystem.Def.ContractEmployerIDList);
                        systemFactions.AddRange(starSystem.Def.ContractTargetIDList.Where(x => !starSystem.Def.ContractEmployerIDList.Contains(x) && x != FactionEnumeration.GetAuriganDirectorateFactionValue().Name));
                        systemActiveFactionWidget.ForEach(delegate (SGNavigationActiveFactionWidget widget)
                        {
                            widget.ActivateFactions(systemFactions, starSystem.Def.OwnerValue.Name);
                        });
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

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
