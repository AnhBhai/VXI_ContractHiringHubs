using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Harmony;
using HBS.Collections;
using BattleTech;
using BattleTech.UI;
using BattleTech.Serialization;

namespace VXIContractHiringHubs
{
    public static class UpdateHubs
    {
        //public static DateTime DateHubUpdate = new DateTime(1978, 3, 2);

        public static void UpdateTheHubs(SimGameState simGame)
        {
            try
            {
                Logger.Log("Update the contracts");
                MercGuild.UpdateTheContracts(simGame);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

        }
                
        [HarmonyPatch(typeof(SimGameState), "PauseTimer")]
        public static class SimGameState_PauseTimer_Patch
        {
            public static void Postfix(SimGameState __instance)
            {
                try
                {
                    UpdateTheHubs(__instance);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        [HarmonyPatch(typeof(SimGameState), "StopPlayMode")]
        public static class SimGameState_StopPlayMode_Patch
        {
            public static void Postfix(SimGameState __instance)
            {
                try
                {
                    UpdateTheHubs(__instance);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        //[HarmonyPatch(typeof(SGTimePlayPause), "SetDay")]
        //public static class SGTimePlayPause_SetDay_Patch
        //{
        //    public static void Postfix(SGTimePlayPause __instance)
        //    {
        //        try
        //        {
        //            var simGame = Traverse.Create(__instance).Field("simState").GetValue<SimGameState>();
        //            UpdateTheHubs(simGame);

        //        }
        //        catch (Exception e)
        //        {
        //            Logger.Error(e);
        //        }
        //    }
        //}


        //[HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
        //public static class SimGameState_OnDayPassed_Patch
        //{
        //    public static void Postfix(SimGameState __instance)
        //    {
        //try
        //{
        //    //UpdateTheHubs(__instance);
        //}
        //catch (Exception e)
        //{
        //    Logger.Error(e);
        //}
        //    }
        //}

        // When starting a new career reset map update to start
        [HarmonyPatch(typeof(SGCharacterCreationCareerBackgroundSelectionPanel), "Done")]
        public class SGCharacterCreationCareerBackgroundSelectionPanel_Done
        {
            public static void Prefix()
            {
                
            }
        }

        #region When loading from save that has Timeline set then set start date for updating the map to current date
        [HarmonyPatch(typeof(SimGameState), "Init")]
        public static class SimGameState_Init_Patch
        {
            public static void Postfix(SimGameState __instance)
            {
                // Load from Date from save
            }
        }

        [HarmonyPatch(typeof(SimGameState), "InitFromSave")]
        public static class SimGameState_InitFromSave_Patch
        {
            public static void Postfix(SimGameState __instance)
            {
                
            }
        }
        #endregion
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
}
