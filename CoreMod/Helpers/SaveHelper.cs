using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Harmony;
using BattleTech;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;
using BattleTech.Serialization;
using BattleTech.UI;
using Newtonsoft.Json;
using VXIContractManagement;
using VXIContractHiringHubs;

namespace Helpers
{
    public class MercGuildInfo
    {
        public DateTime DateHubUpdate = new DateTime(1978, 3, 2);
        public bool IsDeployment = false;
        public bool IsGenInitContracts = false;
        public int EmployerCosts = 0;
        
        public void ClearInfo()
        {
            //DateHubUpdate = new DateTime(1978, 3, 2);
            IsDeployment = false; 
            IsGenInitContracts = false;
            EmployerCosts = 0;
        }
    }

    public struct MercDeploymentStats
    {
        public int MilitaryCount;
        public double MilitarySkulls;
        public int PoliticalCount;
        public double PoliticalSkulls;
        
        public int Victories;
        public double VictorySkulls;
        public int Retreats;
        public int Defeats;
        public int MechsDestroyed;
        public int VeesDestroyed;

        public SortedDictionary<string, int> ContractTypeCounts;

        public int EthicsSum;
        public int BonusPaid;

        public int ProfessionalCount;
        public bool ProfessionalFail;

        public int TrainingCount;
        public double TrainingSkulls;
        public Dictionary<string, int> PilotTraining;

        public MercDeploymentStats(bool all = true)
        {
            MilitaryCount = 0;
            MilitarySkulls = 0;
            PoliticalCount = 0;
            PoliticalSkulls = 0;

            Victories = 0;
            VictorySkulls = 0;
            Retreats = 0;
            Defeats = 0;
            MechsDestroyed = 0;
            VeesDestroyed = 0;

            ContractTypeCounts = new SortedDictionary<string, int>();

            EthicsSum = 0;
            BonusPaid = 0;

            ProfessionalCount = 0;
            ProfessionalFail = false;

            TrainingCount = 0;
            TrainingSkulls = 0;
            Dictionary<string, int> pilotTraining = new Dictionary<string, int>();
            pilotTraining.Add("pilot_recruit_R1", 0); pilotTraining.Add("pilot_recruit_R2", 0); pilotTraining.Add("pilot_recruit_R3", 0); pilotTraining.Add("pilot_recruit_R4", 0); pilotTraining.Add("pilot_recruit_R5", 0); pilotTraining.Add("pilot_recruit_R6", 0);
            pilotTraining.Add("pilot_recruit_R7", 0); pilotTraining.Add("pilot_recruit_R8", 0); pilotTraining.Add("pilot_recruit_R9", 0); pilotTraining.Add("pilot_recruit_R10", 0); pilotTraining.Add("pilot_recruit_R11", 0); pilotTraining.Add("pilot_recruit_R12", 0);
            PilotTraining = pilotTraining;
        }
    }

    public struct DeploymentContractInfo
    {
        public string EmployerShortDesc;
        public string DariusLongDesc;
        public string TargetOverride;
        public string EmployerOverride;
        public string ContractTypeID;
        public string Category;
        public string Who;
        public string Why;
        public string Ethics;
        public int Difficulty;

        public DeploymentContractInfo(string targetOverride, string employerOverride, string employerShortDesc, string dariusLongDesc, string contractTypeID, string sCategory, string sWho, string sWhy, string sEthics, int difficulty)
        {
            EmployerShortDesc = employerShortDesc;
            DariusLongDesc = dariusLongDesc;
            TargetOverride = targetOverride;
            EmployerOverride = employerOverride;
            ContractTypeID = contractTypeID;
            Category = sCategory;
            Who = sWho;
            Why = sWhy;
            Ethics = sEthics;
            Difficulty = difficulty;
        }
    }

    public class MercDeploymentInfo
    {
        public string DeploymentFactionID = "";
        public DateTime DateDeploymentEnd = new DateTime(1978, 3, 2);
        public DateTime DateLastRefresh = new DateTime(1978, 3, 2);
        public bool IsDeployment = false;
        public bool IsGenInitContracts = false;
        public int EmployerCosts = 0;
        public int Wave = 0;
        public int NoWaveContracts = 0;
        public List<string> MissionTypes = new List<string>();
        public MercDeploymentStats MDStats = new MercDeploymentStats(true);
        public Dictionary<string, DeploymentContractInfo> DCInfo = new Dictionary<string, DeploymentContractInfo>();

        public bool CompletedEndGame;

        public void ClearInfo()
        {
            DeploymentFactionID = "";
            //DateDeploymentEnd = new DateTime(1978, 3, 2); ;
            //DateLastRefresh = new DateTime(1978, 3, 2); ;
            IsDeployment = false;
            //IsGenInitContracts = false;
            Wave = 0;
            NoWaveContracts = 0;
            EmployerCosts = 0;
            MissionTypes.Clear();
            DCInfo.Clear();
            //this.ClearMDStats();
            CompletedEndGame = false;
        }

