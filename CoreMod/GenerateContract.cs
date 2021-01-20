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

namespace VXIContractHiringHubs
{
    public class GenerateContract
    {
        public string ContractEmployer { get; set; }
        public string ContractTarget { get; set; }
        public string ContractEmpAlly { get; set; }
        public string ContractTgtAlly { get; set; }
        public string ContractNtlToAll { get; set; }
        public string ContractHtlToAll { get; set; }

        public int MaxContracts { get; set; }
        public int MinDifficulty { get; set; }
        public int MaxDifficulty { get; set; }
        public float SalvagePct { get; set; }
        public float SalaryPct { get; set; }
        public string LongDescriptionStart { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsNegotiable { get; set; }


        public bool IsInitialized;
        public GenerateContract()
        {
            ContractEmployer = "";
            ContractTarget = "";
            ContractEmpAlly = "";
            ContractTgtAlly = "";
            ContractNtlToAll = "";
            ContractHtlToAll = "";
            MaxContracts = 1;
            MinDifficulty = 1;
            MaxDifficulty = 1;
            SalvagePct = 0.5f;
            SalaryPct = 0.5f;
            LongDescriptionStart = "";
            IsGlobal = false;
            IsNegotiable = true;

            IsInitialized = true;
        }

        private class ContractDifficultyRange
        {
            public ContractDifficultyRange(int minDiff, int maxDiff, ContractDifficulty minDiffClamped, ContractDifficulty maxDiffClamped)
            {
                this.MinDifficulty = minDiff;
                this.MinDifficultyClamped = minDiffClamped;
                this.MaxDifficulty = maxDiff;
                this.MaxDifficultyClamped = maxDiffClamped;
            }

            public int MinDifficulty;

            public int MaxDifficulty;

            public ContractDifficulty MinDifficultyClamped;

            public ContractDifficulty MaxDifficultyClamped;
        }

        private static WeightedList<MapAndEncounters> GetSinglePlayerProceduralPlayableMaps(StarSystem system)
        {
            return MetadataDatabase.Instance.GetReleasedMapsAndEncountersBySinglePlayerProceduralContractTypeAndTags(system.Def.MapRequiredTags, system.Def.MapExcludedTags, system.Def.SupportedBiomes, true).ToWeightedList(WeightedListType.SimpleRandom);
        }

        private static ContractDifficulty GetDifficultyEnumFromValue(int value)
        {
            if (value >= 7)
            {
                return ContractDifficulty.Hard;
            }
            if (value >= 4)
            {
                return ContractDifficulty.Medium;
            }
            return ContractDifficulty.Easy;
        }

        private static bool IsWithinDifficultyRange(ContractDifficultyRange diffRange, ContractDifficulty ovrDiff)
        {
            return diffRange.MinDifficultyClamped <= ovrDiff && diffRange.MaxDifficultyClamped >= ovrDiff;
        }

        private static Dictionary<int, List<ContractOverride>> GetSinglePlayerProceduralContractOverrides(ContractDifficultyRange diffRange, SimGameState simGame)
        {
            Func<string, ContractOverride> blah5;
            Func<ContractOverride, bool> blah6;
            return (from c in MetadataDatabase.Instance.GetContractsByDifficultyRangeAndScopeAndOwnership((int)diffRange.MinDifficultyClamped, (int)diffRange.MaxDifficultyClamped, simGame.ContractScope, true)
                    where c.ContractTypeRow.IsSinglePlayerProcedural
                    group c.ContractID by (int)c.ContractTypeRow.ContractTypeID).ToDictionary((IGrouping<int, string> c) => c.Key, delegate (IGrouping<int, string> c)
                    {
                        Func<string, ContractOverride> selector;
                        selector = (blah5 = ((string ci) => simGame.DataManager.ContractOverrides.Get(ci)));
                        IEnumerable<ContractOverride> source = c.Select(selector);
                        Func<ContractOverride, bool> predicate;
                        predicate = (blah6 = ((ContractOverride ci) => IsWithinDifficultyRange(diffRange, GetDifficultyEnumFromValue(ci.difficulty))));
                        return source.Where(predicate).ToList<ContractOverride>();
                    });
        }

        private static bool HasValidMaps(StarSystem system, WeightedList<MapAndEncounters> contractMaps)
        {
            if (!contractMaps.Any<MapAndEncounters>())
            {
                Logger.Log(string.Format("No valid map for System {0}", system.Name));
                return false;
            }
            return true;
        }

        private class ContractParticipants
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


        private static void SetTagsAndStats(SimGameState simGame, StarSystem system, MapAndEncounters level, EventScope scope, out TagSet tags, out StatCollection stats)
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

        private static bool DoesContractMeetRequirements(SimGameState simGame, StarSystem system, MapAndEncounters level, ContractOverride contractOvr)
        {
            for (int i = 0; i < contractOvr.requirementList.Count; i++)
            {
                RequirementDef requirementDef = new RequirementDef(contractOvr.requirementList[i]);
                TagSet curTags;
                StatCollection stats;
                SetTagsAndStats(simGame, system, level, requirementDef.Scope, out curTags, out stats);
                requirementDef.RequirementComparisons = (from c in requirementDef.RequirementComparisons
                                                         where !c.obj.StartsWith("Target") && !c.obj.StartsWith("Employer")
                                                         select c).ToList<ComparisonDef>();
                if (!SimGameState.MeetsRequirements(requirementDef, curTags, stats, null))
                {
                    return false;
                }
            }
            return true;
        }

        

        private List<FactionValue> AddOrRemoveFactionTypeInList(bool add, List<FactionValue> listToModify, bool isRealFaction, bool isGreatHouse, bool isClan, bool isMercenary, bool isPirate, IEnumerable<FactionValue> blackList = null)
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

        private List<string> AddOrRemoveFactionTypeInList(bool add, List<string> listToModify, bool isRealFaction, bool isGreatHouse, bool isClan, bool isMercenary, bool isPirate, IEnumerable<FactionValue> blackList = null)
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

        // BUILD PUBLIC Owner, list<system> Dictionary (if doesn't exist)
        // BUILD PUBLIC SetEmployerFromTarget method (exclude Clans)
        // BUILD PUBLIC SetAlliesHostile method (Exclude Clans)

        //private Contract CreateCustomProceduralContract(StarSystem system, bool usingBreadcrumbs, MapAndEncounters level, SimGameState.MapEncounterContractData MapEncounterContractData, GameContext gameContext)
        //{
        //    WeightedList<SimGameState.PotentialContract> flatContracts = MapEncounterContractData.FlatContracts;
        //    this.FilterContracts(flatContracts);
        //    SimGameState.PotentialContract next = flatContracts.GetNext(true);
        //    int id = next.contractOverride.ContractTypeValue.ID;
        //    MapEncounterContractData.Encounters[id].Shuffle<EncounterLayer_MDD>();
        //    string encounterLayerGUID = MapEncounterContractData.Encounters[id][0].EncounterLayerGUID;
        //    ContractOverride contractOverride = next.contractOverride;
        //    FactionValue employer = next.employer;
        //    FactionValue target = next.target;
        //    FactionValue employerAlly = next.employerAlly;
        //    FactionValue targetAlly = next.targetAlly;
        //    FactionValue neutralToAll = next.NeutralToAll;
        //    FactionValue hostileToAll = next.HostileToAll;
        //    int difficulty = next.difficulty;
        //    Contract contract;
        //    if (usingBreadcrumbs)
        //    {
        //        contract = this.CreateTravelContract(level.Map.MapName, level.Map.MapPath, encounterLayerGUID, next.contractOverride.ContractTypeValue, contractOverride, gameContext, employer, target, targetAlly, employerAlly, neutralToAll, hostileToAll, false, difficulty);
        //    }
        //    else
        //    {
        //        contract = new Contract(level.Map.MapName, level.Map.MapPath, encounterLayerGUID, next.contractOverride.ContractTypeValue, this.BattleTechGame, contractOverride, gameContext, true, difficulty, 0, null);
        //    }
        //    this.mapDiscardPile.Add(level.Map.MapID);
        //    this.contractDiscardPile.Add(contractOverride.ID);
        //    this.PrepContract(contract, employer, employerAlly, target, targetAlly, neutralToAll, hostileToAll, level.Map.BiomeSkinEntry.BiomeSkin, contract.Override.travelSeed, system);
        //    return contract;
        //}

        /// <summary>
        /// Used to set GenerateContracts properties using current StarSystem.
        /// 
        /// </summary>
        /// <param name="originSystem">REQUIRED: This is the system where the contract is to be created</param>
        /// <param name="targetSystem">OPTIONAL: Use if the Target StarSystem differs from Current</param>
        /// <param name="targetName">[OPTIONAL] IF (targetName == VALID FACTION) overwrite this.ContractTarget || ELSE IF (targetName == "INVALID_UNSET" OR this.ContractTarget IS EMPTY) will retrieve faction based on SYSTEM OWNER</param>
        /// <param name="tgtAllyName">[OPTIONAL] IF (tgtAllyName == VALID FACTION) overwrite this.ContractTarget || ELSE IF (tgtAllyName == "INVALID_UNSET" OR this.ContractTgtAlly IS EMPTY) will retrieve faction based on ContractTarget</param>
        /// <param name="employerName">[OPTIONAL] IF (employerName == VALID FACTION) overwrite this.ContractTarget || ELSE IF (employerName == "INVALID_UNSET" OR this.ContractEmployer IS EMPTY) this will retrieve faction based on SYSTEM OWNER</param>
        /// <param name="empAllyName">[OPTIONAL] IF (empAllyName == VALID FACTION) overwrite this.ContractTarget || ELSE IF (empAllyName == "INVALID_UNSET" OR this.ContractEmpAlly IS EMPTY) will retrieve faction based on ContractEmployer</param>
        /// <param name="neutralToAll">OPTIONAL: NOT USED BY CONTRACTS THUS IGNORED</param>
        /// <param name="hostileToAll">[OPTIONAL] IF (targetName == VALID FACTION) overwrite this.ContractTarget || ELSE IF (targetName == "INVALID_UNSET" OR this.ContractTarget IS EMPTY) will retrieve faction based on SYSTEM OWNER</param>
        /// <returns></returns>
        public bool setContractFactionsFromSystem(SimGameState simGame, StarSystem originSystem, StarSystem targetSystem = null, string targetName = "", string tgtAllyName = "", string employerName = "", string empAllyName = "", string neutralToAll = "INVALID_UNSET", string hostileToAll = "")
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
                originSystemID = targetSystemID = originSystem.OwnerDef.ID;
                targetSystem = originSystem;
            }
            else
            {
                originSystemID = originSystem.OwnerDef.ID;
                targetSystemID = targetSystem.OwnerDef.ID;
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
                if (this.ContractEmployer != "" && employerName == "")
                {
                    if (!clanFactions.Remove(this.ContractEmployer) && !sphereFactions.Remove(this.ContractEmployer))
                        this.ContractEmployer = ""; // Doesn't exist in lists so ignore and reset the property
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
                if (this.ContractEmpAlly != "" && empAllyName == "")
                {
                    if (!clanFactions.Remove(this.ContractEmpAlly) && !sphereFactions.Remove(this.ContractEmpAlly))
                        this.ContractEmpAlly = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
            }

            if (tgtAllyName != "INVALID_UNSET") // A Faction may need preserving
            {
                if (tgtAllyName != "")
                {
                    if (!clanFactions.Remove(tgtAllyName) && !sphereFactions.Remove(tgtAllyName))
                        empAllyName = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
                if (this.ContractTgtAlly != "" && tgtAllyName == "")
                {
                    if (!clanFactions.Remove(this.ContractTgtAlly) && !sphereFactions.Remove(this.ContractTgtAlly))
                        this.ContractTgtAlly = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
            }

            if (hostileToAll != "INVALID_UNSET") // A Faction may need preserving
            {
                if (hostileToAll != "")
                {
                    if (!clanFactions.Remove(hostileToAll) && !sphereFactions.Remove(hostileToAll))
                        empAllyName = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
                if (this.ContractHtlToAll != "" && hostileToAll == "")
                {
                    if (!clanFactions.Remove(this.ContractHtlToAll) && !sphereFactions.Remove(this.ContractHtlToAll))
                        this.ContractHtlToAll = ""; // Doesn't exist in lists so ignore and reset the parameter
                }
            }

            // Handle Target Faction first
            if (this.ContractTarget == "" || targetName == "INVALID_UNSET" || targetName != "")
            {
                if (targetName != "" && targetName != "INVALID_UNSET")
                {
                    if (clanFactions.Contains(targetName))
                    {
                        this.ContractTarget = targetName;
                    }
                    else if (sphereFactions.Contains(targetName))
                    {
                        this.ContractTarget = targetName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        targetName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (this.ContractTarget == "" || targetName == "INVALID_UNSET")
                {
                    if (clanFactions.Contains(targetSystemID))
                    {
                        this.ContractTarget = targetSystemID;
                    }
                    else if (sphereFactions.Contains(targetSystemID))
                    {
                        this.ContractTarget = targetSystemID;
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
                            this.ContractTarget = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else if (!targetSystem.OwnerValue.IsClan)
                        {
                            this.ContractTarget = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                        else
                        {
                            this.ContractTarget = clanFactions[simGame.NetworkRandom.Int(0, clanFactions.Count() - 1)];
                        }
                    }
                }
            }
            if (!clanFactions.Remove(this.ContractTarget) && !sphereFactions.Remove(this.ContractTarget))
            {
                Logger.Log("ERROR: GenerateContract.ContractTarget does not have a valid value: " + this.ContractTarget);
            }

            if (this.ContractTgtAlly == "" || tgtAllyName == "INVALID_UNSET" || tgtAllyName != "")
            {
                if (tgtAllyName != "" && tgtAllyName != "INVALID_UNSET")
                {
                    if (clanFactions.Contains(tgtAllyName))
                    {
                        this.ContractTgtAlly = tgtAllyName;
                    }
                    else if (sphereFactions.Contains(tgtAllyName))
                    {
                        this.ContractTgtAlly = tgtAllyName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        tgtAllyName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (this.ContractTgtAlly == "" || tgtAllyName == "INVALID_UNSET")
                {
                    if (this.ContractTarget != "")
                    {
                        FactionValue tmpFactionValue = FactionEnumeration.GetFactionByName(this.ContractTarget);
                        foreach (string contractTargetID in tmpFactionValue.FactionDef.Allies)
                        {
                            if (clanFactions.Contains(contractTargetID) || sphereFactions.Contains(contractTargetID))
                                findSystemFaction.Add(contractTargetID);
                        }
                        if (findSystemFaction.Count > 0)
                        {
                            this.ContractTgtAlly = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else if (!targetSystem.OwnerValue.IsClan)
                        {
                            this.ContractTgtAlly = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                        else
                        {
                            this.ContractTgtAlly = clanFactions[simGame.NetworkRandom.Int(0, clanFactions.Count() - 1)];
                        }
                    }
                    else
                    {
                        Logger.Log("ERROR: GenerateContract.ContractTarget does not have a value: " + this.ContractTarget);
                        return false;
                    }
                }
            }
            if (!clanFactions.Remove(this.ContractTgtAlly) && !sphereFactions.Remove(this.ContractTgtAlly))
            {
                Logger.Log("WARNING: GenerateContract.ContractTgtAlly may have been removed already: " + this.ContractTarget);
            }

            if (this.ContractEmployer == "" || employerName == "INVALID_UNSET" || employerName != "")
            {
                if (employerName != "" && employerName != "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(employerName))
                    {
                        this.ContractEmployer = employerName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        employerName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate employerName
                if (this.ContractEmployer == "" || employerName == "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(originSystemID))
                    {
                        this.ContractEmployer = originSystemID;
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
                            this.ContractEmployer = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else
                        {
                            this.ContractEmployer = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                    }
                }
            }
            if (!clanFactions.Remove(this.ContractEmployer) && !sphereFactions.Remove(this.ContractEmployer) && !bEmpRemoved)
            {
                Logger.Log("ERROR: GenerateContract.ContractEmployer does not have a valid value: " + this.ContractEmployer);
            }

            if (this.ContractEmpAlly == "" || empAllyName == "INVALID_UNSET" || empAllyName != "")
            {
                if (empAllyName != "" && empAllyName != "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(empAllyName))
                    {
                        this.ContractEmpAlly = empAllyName;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        empAllyName = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (this.ContractEmpAlly == "" || empAllyName == "INVALID_UNSET")
                {
                    if (this.ContractEmployer != "")
                    {
                        FactionValue tmpFactionValue = FactionEnumeration.GetFactionByName(this.ContractEmployer);
                        foreach (string contractEmployerID in tmpFactionValue.FactionDef.Allies)
                        {
                            if (sphereFactions.Contains(contractEmployerID))
                                findSystemFaction.Add(contractEmployerID);
                        }
                        if (findSystemFaction.Count > 0)
                        {
                            this.ContractEmpAlly = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else
                        {
                            this.ContractEmpAlly = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                    }
                    else
                    {
                        Logger.Log("ERROR: GenerateContract.ContractEmployer does not have a value: " + this.ContractEmployer);
                        return false;
                    }
                }
            }
            if (!clanFactions.Remove(this.ContractEmpAlly) && !sphereFactions.Remove(this.ContractEmpAlly))
            {
                Logger.Log("WARNING: GenerateContract.ContractTgtAlly may have been removed already: " + this.ContractEmpAlly);
            }

            if (this.ContractHtlToAll == "" || hostileToAll == "INVALID_UNSET" || hostileToAll != "")
            {
                if (hostileToAll != "" && hostileToAll != "INVALID_UNSET")
                {
                    if (sphereFactions.Contains(hostileToAll))
                    {
                        this.ContractHtlToAll = hostileToAll;
                    }
                    else // Doesn't exist in lists so ignore parameter
                    {
                        hostileToAll = "";
                    }
                }
                // NOT AN ELSE because the above may fail to be legitimate targetName
                if (this.ContractHtlToAll == "" || hostileToAll == "INVALID_UNSET")
                {
                    if (this.ContractEmployer != "" && this.ContractTarget != "")
                    {
                        FactionValue tmpFactionEnemy = FactionEnumeration.GetFactionByName(this.ContractEmployer);
                        FactionValue tmpFactionAlly = FactionEnumeration.GetFactionByName(this.ContractTarget);
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
                        tmpFactionEnemy = FactionEnumeration.GetFactionByName(this.ContractTarget);
                        tmpFactionAlly = FactionEnumeration.GetFactionByName(this.ContractEmployer);
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
                            this.ContractHtlToAll = findSystemFaction[simGame.NetworkRandom.Int(0, findSystemFaction.Count() - 1)];
                        }
                        else if (targetSystem.OwnerValue.IsClan)
                        {
                            this.ContractHtlToAll = clanFactions[simGame.NetworkRandom.Int(0, clanFactions.Count() - 1)];
                        }
                        else
                        {
                            this.ContractHtlToAll = sphereFactions[simGame.NetworkRandom.Int(0, sphereFactions.Count() - 1)];
                        }
                    }
                    else
                    {
                        Logger.Log("ERROR: Missing key GenerateContract values: ContractEmployer - " + this.ContractEmployer + " || ContractEmployer - " + this.ContractTarget);
                        return false;
                    }
                }
            }
            if (!clanFactions.Remove(this.ContractHtlToAll) && !sphereFactions.Remove(this.ContractHtlToAll))
            {
                Logger.Log("WARNING: GenerateContract.ContractTgtAlly may have been removed already: " + this.ContractHtlToAll);
            }
            return true;
        }
        private FactionValue GetFactionValueFromString(string factionID)
        {
            FactionValue result = FactionEnumeration.GetInvalidUnsetFactionValue();
            if (!string.IsNullOrEmpty(factionID))
            {
                result = FactionEnumeration.GetFactionByName(factionID);
            }
            return result;
        }

        private Contract CreateCustomTravelContract(SimGameState simGame, SimGameState.AddContractData addContractData, ContractOverride contractOverride, GameContext context, BiomeSkin_MDD biomeSkin_MDD)
        {
            StarSystem starSystem = context.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
            int travelSeed = simGame.NetworkRandom.Int(0, int.MaxValue);
            //ContractOverride ovr = new ContractOverride();
            //contractOverride.CopyContractTypeData(ovr);
            //ovr.FullRehydrate();
            //contractOverride.contractName = ovr.contractName;
            //contractOverride.difficulty = ovr.difficulty;
            ////contractOverride.longDescription = ovr.longDescription;
            //contractOverride.shortDescription = ovr.shortDescription;
            //contractOverride.travelOnly = true;
            //contractOverride.useTravelCostPenalty = !addContractData.IsGlobal;
            ////contractOverride.disableNegotations = ovr.disableNegotations;
            //contractOverride.disableAfterAction = ovr.disableAfterAction;
            //contractOverride.salvagePotential = ovr.salvagePotential;
            //contractOverride.contractRewardOverride = ovr.contractRewardOverride;
            //contractOverride.travelSeed = travelSeed;
            //contractOverride.difficultyUIModifier = ovr.difficultyUIModifier;
            int baseDiff = starSystem.Def.GetDifficulty(simGame.SimGameMode) + Mathf.FloorToInt(simGame.GlobalDifficulty);
            int min = this.MinDifficulty;
            int max = this.MinDifficulty;
            int difficulty2 = simGame.NetworkRandom.Int(min, max + 1);
            SimGameEventResult simGameEventResult = new SimGameEventResult();
            SimGameResultAction simGameResultAction = new SimGameResultAction();
            int num2 = 14;
            simGameResultAction.Type = SimGameResultAction.ActionType.System_AddContract;
            simGameResultAction.value = addContractData.Map;
            simGameResultAction.additionalValues = new string[num2];
            simGameResultAction.additionalValues[0] = starSystem.ID;
            simGameResultAction.additionalValues[1] = addContractData.MapPath;
            simGameResultAction.additionalValues[2] = addContractData.EncounterGuid;
            simGameResultAction.additionalValues[3] = addContractData.ContractName;
            simGameResultAction.additionalValues[4] = addContractData.IsGlobal.ToString();
            simGameResultAction.additionalValues[5] = addContractData.Employer;
            simGameResultAction.additionalValues[6] = addContractData.Target;
            simGameResultAction.additionalValues[7] = contractOverride.difficulty.ToString();
            simGameResultAction.additionalValues[8] = "true";
            simGameResultAction.additionalValues[9] = addContractData.TargetAlly;
            simGameResultAction.additionalValues[10] = travelSeed.ToString();
            simGameResultAction.additionalValues[11] = addContractData.EmployerAlly;
            simGameResultAction.additionalValues[12] = addContractData.NeutralToAll;
            simGameResultAction.additionalValues[13] = addContractData.HostileToAll;
            simGameEventResult.Actions = new SimGameResultAction[1];
            simGameEventResult.Actions[0] = simGameResultAction;
            contractOverride.OnContractSuccessResults.Add(simGameEventResult);

            Logger.Log(" Contract(" + addContractData.Map + ", " + addContractData.MapPath + ", " + addContractData.EncounterGuid + ", " + contractOverride.ContractTypeValue + ", " + simGame.BattleTechGame + ", " + contractOverride + ", " + simGame.Context + ", " + true + ", " + difficulty2 + ", 0, null );");
            Contract contract = new Contract(addContractData.Map, addContractData.MapPath, addContractData.EncounterGuid, contractOverride.ContractTypeValue, simGame.BattleTechGame, contractOverride, context, true, difficulty2, 0, null)
            {
                Override =
                {
                    travelSeed = travelSeed
                }
            };
            
            FactionValue factionValueTarget = GetFactionValueFromString(addContractData.Target);
            FactionValue factionValueEmployer = GetFactionValueFromString(addContractData.Employer);
            FactionValue factionValueTgtAlly = GetFactionValueFromString(addContractData.TargetAlly);
            FactionValue factionValueEmpAlly = GetFactionValueFromString(addContractData.EmployerAlly);
            FactionValue factionValueNeutral = GetFactionValueFromString(addContractData.NeutralToAll);
            FactionValue factionValueHstToAll = GetFactionValueFromString(addContractData.HostileToAll);

            factionValueTgtAlly = (factionValueTgtAlly.IsInvalidUnset ? factionValueTarget : factionValueTgtAlly);
            factionValueEmpAlly = (factionValueEmpAlly.IsInvalidUnset ? factionValueEmployer : factionValueEmpAlly);

            contract.Override.disableNegotations = !this.IsNegotiable;
            contract.Override.negotiatedSalary = this.SalaryPct;
            contract.Override.negotiatedSalvage = this.SalvagePct;
            contract.Override.longDescription = this.LongDescriptionStart + contract.Override.longDescription;

            Logger.Log(" PrepContract(" + contract + ", " + factionValueEmployer + ", " + factionValueEmpAlly + ", " + factionValueTarget + ", " + factionValueTgtAlly + ", " + factionValueNeutral + ", " + factionValueHstToAll + ", " + biomeSkin_MDD.BiomeSkin + ", " + contract.Override.travelSeed + ", " + starSystem + ");");

            simGame.PrepContract(contract, factionValueEmployer, factionValueEmpAlly, factionValueTarget, factionValueTgtAlly, factionValueNeutral, factionValueHstToAll, biomeSkin_MDD.BiomeSkin, contract.Override.travelSeed, starSystem);
            return contract;
            
        }

        public Dictionary<MapAndEncounters, List<ContractOverride>> GetListOfMapsEncountersContracts(SimGameState simGame, StarSystem system, Dictionary<int, List<ContractOverride>> potentialContracts, WeightedList<MapAndEncounters> mapEncounterList)
        {
            Dictionary<MapAndEncounters, List<ContractOverride>> validMapsEncountersContracts = new Dictionary<MapAndEncounters, List<ContractOverride>>();
            foreach (KeyValuePair<int, List<ContractOverride>> eachContract in potentialContracts)
            {
                //Logger.Log("Find Contract 2nd: " + eachContract.Value.Count);
                if (eachContract.Value.Count > 0 && mapEncounterList.Count >= 0)
                {
                    foreach (MapAndEncounters eachMapAndEncounters in mapEncounterList)
                    {
                        if (!validMapsEncountersContracts.ContainsKey(eachMapAndEncounters))
                        {
                            List<ContractOverride> tempContractOvr = new List<ContractOverride>();
                            foreach (EncounterLayer_MDD eachEncounterLayer_MDD in eachMapAndEncounters.Encounters)
                            {
                                foreach (ContractOverride eachOverride in eachContract.Value)
                                {
                                    if (!tempContractOvr.Contains(eachOverride))
                                    {
                                        //Logger.Log("Find Contract 3rd");
                                        if (eachEncounterLayer_MDD.ContractTypeRow.ContractTypeID == (long)eachOverride.ContractTypeValue.ID)
                                        {
                                            if (DoesContractMeetRequirements(simGame, system, eachMapAndEncounters, eachOverride))
                                            {
                                                tempContractOvr.Add(eachOverride);
                                            }
                                        }
                                    }
                                }
                            }
                            if (tempContractOvr.Count > 0)
                            {
                                validMapsEncountersContracts.Add(eachMapAndEncounters, tempContractOvr);
                            }
                        }
                    }
                }
            }
            return validMapsEncountersContracts;
        }

        public int GeneratePotentialContracts(SimGameState simGame, StarSystem targetSystem, bool clearExistingContracts, bool canNegotiate, bool isGlobal)
        {
            int debugCount = 0;
            bool usingBreadcrumbs = targetSystem != null;

            StarSystem system;
            List<Contract> contractList;

            if (usingBreadcrumbs) // Travel Contracts
            {
                system = targetSystem;
                contractList = simGame.CurSystem.SystemBreadcrumbs;
            }
            else // Non Travel Contract
            {
                system = simGame.CurSystem;
                contractList = simGame.CurSystem.SystemContracts;
            }

            if (clearExistingContracts)
            {
                Logger.Log("Clearing Contracts CL:" + contractList.Count + "|GC:" + simGame.GlobalContracts.Count + "|BC:" + simGame.CurSystem.SystemBreadcrumbs.Count + "|SC:" + simGame.CurSystem.SystemContracts.Count);
                contractList.Clear();
                Logger.Log("Cleared Contracts CL:" + contractList.Count + "|GC:" + simGame.GlobalContracts.Count + "|BC:" + simGame.CurSystem.SystemBreadcrumbs.Count + "|SC:" + simGame.CurSystem.SystemContracts.Count);
            }

            this.IsGlobal = isGlobal;
            this.IsNegotiable = canNegotiate;
            
            Logger.Log("Get Playables for: " + " [ MaxC-" + this.MaxContracts + " | MinD-" + this.MinDifficulty + " | MaxD-" + this.MaxDifficulty + " | Tgt-" + this.ContractTarget + " | TgA-" + this.ContractTgtAlly + " | Emp-" + this.ContractEmployer + " | EmA-" + this.ContractEmpAlly + " | H2A-" + this.ContractHtlToAll + " ]");

            WeightedList<MapAndEncounters> playableMaps = GetSinglePlayerProceduralPlayableMaps(system);
            
            ContractDifficultyRange difficultyRange = new ContractDifficultyRange(this.MinDifficulty, this.MaxDifficulty, GetDifficultyEnumFromValue(this.MinDifficulty), GetDifficultyEnumFromValue(this.MaxDifficulty));
            Dictionary<int, List<ContractOverride>> potentialContracts = GetSinglePlayerProceduralContractOverrides(difficultyRange, simGame);

            if (!HasValidMaps(system, playableMaps))
            {
                return 0;
            }

            Logger.Log("BUILD CONTRACT");
            while (contractList.Count < this.MaxContracts && debugCount < 20)
            {
                int num = debugCount;
                debugCount = num + 1;
                IEnumerable<int> source = from map in playableMaps
                                          select map.Map.Weight;
                
                //FactionValue factionHostile = FactionEnumeration.GetHostileMercenariesFactionValue();

                GameContext gameContext = new GameContext(simGame.Context);
                gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, system);

                MapAndEncounters finalMapEncounter = new MapAndEncounters();
                ContractOverride finalContractOvr = new ContractOverride();
                WeightedList<MapAndEncounters> mapEncounterList = new WeightedList<MapAndEncounters>(WeightedListType.WeightedRandom, playableMaps.ToList(), source.ToList<int>(), 0);

                Dictionary<MapAndEncounters, List<ContractOverride>> validMapsEncountersContracts = this.GetListOfMapsEncountersContracts(simGame, system, potentialContracts, mapEncounterList);
                
                if (validMapsEncountersContracts.Count > 0)
                {
                    finalMapEncounter = validMapsEncountersContracts.Keys.GetRandomElement();
                    finalContractOvr = validMapsEncountersContracts[finalMapEncounter].GetRandomElement<ContractOverride>();
                }
                else
                {
                    return contractList.Count;
                }
                
                List<EncounterLayer_MDD> list = new List<EncounterLayer_MDD>();
                foreach (EncounterLayer_MDD encounterLayer_MDD in finalMapEncounter.Encounters)
                {
                    if (encounterLayer_MDD.ContractTypeRow.ContractTypeID == (long)finalContractOvr.ContractTypeValue.ID)
                    {
                        list.Add(encounterLayer_MDD);
                    }
                }
                SimGameState.AddContractData addContractData = new SimGameState.AddContractData();

                addContractData.EncounterGuid = list[simGame.NetworkRandom.Int(0, list.Count - 1)].EncounterLayerGUID;
                addContractData.Target = this.ContractTarget;
                addContractData.TargetAlly = this.ContractTgtAlly;
                addContractData.Employer = this.ContractEmployer;
                addContractData.EmployerAlly = this.ContractEmpAlly;
                addContractData.HostileToAll = this.ContractHtlToAll;
                addContractData.TargetSystem = system.Name;
                addContractData.IsGlobal = this.IsGlobal;
                addContractData.Map = finalMapEncounter.Map.MapName;
                addContractData.MapPath = finalMapEncounter.Map.MapPath;
                addContractData.ContractName = finalContractOvr.ID;
                
                Logger.Log("AddContractData: " + " [ Sys-" + addContractData.TargetSystem + " | Gbl-" + addContractData.IsGlobal + " | Map-" + addContractData.Map + " | CtN-" + addContractData.ContractName + " | GUID-" + addContractData.EncounterGuid + " ] ");
                Logger.Log("With the following Factions: " + "[ Tgt-" + addContractData.Target + " | TgA-" + addContractData.TargetAlly + " | Emp-" + addContractData.Employer + " | EmA-" + addContractData.EmployerAlly + " | H2A-" + addContractData.HostileToAll + " ] ");
                Logger.Log("And the additional Details: " + " [ DNeg-" + finalContractOvr.disableNegotations + " | Sly-" + finalContractOvr.negotiatedSalary + " | Svg-" + finalContractOvr.negotiatedSalvage + " ] ");

                if (usingBreadcrumbs)
                {
                    Contract travelItem = this.CreateCustomTravelContract(simGame, addContractData, finalContractOvr, gameContext, finalMapEncounter.Map.BiomeSkinEntry);
                    contractList.Add(travelItem);
                }
                else
                {
                    Contract proceduralItem = simGame.AddContract(addContractData);
                    contractList.Add(proceduralItem);
                }
            }
            if (debugCount >= 20)
            {
                SimGameState.logger.LogWarning("METHOD GeneratePotentialContracts() :: made 20 unsuccesful attempts to create a contract");
            }
            return contractList.Count;
        }
    }
}
