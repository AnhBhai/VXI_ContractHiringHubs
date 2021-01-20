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
using Newtonsoft.Json;
using VXIContractManagement;
using BattleTech.Framework;
using BattleTech.Save;
using BattleTech.Save.SaveGameStructure;

public struct ContractActionDetails
{
    public string Map; // actionValue
    public int AdditionalSize;
    public string TargetSystem; // 0 Element
    public string MapPath; // 1 Element
    public string EncounterGuid; // 2 Element
    public string ContractName; // 3 Element
    public string IsGlobal; // 4 Element
    public string Employer; // 5 Element
    public string Target; // 6 Element
    public string Difficulty; // 7 Element
    public string CarryOverNegotiation;  // 8 Element
    public string TargetAlly;  // 9 Element
    public string RandomSeed;  // 10 Element
    public string EmployerAlly;  // 11 Element
    public string NeutralToAll;  // 12 Element
    public string HostileToAll;  // 13 Element

    public bool ActionInitialised;

    public ContractActionDetails(SimGameResultAction action, int additionalSize = 14)
    {
        Map = action.value; // actionValue
        AdditionalSize = additionalSize;
        TargetSystem = action.additionalValues[0]; // 0 Element
        MapPath = action.additionalValues[1]; // 1 Element
        EncounterGuid = action.additionalValues[2]; // 2 Element
        ContractName = action.additionalValues[3]; // 3 Element
        IsGlobal = action.additionalValues[4]; // 4 Element
        Employer = action.additionalValues[5]; // 5 Element
        Target = action.additionalValues[6]; // 6 Element
        Difficulty = action.additionalValues[7]; // 7 Element
        CarryOverNegotiation = action.additionalValues[8];  // 8 Element
        TargetAlly = action.additionalValues[9];  // 9 Element
        RandomSeed = action.additionalValues[10];  // 10 Element
        EmployerAlly = action.additionalValues[11];  // 11 Element
        NeutralToAll = action.additionalValues[12];  // 12 Element
        HostileToAll = action.additionalValues[13];  // 13 Element

        ActionInitialised = true;
    }
}
public struct ContractDetails
{
    //public SimGameResultAction Action;
    public ContractActionDetails ActionDetails;
    public float Salary;
    public float Salvage;
    
    public bool ContractInitialised;

    public ContractDetails(SimGameResultAction action, float salary, float salvage)
    {
        ActionDetails = new ContractActionDetails(action);
        if (ActionDetails.ActionInitialised)
        {
            Salary = salary;
            Salvage = salvage;

            ContractInitialised = true;
        }
        else
        {
            Salary = salary;
            Salvage = salvage;

            ContractInitialised = false;
        }
    }

    //public bool ContractActionDetails(SimGameResultAction action, int additionalSize = 14)
    //{
    //    Action = new SimGameResultAction();
    //    Action.value = action.value; // Map
    //    Action.additionalValues = new string[additionalSize];
    //    Action.additionalValues[0] = action.additionalValues[0]; //targetSystem; // 0 Element
    //    Action.additionalValues[1] = action.additionalValues[0]; //mapPath; // 1 Element
    //    Action.additionalValues[2] = action.additionalValues[0]; //encounterGuid; // 2 Element
    //    Action.additionalValues[3] = action.additionalValues[0]; //contractName; // 3 Element
    //    Action.additionalValues[4] = action.additionalValues[0]; //isGlobal.ToString(); // 4 Element
    //    Action.additionalValues[5] = action.additionalValues[0]; //employer; // 5 Element
    //    Action.additionalValues[6] = action.additionalValues[0]; //target; // 6 Element
    //    Action.additionalValues[7] = action.additionalValues[0]; //difficulty.ToString(); // 7 Element
    //    Action.additionalValues[8] = action.additionalValues[0]; //carryOverNegotiation.ToString();  // 8 Element
    //    Action.additionalValues[9] = action.additionalValues[0]; //targetAlly;  // 9 Element
    //    Action.additionalValues[10] = action.additionalValues[0]; //randomSeed.ToString();  // 10 Element
    //    Action.additionalValues[11] = action.additionalValues[0]; //employerAlly;  // 11 Element
    //    Action.additionalValues[12] = action.additionalValues[0]; //neutralToAll;  // 12 Element
    //    Action.additionalValues[13] = action.additionalValues[0]; //hostileToAll;  // 13 Element

        