        public void ClearMDStats(bool clearAll = true)
        {
            MDStats.MilitaryCount = 0;
            MDStats.MilitarySkulls = 0;
            MDStats.PoliticalCount = 0;
            MDStats.PoliticalSkulls = 0;

            MDStats.Victories = 0;
            MDStats.VictorySkulls = 0;
            MDStats.Retreats = 0;
            MDStats.Defeats = 0;
            MDStats.MechsDestroyed = 0;
            MDStats.VeesDestroyed = 0;

            MDStats.ContractTypeCounts.Clear();

            MDStats.EthicsSum = 0;
            MDStats.BonusPaid = 0;

            MDStats.ProfessionalCount = 0;
            MDStats.ProfessionalFail = false;

            MDStats.TrainingCount = 0;
            MDStats.TrainingSkulls = 0;
            MDStats.PilotTraining["pilot_recruit_R1"] = 0; MDStats.PilotTraining["pilot_recruit_R2"] = 0; MDStats.PilotTraining["pilot_recruit_R3"] = 0; MDStats.PilotTraining["pilot_recruit_R4"] = 0; MDStats.PilotTraining["pilot_recruit_R5"] = 0; MDStats.PilotTraining["pilot_recruit_R6"] = 0;
            MDStats.PilotTraining["pilot_recruit_R7"] = 0; MDStats.PilotTraining["pilot_recruit_R8"] = 0; MDStats.PilotTraining["pilot_recruit_R9"] = 0; MDStats.PilotTraining["pilot_recruit_R10"] = 0; MDStats.PilotTraining["pilot_recruit_R11"] = 0; MDStats.PilotTraining["pilot_recruit_R12"] = 0;
        }
    }

    public class MercPilotInfo
    {
        public bool IsGenInitPilots = false;
        public int BondsrefCount = 0;
        public int BondsmanMax = 0;
        public Dictionary<Gender, List<int>> PortraitUsed = new Dictionary<Gender, List<int>>();
    }

    public static class InfoClass
    {
        public static MercGuildInfo MercGuildInfo = new MercGuildInfo();
        public static MercPilotInfo MercPilotInfo = new MercPilotInfo();
        public static MercDeploymentInfo DeploymentInfo = new MercDeploymentInfo();
    }
    
