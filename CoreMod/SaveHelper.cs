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

namespace VXIContractHiringHubs
{
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
    [SerializableContract("MercGuildInfo")]
    public class SaveMercGuildInfo : IGuid
    {
        public DateTime DateHubUpdate;
        public bool IsMercGuildWorld = false;
        public bool IsGenInitContracts = false;
        public int EmployerCosts;
        public string GUID { get; private set; }

        public SaveMercGuildInfo()
        {
            this.DateHubUpdate = new DateTime(1978, 3, 2);
            this.IsGenInitContracts = false;
            this.IsMercGuildWorld = false;
            this.EmployerCosts = 0;
        }

        public void SetGuid(string newGuid)
        {
            this.GUID = newGuid;
        }
    }

    public class Helper
    {
        public static void SaveState(string instanceGUID, DateTime saveTime)
        {
            try
            {
                int unixTimestamp = (int)(saveTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ Core.Settings.modDirectory}").FullName).FullName;
                string filePath = baseDirectory + $"/ModSaves/MercGuildContracts/" + instanceGUID + "-" + unixTimestamp + ".json";
                (new FileInfo(filePath)).Directory.Create();
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    /*JsonSerializerSettings settings = new JsonSerializerSettings {
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,one 
                        Formatting = Formatting.Indented
                    };*/

                    string json = JsonConvert.SerializeObject(MercGuild.MercGuildInfo);
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
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ Core.Settings.modDirectory}").FullName).FullName;
                string filePath = baseDirectory + $"/ModSaves/MercGuildContracts/" + instanceGUID + "-" + unixTimestamp + ".json";
                if (File.Exists(filePath))
                {
                    using (StreamReader r = new StreamReader(filePath))
                    {
                        string json = r.ReadToEnd();
                        SaveMercGuildInfo save = JsonConvert.DeserializeObject<SaveMercGuildInfo>(json);

                        MercGuild.MercGuildInfo.DateHubUpdate = save.DateHubUpdate;
                        MercGuild.MercGuildInfo.IsGenInitContracts = save.IsGenInitContracts;
                        MercGuild.MercGuildInfo.IsMercGuildWorld = save.IsMercGuildWorld;
                        MercGuild.MercGuildInfo.EmployerCosts = save.EmployerCosts;
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
                string baseDirectory = Directory.GetParent(Directory.GetParent($"{ Core.Settings.modDirectory}").FullName).FullName;
                string filePath = baseDirectory + $"/ModSaves/MercGuildContracts/" + instanceGUID + "-" + unixTimestamp + ".json";

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
}
