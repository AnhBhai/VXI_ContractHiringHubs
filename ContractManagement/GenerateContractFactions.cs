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

namespace VXIContractManagement
{
    public static class GenerateContractFactions
    {

        //public bool IsNegotiable { get; set; }


        //public bool IsInitialized;
        //public GenerateContractFactions()
        //{
        //    IsInitialized = true;
        //}

        public class ContractParticipants
        {
            // Token: 0x06009328 RID: 37672 RVA: 0x00268BAC File Offset: 0x00266DAC
            public ContractParticipants(FactionValue target, WeightedList<FactionValue> targetAllies, WeightedList<FactionValue> employerAllies, List<FactionValue> neutrals, List<FactionValue> hostiles)
            {
                this.Target = target;
                this.TargetAllies = targetAllies;
                this.EmployerAllies = employerAllies;
                this.NeutralToAll = neutrals;
                this.HostileToAll = hostiles;
            }

            // Token: 0x04005D3B RID: 23867
            public FactionValue Target;

            // Token: 0x04005D3C RID: 23868
            public WeightedList<FactionValue> EmployerAllies;

            // Token: 0x04005D3D RID: 23869
            public WeightedList<FactionValue> TargetAllies;

            // Token: 0x04005D3E RID: 23870
            public List<FactionValue> NeutralToAll;

            // Token: 0x04005D3F RID: 23871
            public List<FactionValue> HostileToAll;
        }


        public static void SetTagsAndStats(SimGameState simGame, StarSystem system, MapAndEncounters level, EventScope scope, out TagSet tags, out StatCollection stats)
        {
            switch (scope)
            {
                case EventScope.Company:
                    tags = simGame.CompanyTags;
                    stats = simGame.CompanyStats;
                    return;
                case EventScope.MechWarrior:
                case EventScope.Mech:
                    break;
                case EventScope.Commander:
                    tags = simGame.CommanderTags;
                    stats = simGame.CommanderStats;
                    return;
                case EventScope.StarSystem:
                    tags = system.Tags;
                    stats = system.Stats;
                    return;
                default:
                    if (scope == EventScope.Map)
                    {
                        tags = MetadataDatabase.Instance.GetTagSetForTagSetEntry(level.Map.TagSetID);
                        stats = new StatCollection();
                        return;
                    }
                    break;
            }
            throw new Exception("Contracts cannot use the scope of: " + scope);
        }