    public static class ContractHiringHub_Save
    {
        public const int MERCGUILD = 0;
        public const int DEPLOYMENT = 1;
        public const int PILOT = 2;
        public static void SaveState(string instanceGUID, DateTime saveTime, int typeSave)
        {
            try
            {
                int unixTimestamp = (int)(saveTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ VXIContractHiringHubs.Main.Settings.modDirectory}").FullName).FullName;
                string filePath = "";
                switch (typeSave)
                {
                    case MERCGUILD:
                        filePath = baseDirectory + $"/ModSaves/MercGuildContracts/" + instanceGUID + "-" + unixTimestamp + ".json";
                        (new FileInfo(filePath)).Directory.Create();
                        using (StreamWriter writer = new StreamWriter(filePath, true))
                        {
                            string json = JsonConvert.SerializeObject(InfoClass.MercGuildInfo);
                            writer.Write(json);
                        }
                        break;
                    case DEPLOYMENT:
                        filePath = baseDirectory + $"/ModSaves/MercDeployDetails/" + instanceGUID + "-" + unixTimestamp + ".json";
                        (new FileInfo(filePath)).Directory.Create();
                        using (StreamWriter writer = new StreamWriter(filePath, true))
                        {
                            string json = JsonConvert.SerializeObject(InfoClass.DeploymentInfo);
                            writer.Write(json);
                        }
                        break;
                    case PILOT:
                        filePath = baseDirectory + $"/ModSaves/MercPilotDetails/" + instanceGUID + "-" + unixTimestamp + ".json";
                        (new FileInfo(filePath)).Directory.Create();
                        using (StreamWriter writer = new StreamWriter(filePath, true))
                        {
                            string json = JsonConvert.SerializeObject(InfoClass.MercPilotInfo);
                            writer.Write(json);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static void LoadState(string instanceGUID, DateTime saveTime, int typeSave)
        {
            try
            {
                int unixTimestamp = (int)(saveTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ VXIContractHiringHubs.Main.Settings.modDirectory}").FullName).FullName;
                switch (typeSave)
                {
                    case MERCGUILD:
                        string mercGuildPath = baseDirectory + $"/ModSaves/MercGuildContracts/" + instanceGUID + "-" + unixTimestamp + ".json";
                        if (File.Exists(mercGuildPath))
                        {
                            using (StreamReader r = new StreamReader(mercGuildPath))
                            {
                                string json = r.ReadToEnd();
                                MercGuildInfo save = JsonConvert.DeserializeObject<MercGuildInfo>(json);

                                InfoClass.MercGuildInfo.DateHubUpdate = save.DateHubUpdate;
                                InfoClass.MercGuildInfo.IsDeployment = save.IsDeployment;
                                InfoClass.MercGuildInfo.IsGenInitContracts = save.IsGenInitContracts;
                                InfoClass.MercGuildInfo.EmployerCosts = save.EmployerCosts;
                            }
                        }
                        break;
                    case DEPLOYMENT:
                        string deploymentPath = baseDirectory + $"/ModSaves/MercDeployDetails/" + instanceGUID + "-" + unixTimestamp + ".json";
                        if (File.Exists(deploymentPath))
                        {
                            using (StreamReader r = new StreamReader(deploymentPath))
                            {
                                string json = r.ReadToEnd();
                                MercDeploymentInfo save = JsonConvert.DeserializeObject<MercDeploymentInfo>(json);

                                InfoClass.DeploymentInfo.DeploymentFactionID = save.DeploymentFactionID;
                                InfoClass.DeploymentInfo.DateDeploymentEnd = save.DateDeploymentEnd;
                                InfoClass.DeploymentInfo.DateLastRefresh = save.DateLastRefresh;
                                InfoClass.DeploymentInfo.IsGenInitContracts = save.IsGenInitContracts;
                                InfoClass.DeploymentInfo.IsDeployment = save.IsDeployment;
                                InfoClass.DeploymentInfo.Wave = save.Wave;
                                InfoClass.DeploymentInfo.NoWaveContracts = save.NoWaveContracts;
                                InfoClass.DeploymentInfo.EmployerCosts = save.EmployerCosts;
                                InfoClass.DeploymentInfo.MissionTypes = save.MissionTypes;
                                InfoClass.DeploymentInfo.MDStats = save.MDStats;
                                InfoClass.DeploymentInfo.DCInfo = save.DCInfo;
                            }
                        }
                        break;
                    case PILOT:
                        string mercPilotPath = baseDirectory + $"/ModSaves/MercPilotDetails/" + instanceGUID + "-" + unixTimestamp + ".json";
                        if (File.Exists(mercPilotPath))
                        {
                            using (StreamReader r = new StreamReader(mercPilotPath))
                            {
                                string json = r.ReadToEnd();
                                MercPilotInfo save = JsonConvert.DeserializeObject<MercPilotInfo>(json);
                                InfoClass.MercPilotInfo.IsGenInitPilots = save.IsGenInitPilots;
                                InfoClass.MercPilotInfo.BondsrefCount = save.BondsrefCount;
                                InfoClass.MercPilotInfo.PortraitUsed = save.PortraitUsed;
                            }
                        }
                        break;
                }
                
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static void DeleteState(string instanceGUID, DateTime saveTime)
        {
            try
            {
                int unixTimestamp = (int)(saveTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ VXIContractHiringHubs.Main.Settings.modDirectory}").FullName).FullName;

                string mercGuildPath = baseDirectory + $"/ModSaves/MercGuildContracts/" + instanceGUID + "-" + unixTimestamp + ".json";

                if (File.Exists(mercGuildPath))
                {
                    File.Delete(mercGuildPath);
                }

                string deploymentPath = baseDirectory + $"/ModSaves/DeploymentDetails/" + instanceGUID + "-" + unixTimestamp + ".json";

                if (File.Exists(deploymentPath))
                {
                    File.Delete(deploymentPath);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }


        [HarmonyPatch(typeof(GameInstanceSave), MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(GameInstance), typeof(SaveReason) })]
        public static class GameInstanceSave_Constructor_Patch
        {
            static void Postfix(GameInstanceSave __instance, GameInstance gameInstance, SaveReason saveReason)
            {
                try
                {
                    SaveState(__instance.InstanceGUID, __instance.SaveTime, MERCGUILD);
                    SaveState(__instance.InstanceGUID, __instance.SaveTime, DEPLOYMENT);
                    SaveState(__instance.InstanceGUID, __instance.SaveTime, PILOT);
                }
                catch (Exception e)
                {
                    Log.Error(e);
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
                    LoadState(save.InstanceGUID, save.SaveTime, MERCGUILD);
                    LoadState(save.InstanceGUID, save.SaveTime, DEPLOYMENT);
                    LoadState(save.InstanceGUID, save.SaveTime, PILOT);
                }
                catch (Exception e)
                {
                    Log.Error(e);
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
                    Log.Error(e);
                }
            }
            static void Postfix(SlotModel slot)
            {
                // IF startCount > slot.count
                DeleteState(tmpInstanceGUID, tmpSaveTime);
                Log.Info($".DeleteState({tmpInstanceGUID}, {tmpSaveTime});");
            }
        }
    }
}