    //    return true;
    //}
}

namespace VXIContractManagement
{
    [SerializableContract("NonGlobalTravelContracts")]
    public static class NonGlobalTravelContracts
    {
        // Unique contracts created for Merc Deployments and Merc Guild Contracts
        // Usually disableNegotiations == true and IsPriority == true
        public static bool IsNegotiable = false;
        public static ContractDisplayStyle PriorityDisplay = ContractDisplayStyle.BaseCampaignStory;


        //public static bool IsGlobal = false;
        public static Dictionary<string, ContractDetails> GuidContralDetails = new Dictionary<string, ContractDetails>();

        //public static List<ContractActionDetails> listContractDetails = new List<ContractActionDetails>();

        //public static bool AddTravelContract(Contract contract, string overrideID)
        //{
        //    ContractActionDetails actionDetails = new ContractActionDetails(contract.mapName, contract.TargetSystem, contract.mapPath, contract.encounterObjectGuid, overrideID, false, contract.Override.employerTeam.FactionValue.Name, contract.Override.targetTeam.FactionValue.Name, contract.Difficulty, true, contract.Override.targetsAllyTeam.FactionValue.Name, contract.Override.travelSeed, contract.Override.employersAllyTeam.FactionValue.Name, contract.Override.neutralToAllTeam.FactionValue.Name, contract.Override.hostileToAllTeam.FactionValue.Name);
            
        //    if (actionDetails.ActionInitialised)
        //    {
        //        ContractDetails details = new ContractDetails(contract.PercentageContractValue, contract.PercentageContractSalvage, actionDetails);
        //        if (details.ContractInitialised)
        //        {
        //            string GUID = contract.GUID;
        //            GuidContralDetails.Add(GUID, details);
        //            return true;
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    return false;
        //}

        public static bool AddTravelContract(string guid, SimGameResultAction action, float percentageContractValue, float percentageContractSalvage)
        {
            ContractDetails details = new ContractDetails(action, percentageContractValue, percentageContractSalvage);
            if (details.ContractInitialised)
            {
                string GUID = guid;
                GuidContralDetails.Add(GUID, details);
                return true;

            }
            else
            {
                return false;
            }
        }

        //public static bool RetrieveTravelAdditionals(string guid, SimGameResultAction action)
        //{
            
        //    Map = action.value; // actionValue
        //    TargetSystem = action.additionalValues[0]; // 0 Element
        //    MapPath = action.additionalValues[1]; // 1 Element
        //    EncounterGuid = action.additionalValues[2]; // 2 Element
        //    ContractName = action.additionalValues[3]; // 3 Element
        //    IsGlobal = (bool.TrueString == action.additionalValues[4]); // 4 Element
        //    Employer = action.additionalValues[5]; // 5 Element
        //    Target = action.additionalValues[6]; // 6 Element
        //    Difficulty = int.Parse(action.additionalValues[7]); // 7 Element
        //    CarryOverNegotiation = true;  // 8 Element
        //    TargetAlly = action.additionalValues[9];  // 9 Element
        //    RandomSeed = int.Parse(action.additionalValues[10]);  // 10 Element
        //    EmployerAlly = action.additionalValues[11];  // 11 Element
        //    NeutralToAll = action.additionalValues[12];  // 12 Element
        //    HostileToAll = action.additionalValues[13]; // 13 Element

        //    ActionInitialised = true;

        //    return ActionInitialised;
        //}

        public static void GetMercGuildActions(string guid, ref string actionValue, ref string[] additionalValues)
        {
            ContractDetails contractDetails;
            
            if (GuidContralDetails.TryGetValue(guid, out contractDetails))
            {
                if (contractDetails.ActionDetails.ActionInitialised)
                {
                    ContractActionDetails actionDetails = contractDetails.ActionDetails;

                    actionValue = actionDetails.Map;
                    additionalValues = new string[actionDetails.AdditionalSize];
                    additionalValues[0] = actionDetails.TargetSystem;
                    additionalValues[1] = actionDetails.MapPath;
                    additionalValues[2] = actionDetails.EncounterGuid;
                    additionalValues[3] = actionDetails.ContractName;
                    additionalValues[4] = actionDetails.IsGlobal.ToString();
                    additionalValues[5] = actionDetails.Employer;
                    additionalValues[6] = actionDetails.Target;
                    additionalValues[7] = actionDetails.Difficulty.ToString();
                    additionalValues[8] = actionDetails.CarryOverNegotiation.ToString();
                    additionalValues[9] = actionDetails.TargetAlly;
                    additionalValues[10] = actionDetails.RandomSeed.ToString();
                    additionalValues[11] = actionDetails.EmployerAlly;
                    additionalValues[12] = actionDetails.NeutralToAll;
                    additionalValues[13] = actionDetails.HostileToAll;
                }
            }
        }

