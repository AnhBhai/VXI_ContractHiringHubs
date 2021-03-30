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
    public class GenerateContract
    {
        // Generate Contract Settings
        public int MaxContracts { get; set; }
        public bool BuffSalvage { get; set; }
        public bool strictTargetReq { get; set; }
        public bool strictEmployerReq { get; set; }
        public bool strictOwnerReq { get; set; }
        public bool strictNeutralReq { get; set; }
        public bool strictHostileReq { get; set; }

        // Contract Factions
        public string ContractEmployer { get; set; }
        public string ContractTarget { get; set; }
        public string ContractEmpAlly { get; set; }
        public string ContractTgtAlly { get; set; }
        public string ContractNtlToAll { get; set; }
        public string ContractHtlToAll { get; set; }
        public string TargetOverride { get; set; }
        public string EmployerOverride { get; set; }

        // Lance Overrides
        public Dictionary<int, Dictionary<int, string>> PlayerTeamOverride { get; set; }
        public Dictionary<int, Dictionary<int, string>> TargetTeamOverride { get; set; }
        public Dictionary<int, Dictionary<int, string>> TgtAllyTeamOverride { get; set; }
        public Dictionary<int, Dictionary<int, string>> EmployerTeamOverride { get; set; }
        public Dictionary<int, Dictionary<int, string>> EmpAllyTeamOverride { get; set; }
        public Dictionary<int, Dictionary<int, string>> HostileTeamOverride { get; set; }

        // Contract Creation Settings
        public int MinDifficulty { get; set; }
        public int MaxDifficulty { get; set; }
        public float SalaryPct { get; set; }
        public float SalvagePct { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsNegotiable { get; set; }

        // Dialogue Overrides
        public string LongDescriptionStart { get; set; } // Created for MercGuild Travel Contracts
        public string ShortDescriptionStart { get; set; } // Created for MercDeployments Environment.NewLine
        public string EmployerShortDescription { get; set; } // Created for MercDeployments
        public string DariusLongDescription { get; set; } // Created for MercDeployments
        public string ContractID { get; set; } // Created for MercDeployments
        public bool IsDeployment { get; set; } // Created for MercDeployments

        public GenerateContract()
        {
            ContractEmployer = "";
            ContractTarget = "";
            ContractEmpAlly = "";
            ContractTgtAlly = "";
            ContractNtlToAll = "";
            ContractHtlToAll = "";
            TargetOverride = "";
            EmployerOverride = "";
            MaxContracts = 1;
            MinDifficulty = 1;
            MaxDifficulty = 1;
            SalaryPct = 0.5f;
            SalvagePct = 0.5f;
            LongDescriptionStart = "";
            ShortDescriptionStart = "";
            EmployerShortDescription = "";
            DariusLongDescription = "";
            ContractID = "";
            IsGlobal = false;
            IsNegotiable = true;
            IsDeployment = false;

            BuffSalvage = false;

            strictTargetReq = false;
            strictEmployerReq = false;
            strictOwnerReq = false;
            strictNeutralReq = false;
            strictHostileReq = false;

            PlayerTeamOverride = new Dictionary<int, Dictionary<int, string>> ();
            TargetTeamOverride = new Dictionary<int, Dictionary<int, string>> ();
            TgtAllyTeamOverride = new Dictionary<int, Dictionary<int, string>> ();
            EmployerTeamOverride = new Dictionary<int, Dictionary<int, string>> ();
            EmpAllyTeamOverride = new Dictionary<int, Dictionary<int, string>> ();
            HostileTeamOverride = new Dictionary<int, Dictionary<int, string>>();
        }

        //public GenerateContract(SimGameState.AddContractData addContractData)
        //{
        //    ContractEmployer = addContractData.Employer;
        //    ContractTarget = addContractData.Target;
        //    ContractEmpAlly = addContractData.EmployerAlly;
        //    ContractTgtAlly = addContractData.TargetAlly;
        //    ContractNtlToAll = addContractData.NeutralToAll;
        //    ContractHtlToAll = addContractData.HostileToAll;
        //    MaxContracts = 1;
        //    ActualDifficulty = addContractData.Difficulty;
            
        //    IsInitialized = true;
        //}

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
            Func<string, ContractOverride> func1;
            Func<ContractOverride, bool> func2;
            return (from c in MetadataDatabase.Instance.GetContractsByDifficultyRangeAndScopeAndOwnership((int)diffRange.MinDifficultyClamped, (int)diffRange.MaxDifficultyClamped, simGame.ContractScope, true)
                    where c.ContractTypeRow.IsSinglePlayerProcedural
                    group c.ContractID by (int)c.ContractTypeRow.ContractTypeID).ToDictionary((IGrouping<int, string> c) => c.Key, delegate (IGrouping<int, string> c)
                    {
                        Func<string, ContractOverride> selector;
                        selector = (func1 = ((string ci) => simGame.DataManager.ContractOverrides.Get(ci)));
                        IEnumerable<ContractOverride> source = c.Select(selector);
                        Func<ContractOverride, bool> predicate;
                        predicate = (func2 = ((ContractOverride ci) => IsWithinDifficultyRange(diffRange, GetDifficultyEnumFromValue(ci.difficulty))));
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

        public ContractOverride SetCustomTeamOverrides(ContractOverride ovr)
        {
            if (this.PlayerTeamOverride.Count() > 0 && ovr.player1Team.lanceOverrideList.Count() > 0)
            {
                for (int i = 0; i < this.PlayerTeamOverride.Count(); i++)
                {
                    if (this.PlayerTeamOverride.ContainsKey(i) && ovr.player1Team.lanceOverrideList.Count() > i)
                    {
                        foreach (KeyValuePair<int, string> keyValuePair in this.PlayerTeamOverride[i])
                        {
                            if (ovr.player1Team.lanceOverrideList[i].unitSpawnPointOverrideList.Count() > keyValuePair.Key)
                                ovr.player1Team.lanceOverrideList[i].unitSpawnPointOverrideList[keyValuePair.Key].pilotDefId = keyValuePair.Value;
                        }
                    }
                }
            }

            if (this.TargetTeamOverride.Count() > 0 && ovr.targetTeam.lanceOverrideList.Count() > 0)
            {
                for (int i = 0; i < this.TargetTeamOverride.Count(); i++)
                {
                    if (this.TargetTeamOverride.ContainsKey(i) && ovr.targetTeam.lanceOverrideList.Count() > i)
                    {
                        foreach (KeyValuePair<int, string> keyValuePair in this.TargetTeamOverride[i])
                        {
                            if (ovr.targetTeam.lanceOverrideList[i].unitSpawnPointOverrideList.Count() > keyValuePair.Key)
                                ovr.targetTeam.lanceOverrideList[i].unitSpawnPointOverrideList[keyValuePair.Key].pilotDefId = keyValuePair.Value;
                        }
                    }
                }
            }

            if (this.TgtAllyTeamOverride.Count() > 0 && ovr.targetsAllyTeam.lanceOverrideList.Count() > 0)
            {
                for (int i = 0; i < this.TgtAllyTeamOverride.Count(); i++)
                {
                    if (this.TgtAllyTeamOverride.ContainsKey(i) && ovr.targetsAllyTeam.lanceOverrideList.Count() > i)
                    {
                        foreach (KeyValuePair<int, string> keyValuePair in this.TgtAllyTeamOverride[i])
                        {
                            if (ovr.targetsAllyTeam.lanceOverrideList[i].unitSpawnPointOverrideList.Count() > keyValuePair.Key)
                                ovr.targetsAllyTeam.lanceOverrideList[i].unitSpawnPointOverrideList[keyValuePair.Key].pilotDefId = keyValuePair.Value;
                        }
                    }
                }
            }

            if (this.EmployerTeamOverride.Count() > 0 && ovr.employerTeam.lanceOverrideList.Count() > 0)
            {
                for (int i = 0; i < this.EmployerTeamOverride.Count(); i++)
                {
                    if (this.EmployerTeamOverride.ContainsKey(i) && ovr.employerTeam.lanceOverrideList.Count() > i)
                    {
                        foreach (KeyValuePair<int, string> keyValuePair in this.EmployerTeamOverride[i])
                        {
                            if (ovr.employerTeam.lanceOverrideList[i].unitSpawnPointOverrideList.Count() > keyValuePair.Key)
                                ovr.employerTeam.lanceOverrideList[i].unitSpawnPointOverrideList[keyValuePair.Key].pilotDefId = keyValuePair.Value;
                        }
                    }
                }
            }

            if (this.EmpAllyTeamOverride.Count() > 0 && ovr.employersAllyTeam.lanceOverrideList.Count() > 0)
            {
                for (int i = 0; i < this.EmpAllyTeamOverride.Count(); i++)
                {
                    if (this.EmpAllyTeamOverride.ContainsKey(i) && ovr.employersAllyTeam.lanceOverrideList.Count() > i)
                    {
                        foreach (KeyValuePair<int, string> keyValuePair in this.EmpAllyTeamOverride[i])
                        {
                            if (ovr.employersAllyTeam.lanceOverrideList[i].unitSpawnPointOverrideList.Count() > keyValuePair.Key)
                                ovr.employersAllyTeam.lanceOverrideList[i].unitSpawnPointOverrideList[keyValuePair.Key].pilotDefId = keyValuePair.Value;
                        }
                    }
                }
            }

            if (this.HostileTeamOverride.Count() > 0 && ovr.hostileToAllTeam.lanceOverrideList.Count() > 0)
            {
                for (int i = 0; i < this.HostileTeamOverride.Count(); i++)
                {
                    if (this.HostileTeamOverride.ContainsKey(i) && ovr.hostileToAllTeam.lanceOverrideList.Count() > i)
                    {
                        foreach (KeyValuePair<int, string> keyValuePair in this.EmpAllyTeamOverride[i])
                        {
                            if (ovr.hostileToAllTeam.lanceOverrideList[i].unitSpawnPointOverrideList.Count() > keyValuePair.Key)
                                ovr.hostileToAllTeam.lanceOverrideList[i].unitSpawnPointOverrideList[keyValuePair.Key].pilotDefId = keyValuePair.Value;
                        }
                    }
                }
            }

            return ovr;
        }

        public ContractOverride BuildOverride(ContractOverride ovr, int travelSeed)
        {
            ContractOverride contractOverride = new ContractOverride();

            contractOverride = ovr.Copy();
            contractOverride.travelOnly = false;
            contractOverride.useTravelCostPenalty = false;
            contractOverride.disableNegotations = !this.IsNegotiable;
            contractOverride.negotiatedSalary = this.SalaryPct;
            contractOverride.negotiatedSalvage = this.SalvagePct;
            contractOverride.travelSeed = travelSeed;

            return contractOverride;
        }

        public ContractOverride BuildTravelOverride(ContractOverride ovr, int travelSeed, SimGameState.AddContractData addContractData, StarSystem starSystem, out SimGameResultAction simGameResultAction)
        {
            ovr.FullRehydrate();
            ContractOverride contractOverride = new ContractOverride();
            contractOverride.CopyContractTypeData(ovr);
            contractOverride.contractName = ovr.contractName;
            contractOverride.difficulty = ovr.difficulty;
            contractOverride.travelOnly = true;
            contractOverride.useTravelCostPenalty = true;
            contractOverride.disableNegotations = !this.IsNegotiable;
            contractOverride.disableAfterAction = ovr.disableAfterAction;
            contractOverride.salvagePotential = ovr.salvagePotential;
            contractOverride.contractRewardOverride = ovr.contractRewardOverride;
            contractOverride.negotiatedSalary = this.SalaryPct;
            contractOverride.negotiatedSalvage = this.SalvagePct;

            SimGameEventResult simGameEventResult = new SimGameEventResult();
            simGameResultAction = new SimGameResultAction();
            int num2 = 14;
            simGameResultAction.Type = SimGameResultAction.ActionType.System_StartNonProceduralContract;
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

            return contractOverride;
        }

        public Contract CreateCustomContract(SimGameState simGame, SimGameState.AddContractData addContractData, ContractOverride ovr, GameContext context, BiomeSkin_MDD biomeSkin_MDD, bool isTravel = false)
        {
            StarSystem starSystem = context.GetObject(GameContextObjectTagEnum.TargetStarSystem) as StarSystem;
            int travelSeed = simGame.NetworkRandom.Int(0, int.MaxValue);
            double dblBuffSalvage = 0;
            ContractOverride contractOverride = new ContractOverride();
            SimGameResultAction simGameResultAction = new SimGameResultAction();
            if (isTravel)
                contractOverride = BuildTravelOverride(ovr, travelSeed, addContractData, starSystem, out simGameResultAction);
            else
                contractOverride = BuildOverride(ovr, travelSeed);

            if (!this.EmployerShortDescription.Equals(""))
                contractOverride.shortDescription = this.ShortDescriptionStart + this.EmployerShortDescription;
            else
                contractOverride.shortDescription = this.ShortDescriptionStart + ovr.shortDescription;

            if (!this.DariusLongDescription.Equals(""))
                contractOverride.longDescription = this.LongDescriptionStart + this.DariusLongDescription;
            else
                contractOverride.longDescription = this.LongDescriptionStart + ovr.longDescription;

            if (this.TargetOverride != "")
                contractOverride.targetTeam.faction = this.TargetOverride;

            if (this.EmployerOverride != "")
                contractOverride.employerTeam.faction = this.EmployerOverride;

            if (this.BuffSalvage && contractOverride.salvagePotential > 0) // Add logarithmic formula to add generous salvage [1 + sqrt(1+8n)] / 2 where n = existing salvage
                dblBuffSalvage = (1 + Math.Sqrt(1 + (8 * contractOverride.salvagePotential))) / 2; // goes from +2 for 1,2 up to +8 for 29,...,36
            else
                dblBuffSalvage = 0.0;

            contractOverride = SetCustomTeamOverrides(contractOverride);

            //contractOverride.difficultyUIModifier = ovr.difficultyUIModifier;
            int baseDiff = starSystem.Def.GetDifficulty(simGame.SimGameMode) + Mathf.FloorToInt(simGame.GlobalDifficulty);
            int min = this.MinDifficulty;
            int max = this.MinDifficulty;
            int difficulty = simGame.NetworkRandom.Int(min, max + 1);

            Logger.Log(" Contract(" + addContractData.Map + ", " + addContractData.MapPath + ", " + addContractData.EncounterGuid + ", " + contractOverride.ContractTypeValue + ", " + simGame.BattleTechGame + ", " + contractOverride + ", " + simGame.Context + ", " + true + ", " + difficulty + ", 0, null );");
            Contract contract = new Contract(addContractData.Map, addContractData.MapPath, addContractData.EncounterGuid, contractOverride.ContractTypeValue, simGame.BattleTechGame, contractOverride, context, true, difficulty, 0, null)
            {
                Override =
                {
                    travelSeed = travelSeed
                }
            };
            
            FactionValue factionValueTarget = GenerateContractFactions.GetFactionValueFromString(addContractData.Target);
            FactionValue factionValueEmployer = GenerateContractFactions.GetFactionValueFromString(addContractData.Employer);
            FactionValue factionValueTgtAlly = GenerateContractFactions.GetFactionValueFromString(addContractData.TargetAlly);
            FactionValue factionValueEmpAlly = GenerateContractFactions.GetFactionValueFromString(addContractData.EmployerAlly);
            FactionValue factionValueNeutral = GenerateContractFactions.GetFactionValueFromString(addContractData.NeutralToAll);
            FactionValue factionValueHstToAll = GenerateContractFactions.GetFactionValueFromString(addContractData.HostileToAll);

            factionValueTgtAlly = (factionValueTgtAlly.IsInvalidUnset ? factionValueTarget : factionValueTgtAlly);
            factionValueEmpAlly = (factionValueEmpAlly.IsInvalidUnset ? factionValueEmployer : factionValueEmpAlly);

            // Set Custom Lances


            //contract.Override.contract = contract;
            //contract.Override.GenerateUnits(contract.DataManager, simGame.CurrentDate, simGame.CompanyTags);

            Logger.Log(" PrepContract(" + contract + ", " + factionValueEmployer + ", " + factionValueEmpAlly + ", " + factionValueTarget + ", " + factionValueTgtAlly + ", " + factionValueNeutral + ", " + factionValueHstToAll + ", " + biomeSkin_MDD.BiomeSkin + ", " + contract.Override.travelSeed + ", " + starSystem + ");");
            simGame.PrepContract(contract, factionValueEmployer, factionValueEmpAlly, factionValueTarget, factionValueTgtAlly, factionValueNeutral, factionValueHstToAll, biomeSkin_MDD.BiomeSkin, contract.Override.travelSeed, starSystem);

            string seedGUID = contract.Override.travelSeed + contract.encounterObjectGuid;
            if(!addContractData.IsGlobal && isTravel)
                NonGlobalTravelContracts.AddTravelContract(seedGUID, simGameResultAction, this.SalaryPct, this.SalvagePct, this.IsDeployment);
            
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
                                                if (FilterContractOverride(eachOverride, system))
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
        
        public List<MapAndEncounters> GetListOfMapsEncountersContracts(SimGameState simGame, StarSystem system, ContractOverride ovr, WeightedList<MapAndEncounters> mapEncounterList)
        {
            List<MapAndEncounters> validMapsEncounters = new List<MapAndEncounters>();
            foreach (MapAndEncounters eachMapAndEncounters in mapEncounterList)
                    {
                if (!validMapsEncounters.Contains(eachMapAndEncounters))
                {
                    List<ContractOverride> tempContractOvr = new List<ContractOverride>();
                    foreach (EncounterLayer_MDD eachEncounterLayer_MDD in eachMapAndEncounters.Encounters)
                    {
                        if (eachEncounterLayer_MDD.ContractTypeRow.ContractTypeID == (long)ovr.ContractTypeValue.ID)
                        {
                            if (DoesContractMeetRequirements(simGame, system, eachMapAndEncounters, ovr))
                            {
                                validMapsEncounters.Add(eachMapAndEncounters);
                            }
                        }
                    }
                }
            }
            return validMapsEncounters;
        }

        public void GetEmployerTargetComparisons(IEnumerable<ComparisonDef> comparisons, out List<ComparisonDef> employer, out List<ComparisonDef> target, out List<ComparisonDef> neutralToAll, out List<ComparisonDef> hostileToAll)
        {
            employer = new List<ComparisonDef>();
            target = new List<ComparisonDef>();
            neutralToAll = new List<ComparisonDef>();
            hostileToAll = new List<ComparisonDef>();
            foreach (ComparisonDef comparisonDef in comparisons)
            {
                if (comparisonDef.obj.StartsWith("Employer"))
                {
                    employer.Add(comparisonDef);
                }
                else if (comparisonDef.obj.StartsWith("Target"))
                {
                    target.Add(comparisonDef);
                }
                else if (comparisonDef.obj.StartsWith("NeutralToAll"))
                {
                    neutralToAll.Add(comparisonDef);
                }
                else if (comparisonDef.obj.StartsWith("HostileToAll"))
                {
                    hostileToAll.Add(comparisonDef);
                }
            }
        }

        public bool FilterContractOverride(ContractOverride contractOverride, StarSystem starSystem)
        {
            IEnumerable<ComparisonDef> comparisons = contractOverride.requirementList.SelectMany((RequirementDef r) => r.RequirementComparisons);
            List<ComparisonDef> compareEmployer;
            List<ComparisonDef> compareTarget;
            List<ComparisonDef> compareNeutral;
            List<ComparisonDef> compareHostile;
            GetEmployerTargetComparisons(comparisons, out compareEmployer, out compareTarget, out compareNeutral, out compareHostile);

            bool returnValue = true;

            if (this.strictTargetReq)
            {
                foreach (ComparisonDef comparison in compareTarget)
                {
                    if (comparison.obj.Contains(this.ContractTarget) && comparison.val == 0)
                        return false;
                    if (!comparison.obj.Contains(this.ContractTarget) && comparison.val == 1)
                        returnValue = false;
                    if (starSystem.OwnerValue.Name == this.ContractTarget && strictOwnerReq)
                    {
                        if (comparison.obj.Contains("IsOwner") && comparison.val == 0)
                            return false;
                    }
                }
            }

            if (this.strictEmployerReq)
            {
                foreach (ComparisonDef comparison in compareEmployer)
                {
                    if (comparison.obj.Contains(this.ContractEmployer) && comparison.val == 0)
                        return false;
                    if (comparison.obj.Contains(this.ContractEmployer) && comparison.val == 1)
                        returnValue = true;
                    if (!comparison.obj.Contains(this.ContractEmployer) && comparison.val == 1 && !returnValue)
                        returnValue = false;
                    if (starSystem.OwnerValue.Name == this.ContractEmployer && strictOwnerReq)
                    {
                        if (comparison.obj.Contains("IsOwner") && comparison.val == 0)
                            return false;
                    }
                }
                if (!returnValue)
                    return returnValue;
            }

            if (this.strictNeutralReq)
            {
                foreach (ComparisonDef comparison in compareEmployer)
                {
                    if (comparison.obj.Contains(this.ContractNtlToAll) && comparison.val == 0)
                        return false;
                    if (comparison.obj.Contains(this.ContractNtlToAll) && comparison.val == 1)
                        returnValue = true;
                    if (!comparison.obj.Contains(this.ContractNtlToAll) && comparison.val == 1 && !returnValue)
                        returnValue = false;
                    if (starSystem.OwnerValue.Name == this.ContractNtlToAll && strictOwnerReq)
                    {
                        if (comparison.obj.Contains("IsOwner") && comparison.val == 0)
                            return false;
                    }
                }
                if (!returnValue)
                    return returnValue;
            }

            if (this.strictHostileReq)
            {
                foreach (ComparisonDef comparison in compareEmployer)
                {
                    if (comparison.obj.Contains(this.ContractHtlToAll) && comparison.val == 0)
                        return false;
                    if (comparison.obj.Contains(this.ContractHtlToAll) && comparison.val == 1)
                        returnValue = true;
                    if (!comparison.obj.Contains(this.ContractHtlToAll) && comparison.val == 1 && !returnValue)
                        returnValue = false;
                    if (starSystem.OwnerValue.Name == this.ContractHtlToAll && strictOwnerReq)
                    {
                        if (comparison.obj.Contains("IsOwner") && comparison.val == 0)
                            return false;
                    }
                }
                if (!returnValue)
                    return returnValue;
            }

            return returnValue;
        }

        public Dictionary<int, List<ContractOverride>> ListProceduralContracts(SimGameState simGame)
        {
            ContractDifficultyRange difficultyRange = new ContractDifficultyRange(this.MinDifficulty, this.MaxDifficulty, GetDifficultyEnumFromValue(this.MinDifficulty), GetDifficultyEnumFromValue(this.MaxDifficulty));
            Dictionary<int, List<ContractOverride>> potentialContracts = GetSinglePlayerProceduralContractOverrides(difficultyRange, simGame);
            
            return potentialContracts;
        }

        public int BuildProceduralContracts(SimGameState simGame, StarSystem targetSystem, bool clearExistingContracts, List<string> contractNames, bool canNegotiate = true, bool isGlobal = false)
        {
            ContractOverride ovr = new ContractOverride();

            Dictionary<int, List<ContractOverride>> listContractOvr = ListProceduralContracts(simGame);
            List<ContractOverride> listContractID = new List<ContractOverride>();

            List<ContractOverride> tmpLCOID = listContractOvr.SelectMany(x => x.Value).ToList<ContractOverride>();
            listContractID = tmpLCOID.FindAll(x => contractNames.Contains(x.ID));

            if (listContractID.Count > 0)
            {
                ovr = listContractID.GetRandomElement();
                return BuildProceduralContracts(simGame, targetSystem, clearExistingContracts, ovr);
            }

            return 0;
        }
        public int BuildProceduralContracts(SimGameState simGame, StarSystem targetSystem, bool clearExistingContracts, ContractOverride ovr, bool canNegotiate = true, bool isGlobal = false)
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
            //Dictionary<int, List<ContractOverride>> potentialContracts = GetSinglePlayerProceduralContractOverrides(difficultyRange, simGame);

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

                GameContext gameContext = new GameContext(simGame.Context);
                gameContext.SetObject(GameContextObjectTagEnum.TargetStarSystem, system);

                MapAndEncounters finalMapEncounter = new MapAndEncounters();
                ContractOverride finalContractOvr = new ContractOverride();
                WeightedList<MapAndEncounters> mapEncounterList = new WeightedList<MapAndEncounters>(WeightedListType.WeightedRandom, playableMaps.ToList(), source.ToList<int>(), 0);

                List<MapAndEncounters> validMapsEncountersContracts = this.GetListOfMapsEncountersContracts(simGame, system, ovr, mapEncounterList);

                if (validMapsEncountersContracts.Count > 0)
                {
                    finalMapEncounter = validMapsEncountersContracts.GetRandomElement();
                    finalContractOvr = ovr;
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
                addContractData.NeutralToAll = this.ContractNtlToAll;
                addContractData.HostileToAll = this.ContractHtlToAll;
                addContractData.TargetSystem = system.Name;
                addContractData.IsGlobal = this.IsGlobal;
                addContractData.Map = finalMapEncounter.Map.MapName;
                addContractData.MapPath = finalMapEncounter.Map.MapPath;
                addContractData.ContractName = finalContractOvr.ID;

                Logger.Log("AddContractData: " + " [ Sys-" + addContractData.TargetSystem + " | Gbl-" + addContractData.IsGlobal + " | Map-" + addContractData.Map + " | CtN-" + addContractData.ContractName + " | GUID-" + addContractData.EncounterGuid + " ] ");
                Logger.Log("With the following Factions: " + "[ Tgt-" + addContractData.Target + " | TgA-" + addContractData.TargetAlly + " | Emp-" + addContractData.Employer + " | EmA-" + addContractData.EmployerAlly + " | H2A-" + addContractData.HostileToAll + " ] ");
                Logger.Log("And the additional Details: " + " [ DNeg-" + finalContractOvr.disableNegotations + " | Sly-" + finalContractOvr.negotiatedSalary + " | Svg-" + finalContractOvr.negotiatedSalvage + " ] ");

                Contract contract = this.CreateCustomContract(simGame, addContractData, finalContractOvr, gameContext, finalMapEncounter.Map.BiomeSkinEntry, usingBreadcrumbs);
                contractList.Add(contract);
            }
            if (debugCount >= 20)
            {
                SimGameState.logger.LogWarning("METHOD GeneratePotentialContracts() :: made 20 unsuccesful attempts to create a contract");
            }
            return contractList.Count;
        }
        public int GenerateProceduralContracts(SimGameState simGame, StarSystem targetSystem, bool clearExistingContracts, bool canNegotiate = true, bool isGlobal = false)
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
                addContractData.NeutralToAll = this.ContractNtlToAll;
                addContractData.HostileToAll = this.ContractHtlToAll;
                addContractData.TargetSystem = system.Name;
                addContractData.IsGlobal = this.IsGlobal;
                addContractData.Map = finalMapEncounter.Map.MapName;
                addContractData.MapPath = finalMapEncounter.Map.MapPath;
                addContractData.ContractName = finalContractOvr.ID;

                Logger.Log("AddContractData: " + " [ Sys-" + addContractData.TargetSystem + " | Gbl-" + addContractData.IsGlobal + " | Map-" + addContractData.Map + " | CtN-" + addContractData.ContractName + " | GUID-" + addContractData.EncounterGuid + " ] ");
                Logger.Log("With the following Factions: " + "[ Tgt-" + addContractData.Target + " | TgA-" + addContractData.TargetAlly + " | Emp-" + addContractData.Employer + " | EmA-" + addContractData.EmployerAlly + " | H2A-" + addContractData.HostileToAll + " ] ");
                Logger.Log("And the additional Details: " + " [ DNeg-" + finalContractOvr.disableNegotations + " | Sly-" + finalContractOvr.negotiatedSalary + " | Svg-" + finalContractOvr.negotiatedSalvage + " ] ");

                Contract contract = this.CreateCustomContract(simGame, addContractData, finalContractOvr, gameContext, finalMapEncounter.Map.BiomeSkinEntry, usingBreadcrumbs);
                contractList.Add(contract);
            }
            if (debugCount >= 20)
            {
                SimGameState.logger.LogWarning("METHOD GeneratePotentialContracts() :: made 20 unsuccesful attempts to create a contract");
            }
            return contractList.Count;
        }
    }
}