        public static List<FactionValue> AddOrRemoveFactionTypeInList(bool add, List<FactionValue> listToModify, bool isRealFaction, bool isGreatHouse, bool isClan, bool isMercenary, bool isPirate, IEnumerable<FactionValue> blackList = null)
        {
            if (blackList == null)
            {
                blackList = new List<FactionValue>();
            }
            if (isRealFaction && isGreatHouse)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue);
                        }
                    }
                    else if (listToModify.Contains(factionValue))
                    {
                        listToModify.Remove(factionValue);
                    }
                }
            }
            if (isRealFaction && isClan)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue);
                        }
                    }
                    else if (listToModify.Contains(factionValue))
                    {
                        listToModify.Remove(factionValue);
                    }
                }
            }
            if (isRealFaction && isMercenary)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue);
                        }
                    }
                    else if (listToModify.Contains(factionValue))
                    {
                        listToModify.Remove(factionValue);
                    }
                }
            }
            if (isRealFaction && isPirate)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue);
                        }
                    }
                    else if (listToModify.Contains(factionValue))
                    {
                        listToModify.Remove(factionValue);
                    }
                }
            }
            return listToModify;
        }

        public static List<string> AddOrRemoveFactionTypeInList(bool add, List<string> listToModify, bool isRealFaction, bool isGreatHouse, bool isClan, bool isMercenary, bool isPirate, IEnumerable<FactionValue> blackList = null)
        {
            if (blackList == null)
            {
                blackList = new List<FactionValue>();
            }
            if (isRealFaction && isGreatHouse)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue.Name);
                        }
                    }
                    else if (listToModify.Contains(factionValue.Name))
                    {
                        listToModify.Remove(factionValue.Name);
                    }
                }
            }
            if (isRealFaction && isClan)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue.Name);
                        }
                    }
                    else if (listToModify.Contains(factionValue.Name))
                    {
                        listToModify.Remove(factionValue.Name);
                    }
                }
            }
            if (isRealFaction && isMercenary)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue.Name);
                        }
                    }
                    else if (listToModify.Contains(factionValue.Name))
                    {
                        listToModify.Remove(factionValue.Name);
                    }
                }
            }
            if (isRealFaction && isPirate)
            {
                foreach (FactionValue factionValue in FactionEnumeration.FactionList.FindAll((FactionValue faction) => faction.IsGreatHouse))
                {
                    if (add)
                    {
                        if (!blackList.Contains(factionValue))
                        {
                            listToModify.Add(factionValue.Name);
                        }
                    }
                    else if (listToModify.Contains(factionValue.Name))
                    {
                        listToModify.Remove(factionValue.Name);
                    }
                }
            }
            return listToModify;
        }

        // BUILD PUBLIC SetEmployerFromTarget method (exclude Clans)
        // BUILD PUBLIC SetAlliesHostile method (Exclude Clans)

        /// <summary>
        /// Used to set GenerateContracts properties using current StarSystem.
        /// 
        /// </summary>
        /// <param name="originSystem">REQUIRED: generateContract is the system where the contract is to be created</param>
        /// <param name="targetSystem">OPTIONAL: Use if the Target StarSystem differs from Current</param>
        /// <param name="targetName">[OPTIONAL] IF (targetName == VALID FACTION) overwrite generateContract.ContractTarget || ELSE IF (targetName == "INVALID_UNSET" OR generateContract.ContractTarget IS EMPTY) will retrieve faction based on SYSTEM OWNER</param>
        /// <param name="tgtAllyName">[OPTIONAL] IF (tgtAllyName == VALID FACTION) overwrite generateContract.ContractTarget || ELSE IF (tgtAllyName == "INVALID_UNSET" OR generateContract.ContractTgtAlly IS EMPTY) will retrieve faction based on generateContract.ContractTarget</param>
        /// <param name="employerName">[OPTIONAL] IF (employerName == VALID FACTION) overwrite generateContract.ContractTarget || ELSE IF (employerName == "INVALID_UNSET" OR generateContract.ContractEmployer IS EMPTY) generateContract will retrieve faction based on SYSTEM OWNER or generateContract.ContractTarget</param>
        /// <param name="empAllyName">[OPTIONAL] IF (empAllyName == VALID FACTION) overwrite generateContract.ContractTarget || ELSE IF (empAllyName == "INVALID_UNSET" OR generateContract.ContractEmpAlly IS EMPTY) will retrieve faction based on generateContract.ContractEmployer</param>
        /// <param name="neutralToAll">OPTIONAL: N/A Defaults to "INVALID_UNSET" unless given a Value</param>
        /// <param name="hostileToAll">[OPTIONAL] IF (targetName == VALID FACTION) overwrite generateContract.ContractTarget || ELSE IF (targetName == "INVALID_UNSET" OR generateContract.ContractTarget IS EMPTY) will retrieve faction based on SYSTEM OWNER</param>
        /// <returns></returns>
        public static bool setContractFactionsBasedOnSystems(GenerateContract generateContract, SimGameState simGame, StarSystem originSystem, StarSystem targetSystem = null, string targetName = "", string tgtAllyName = "", string employerName = "", string empAllyName = "", string neutralToAll = "INVALID_UNSET", string hostileToAll = "")
        {
            List<FactionValue> tmpFactionList = FactionEnumeration.ProceduralContractFactionList;
            List<string> clanFactions = tmpFactionList.FindAll(f => f.IsClan == true).ToList().ConvertAll(x => x.Name);
            List<string> sphereFactions = tmpFactionList.FindAll(f => f.IsClan == false).ToList().ConvertAll(x => x.Name);

            List<string> findSystemFaction = new List<string>();
            List<string> findNonSystemFaction = new List<string>();

            bool bEmpRemoved = false;

            string originSystemID = "";
            string targetSystemID = "";
            if (targetSystem is null)
            {
                originSystemID = targetSystemID = originSystem.OwnerDef.Name;
                targetSystem = originSystem;
            }
            else
            {
                originSystemID = originSystem.OwnerDef.Name;
                targetSystemID = targetSystem.OwnerDef.Name;
            }

            // Need to avoid duplicate Factions by removing factions assigned already or that will be assigned
            
            if (employerName != "INVALID_UNSET") // A Faction may need preserving
            {
                if (employerName != "")
                {
                    if (!clanFactions.Remove(employerName) && !sphereFactions.Remove(employerName))
                        employerName = ""; // Doesn't exist in lists so ignore and reset the parameter
                    else
                        bEmpRemoved = true;
                }
                if (generateContract.ContractEmployer != "" && employerName == "")
                {
                    if (!clanFactions.Remove(generateContract.ContractEmployer) && !sphereFactions.Remove(generateContract.ContractEmployer))
                        generateContract.ContractEmployer = ""; // Doesn't exist in lists so ignore and reset the property
                    else
                        bEmpRemoved = true;
                }
            }

            if (empAllyName != "INVALID_UNSET") // A Faction may need preserving
            {
                if (empAllyName != "")
                {
                    if (!clanFactions.Remove(empAllyName) && !sphereFactions.Remove(empAllyName))
                        empAllyName = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
                if (generateContract.ContractEmpAlly != "" && empAllyName == "")
                {
                    if (!clanFactions.Remove(generateContract.ContractEmpAlly) && !sphereFactions.Remove(generateContract.ContractEmpAlly))
                        generateContract.ContractEmpAlly = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
            }

            if (tgtAllyName != "INVALID_UNSET") // A Faction may need preserving
            {
                if (tgtAllyName != "")
                {
                    if (!clanFactions.Remove(tgtAllyName) && !sphereFactions.Remove(tgtAllyName))
                        empAllyName = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
                if (generateContract.ContractTgtAlly != "" && tgtAllyName == "")
                {
                    if (!clanFactions.Remove(generateContract.ContractTgtAlly) && !sphereFactions.Remove(generateContract.ContractTgtAlly))
                        generateContract.ContractTgtAlly = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
            }

            if (hostileToAll != "INVALID_UNSET") // A Faction may need preserving
            {
                if (hostileToAll != "")
                {
                    if (!clanFactions.Remove(hostileToAll) && !sphereFactions.Remove(hostileToAll))
                        empAllyName = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
                if (generateContract.ContractHtlToAll != "" && hostileToAll == "")
                {
                    if (!clanFactions.Remove(generateContract.ContractHtlToAll) && !sphereFactions.Remove(generateContract.ContractHtlToAll))
                        generateContract.ContractHtlToAll = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
            }

            // Handle Target Faction first
            if (generateContract.ContractTarget == "" || targetName == "INVALID_UNSET" || targetName != "")
            {
                if (targetName != "" && targetName != "INVALID_UNSET")
                {
                    if (clanFactions.Contains(targetName))
                    {
                        generateContract.ContractTarget = targetName;
                    }
                    else if (sphereFactions.Contains(targetName))
                    {
                        generateContract.ContractTarget = targetName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        targetName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (generateContract.ContractTarget == "" || targetName == "INVALID_UNSET")
                {
                    if (clanFactions.Contains(targetSystemID))
                    {
                        generateContract.ContractTarget = targetSystemID;
                    }
                    else if (sphereFactions.Contains(targetSystemID))
                    {
                        generateContract.ContractTarget = targetSystemID;
                    }
                    else // Owner doesn't exist in list, likely already another faction, so find another faction
                    {
                        foreach (string contractTargetID in targetSystem.Def.ContractTargetIDList)
                        {
                            if (clanFactions.Contains(contractTargetID) || sphereFactions.Contains(contractTargetID))
                                findSystemFaction.Add(contractTargetID);
                        }
                        if (findSystemFaction.Count > 0)
                        {
                            generateContract.ContractTarget = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else if (!targetSystem.OwnerValue.IsClan)
                        {
                            generateContract.ContractTarget = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                        else
                        {
                            generateContract.ContractTarget = clanFactions[simGame.NetworkRandom.Int(0, clanFactions.Count() - 1)];
                        }
                    }
                }
            }
            if (!clanFactions.Remove(generateContract.ContractTarget) && !sphereFactions.Remove(generateContract.ContractTarget))
            {
                Logger.Log("ERROR: GenerateContract.ContractTarget does not have a valid value: " + generateContract.ContractTarget);
            }
            findSystemFaction.Clear();

            if (neutralToAll == "INVALID_UNSET")
                generateContract.ContractNtlToAll = neutralToAll;
            else
                generateContract.ContractNtlToAll = "INVALID_UNSET";

            if (generateContract.ContractTgtAlly == "" || tgtAllyName == "INVALID_UNSET" || tgtAllyName != "")
            {
                if (tgtAllyName != "" && tgtAllyName != "INVALID_UNSET")
                {
                    if (clanFactions.Contains(tgtAllyName))
                    {
                        generateContract.ContractTgtAlly = tgtAllyName;
                    }
                    else if (sphereFactions.Contains(tgtAllyName))
                    {
                        generateContract.ContractTgtAlly = tgtAllyName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        tgtAllyName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (generateContract.ContractTgtAlly == "" || tgtAllyName == "INVALID_UNSET")
                {
                    if (generateContract.ContractTarget != "")
                    {
                        FactionValue tmpFactionValue = FactionEnumeration.GetFactionByName(generateContract.ContractTarget);
                        foreach (string contractTargetID in tmpFactionValue.FactionDef.Allies)
                        {
                            if (clanFactions.Contains(contractTargetID) || sphereFactions.Contains(contractTargetID))
                                findSystemFaction.Add(contractTargetID);
                        }
                        if (findSystemFaction.Count > 0)
                        {
                            generateContract.ContractTgtAlly = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else if (!targetSystem.OwnerValue.IsClan)
                        {
                            generateContract.ContractTgtAlly = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                        else
                        {
                            generateContract.ContractTgtAlly = clanFactions[simGame.NetworkRandom.Int(0, clanFactions.Count() - 1)];
                        }
                    }
                    else
                    {
                        Logger.Log("ERROR: GenerateContract.ContractTarget does not have a value: " + generateContract.ContractTarget);
                        return false;
                    }
                }
            }
            if (!clanFactions.Remove(generateContract.ContractTgtAlly) && !sphereFactions.Remove(generateContract.ContractTgtAlly))
            {
                Logger.Log("WARNING: GenerateContract.ContractTgtAlly may have been removed already: " + generateContract.ContractTgtAlly);
            }
            findSystemFaction.Clear();

            if (generateContract.ContractEmployer == "" || employerName == "INVALID_UNSET" || employerName != "")
            {
                if (employerName != "" && employerName != "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(employerName))
                    {
                        generateContract.ContractEmployer = employerName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        employerName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate employerName
                if (generateContract.ContractEmployer == "" || employerName == "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(originSystemID))
                    {
                        generateContract.ContractEmployer = originSystemID;
                    }
                    else // Owner doesn't exist in list, likely already another faction, so find another
                    {
                        foreach (string contractEmployerID in originSystem.Def.ContractEmployerIDList)
                        {
                            if (sphereFactions.Contains(contractEmployerID))
                                findSystemFaction.Add(contractEmployerID);
                        }
                        if (findSystemFaction.Count > 0)
                        {
                            generateContract.ContractEmployer = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else
                        {
                            generateContract.ContractEmployer = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                    }
                }
            }
            if (!clanFactions.Remove(generateContract.ContractEmployer) && !sphereFactions.Remove(generateContract.ContractEmployer) && !bEmpRemoved)
            {
                Logger.Log("ERROR: GenerateContract.ContractEmployer does not have a valid value: " + generateContract.ContractEmployer);
            }
            findSystemFaction.Clear();

            if (generateContract.ContractEmpAlly == "" || empAllyName == "INVALID_UNSET" || empAllyName != "")
            {
                if (empAllyName != "" && empAllyName != "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(empAllyName))
                    {
                        generateContract.ContractEmpAlly = empAllyName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        empAllyName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (generateContract.ContractEmpAlly == "" || empAllyName == "INVALID_UNSET")
                {
                    if (generateContract.ContractEmployer != "")
                    {
                        FactionValue tmpFactionValue = FactionEnumeration.GetFactionByName(generateContract.ContractEmployer);
                        foreach (string contractEmployerID in tmpFactionValue.FactionDef.Allies)
                        {
                            if (sphereFactions.Contains(contractEmployerID))
                                findSystemFaction.Add(contractEmployerID);
                        }
                        if (findSystemFaction.Count > 0)
                        {
                            generateContract.ContractEmpAlly = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else
                        {
                            generateContract.ContractEmpAlly = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                    }
                    else
                    {
                        Logger.Log("ERROR: GenerateContract.ContractEmployer does not have a value: " + generateContract.ContractEmployer);
                        return false;
                    }
                }
            }
            if (!clanFactions.Remove(generateContract.ContractEmpAlly) && !sphereFactions.Remove(generateContract.ContractEmpAlly))
            {
                Logger.Log("WARNING: GenerateContract.ContractEmpAlly may have been removed already: " + generateContract.ContractEmpAlly);
            }
            findSystemFaction.Clear();

            if (generateContract.ContractHtlToAll == "" || hostileToAll == "INVALID_UNSET" || hostileToAll != "")
            {
                if (hostileToAll != "" && hostileToAll != "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(hostileToAll))
                    {
                        generateContract.ContractHtlToAll = hostileToAll;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        hostileToAll = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (generateContract.ContractHtlToAll == "" || hostileToAll == "INVALID_UNSET")
                {
                    if (generateContract.ContractEmployer != "" && generateContract.ContractTarget != "")
                    {
                        FactionValue tmpFactionEnemy = FactionEnumeration.GetFactionByName(generateContract.ContractEmployer);
                        FactionValue tmpFactionAlly = FactionEnumeration.GetFactionByName(generateContract.ContractTarget);
                        foreach (string contractEmpEnemyID in tmpFactionEnemy.FactionDef.Enemies)
                        {
                            if (!tmpFactionAlly.FactionDef.Allies.Contains(contractEmpEnemyID))
                            {
                                if (targetSystem.OwnerValue.IsClan)
                                {
                                    if (clanFactions.Contains(contractEmpEnemyID))
                                        findSystemFaction.Add(contractEmpEnemyID);
                                }
                                else if (sphereFactions.Contains(contractEmpEnemyID))
                                {
                                    findSystemFaction.Add(contractEmpEnemyID);
                                }
                            }
                        }
                        tmpFactionEnemy = FactionEnumeration.GetFactionByName(generateContract.ContractTarget);
                        tmpFactionAlly = FactionEnumeration.GetFactionByName(generateContract.ContractEmployer);
                        foreach (string contractTgtEnemyID in tmpFactionEnemy.FactionDef.Enemies)
                        {
                            if (!tmpFactionAlly.FactionDef.Allies.Contains(contractTgtEnemyID))
                            {
                                if (targetSystem.OwnerValue.IsClan)
                                {
                                    if (clanFactions.Contains(contractTgtEnemyID))
                                        findSystemFaction.Add(contractTgtEnemyID);
                                }
                                else if (sphereFactions.Contains(contractTgtEnemyID))
                                {
                                    findSystemFaction.Add(contractTgtEnemyID);
                                }
                            }
                        }
                        if (findSystemFaction.Count > 0)
                        {
                            generateContract.ContractHtlToAll = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else if (targetSystem.OwnerValue.IsClan)
                        {
                            generateContract.ContractHtlToAll = clanFactions[simGame.NetworkRandom.Int(0, clanFactions.Count() - 1)];
                        }
                        else
                        {
                            generateContract.ContractHtlToAll = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                    }
                    else
                    {
                        Logger.Log("ERROR: Missing key GenerateContract values: ContractEmployer - " + generateContract.ContractEmployer + " || ContractEmployer - " + generateContract.ContractTarget);
                        return false;
                    }
                }
            }
            if (!clanFactions.Remove(generateContract.ContractHtlToAll) && !sphereFactions.Remove(generateContract.ContractHtlToAll))
            {
                Logger.Log("WARNING: GenerateContract.ContractHtlToAll may have been removed already: " + generateContract.ContractHtlToAll);
            }
            return true;
        }
        public static FactionValue GetFactionValueFromString(string factionID)
        {
            FactionValue result = FactionEnumeration.GetInvalidUnsetFactionValue();
            if (!string.IsNullOrEmpty(factionID))
            {
                result = FactionEnumeration.GetFactionByName(factionID);
            }
            return result;
        }
        public static string SetProceduralFactionID(FactionValue factionValue, string invalidFaction = "")
        {
            
            if (factionValue != null)
            {
                if (factionValue.IsProceduralContractFaction)
                    return factionValue.Name;
                else
                    return invalidFaction;
            }
            return invalidFaction;
        }
    }
}