        public static void GetMercGuildSettings(string guid, out float percentageContractValue, out float percentageContractSalvage)
        {
            percentageContractValue = GuidContralDetails[guid].Salary;
            percentageContractSalvage = GuidContralDetails[guid].Salvage;
        }

        public static void RetrieveAll()
        {

        }
    }

    public class Helper
    {
        public static void SaveState(string instanceGUID, DateTime saveTime)
        {
            try
            {
                int unixTimestamp = (int)(saveTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ ContractManagement.Settings.modDirectory}").FullName).FullName;
                string filePath = baseDirectory + $"/ModSaves/VXIContractManagement/" + instanceGUID + "-" + unixTimestamp + ".json";
                (new FileInfo(filePath)).Directory.Create();
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    /*JsonSerializerSettings settings = new JsonSerializerSettings {
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,one 
                        Formatting = Formatting.Indented
                    };*/

                    string json = JsonConvert.SerializeObject(NonGlobalTravelContracts.GuidContralDetails);
                    writer.Write(json);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public static void LoadState(string instanceGUID, DateTime saveTime)
        {
            try
            {
                int unixTimestamp = (int)(saveTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ ContractManagement.Settings.modDirectory}").FullName).FullName;
                string filePath = baseDirectory + $"/ModSaves/VXIContractManagement/" + instanceGUID + "-" + unixTimestamp + ".json";
                if (File.Exists(filePath))
                {
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        string json = r.ReadToEnd();
                        NonGlobalTravelContracts.GuidContralDetails.Clear();
                        NonGlobalTravelContracts.GuidContralDetails = JsonConvert.DeserializeObject<Dictionary<string, ContractDetails>>(json);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public static void DeleteState(string instanceGUID, DateTime saveTime)
        {
            try
            {
                int unixTimestamp = (int)(saveTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ ContractManagement.Settings.modDirectory}").FullName).FullName;
                string filePath = baseDirectory + $"/ModSaves/VXIContractManagement/" + instanceGUID + "-" + unixTimestamp + ".json";

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
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

    // Reset all travel contracts
    [HarmonyPatch(typeof(SimGameState), "OnBreadcrumbCancelledByUser")]
    public static class SimGameState_OnBreadcrumbCancelledByUser_Patch
    {
        static void Prefix(ref SimGameState __instance)
        {
            try
            {
                NonGlobalTravelContracts.GuidContralDetails = new Dictionary<string, ContractDetails>();
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
        static void Postfix(ref SimGameState __instance)
        {
            try
            {
                NonGlobalTravelContracts.GuidContralDetails = new Dictionary<string, ContractDetails>();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
    }

    [HarmonyPatch(typeof(StarSystem), "OnSystemExit")]
    public static class StarSystem_OnSystemExit_Patch
    {
        public static void Prefix(StarSystem __instance)
        {
            SimGameState simGame = __instance.Sim;

            Contract travelContract = Traverse.Create(simGame).Field("activeBreadcrumb").GetValue<Contract>();

            if (travelContract != null)
            {
                string seedGUID = travelContract.Override.travelSeed + travelContract.encounterObjectGuid;
                if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGUID))
                {
                    SimGameResultAction action = new SimGameResultAction();
                    NonGlobalTravelContracts.GetMercGuildActions(seedGUID, ref action.value, ref action.additionalValues);
                    float tmpSalary = NonGlobalTravelContracts.GuidContralDetails[seedGUID].Salary; 
                    float tmpSalvage = NonGlobalTravelContracts.GuidContralDetails[seedGUID].Salvage;

                    NonGlobalTravelContracts.GuidContralDetails.Clear();
                    NonGlobalTravelContracts.AddTravelContract(seedGUID, action, tmpSalary, tmpSalvage);
                }
            }
            else
            {
                NonGlobalTravelContracts.GuidContralDetails.Clear();
            }
        }
    }
}
