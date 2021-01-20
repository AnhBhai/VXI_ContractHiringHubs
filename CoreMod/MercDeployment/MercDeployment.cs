﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Harmony;
using BattleTech;
using BattleTech.UI;
using BattleTech.Framework;
using fastJSON;
using HBS;
using HBS.Logging;
using Localize;
using UnityEngine.Events;
using VXIContractManagement;
using Helpers;
using static Helpers.GlobalMethods;
using static VXIContractHiringHubs.MercGuildDictionary;
using static Helpers.InfoClass;


namespace VXIContractHiringHubs
{
    public static class MercDeployment
    {
        public static string EndGame = MercGuildDictionary.EmpTextContractTypeID["EndGame"][0].Key;

        public static string AdjustEthics(SimGameState simGame, string sEthics)
        {
            int percent = simGame.NetworkRandom.Int(0, 100 + 1);
            int iChange = simGame.NetworkRandom.Int(-1, 1 + 1);

            if (!(Main.Settings.MercDeployEthic[sEthics] >= 3 && iChange >= 0) && !(Main.Settings.MercDeployEthic[sEthics] <= -3 && iChange <= 0))
            {
                Log.Info($"Before(Percent) {iChange}({percent}) for sEthics(Value) {sEthics}({Main.Settings.MercDeployEthic[sEthics]})");
                if (percent > 50)
                {
                    iChange = Main.Settings.MercDeployEthic[sEthics] + iChange;
                    sEthics = Main.Settings.MercDeployEthic.FirstOrDefault(x => x.Value == iChange).Key;
                    Log.Info($"After(Percent) {iChange}({percent}) for sEthics(Value) {sEthics}({Main.Settings.MercDeployEthic[sEthics]})");
                }
            }
            Log.Info($"sEthics(Value) {sEthics}({Main.Settings.MercDeployEthic[sEthics]})");

            return sEthics;
        }
        public static void PrepareDialogue(SimGameState simGame, ContractOverride ovr, GenerateContract generateContract, string sWho = "", string sWhat = "", string sWhy = "", string sHow = "", string sCategory = "", string sAttDef = "", string sEthics = "")
        {
            ContractListDetails contractListDetails = ContractListing[ovr.ID];

            if (sWho == "")
                sWho = contractListDetails.Who;

            if (sWhat == "")
                sWhat = contractListDetails.What;

            if (sWhy == "")
                sWhy = contractListDetails.Why;

            if (sHow == "")
                sHow = contractListDetails.How;


            if (sCategory == "")
                sCategory = contractListDetails.Category_Type.Key;

            if (sAttDef == "")
                sAttDef = contractListDetails.Category_Type.Value;


            if (sEthics == "")
            {
                sEthics = contractListDetails.Ethics;
                sEthics = AdjustEthics(simGame, sEthics);
            }

            if (sCategory != "Special")
            {

                if (!Main.Settings.MercDeployCategory.Contains(sCategory))
                {
                    sCategory = Main.Settings.MercDeployCategory.GetRandomElement();
                }


                Log.Info("==================================================================");
                Log.Info($"=== [{sCategory}]::[{contractListDetails.ContractType_Name.Key}] ===");

                if (sWho == "Random")
                    sWho = ListFlavourTextByTypeCategory(TextByContractTypeID[contractListDetails.ContractType_Name.Key].ToList<string>(), "Who", sCategory).GetRandomElement<string>();
                if (sWhat == "Random")
                    sWhat = ListFlavourTextByTypeCategory(TextByContractTypeID[contractListDetails.ContractType_Name.Key].ToList<string>(), "What", sCategory).GetRandomElement<string>();
                if (sWhy == "Random")
                    sWhy = ListFlavourTextByTypeCategory(TextByContractTypeID[contractListDetails.ContractType_Name.Key].ToList<string>(), "Why", sCategory).GetRandomElement<string>();
                if (sHow == "Random")
                    sHow = ListFlavourTextByTypeCategory(TextByContractTypeID[contractListDetails.ContractType_Name.Key].ToList<string>(), "How", sCategory).GetRandomElement<string>();

                Log.Info($"=== [{sWho}]::[{sWhat}] ===");
                Log.Info($"=== [{sWhy}]::[{sHow}] ===");
                Log.Info("==================================================================");

                if (!EmpTextContractTypeID.Any(x => x.Value.Any(y => y.Key == sAttDef)))
                    generateContract.EmployerShortDescription = EmpTextContractTypeID[contractListDetails.ContractType_Name.Key].GetRandomElement().Value;
                else
                    generateContract.EmployerShortDescription = EmpTextContractTypeID[contractListDetails.ContractType_Name.Key][Main.Settings.MercDeploySubType[sAttDef.ToUpper()]].Value;

                if (contractListDetails.EmployerFinalWords != "")
                    generateContract.EmployerShortDescription += contractListDetails.EmployerFinalWords;

                generateContract.DariusLongDescription = DariusText[sCategory].Find(f => f.Key == sEthics).Value;
            }
            else
            {
                if (EmpTextContractTypeID.ContainsKey(sWho))
                {
                    List<KeyValuePair<string, string>> listProfessionText = EmpTextContractTypeID[sWho];
                    foreach (KeyValuePair<string, string> text in listProfessionText)
                    {
                        if (text.Key == ovr.ID)
                            generateContract.EmployerShortDescription = text.Value;
                    }
                    if (generateContract.EmployerShortDescription == "")
                        generateContract.EmployerShortDescription = "How unprofessional of us to say but I have no information about this mission{Comma} you'll just need to launch and find out on the ground.";
                }
                else if (EmpTextContractTypeID.ContainsKey(sWhy))
                {
                    List<KeyValuePair<string, string>> listProfessionText = EmpTextContractTypeID[sWhy];
                    foreach (KeyValuePair<string, string> text in listProfessionText)
                    {
                        if (text.Key == ovr.ID)
                            generateContract.EmployerShortDescription = text.Value;
                    }
                    if (generateContract.EmployerShortDescription == "")
                        generateContract.EmployerShortDescription = "I guess our administrators need some training{Comma} there is no information about this training mission. You'll just need to launch with the training cadre and find out on the ground.";

                }

                if (DariusText.ContainsKey(sWho))
                {
                    List<KeyValuePair<string, string>> listProfessionText = DariusText[sWho];
                    foreach (KeyValuePair<string, string> text in listProfessionText)
                    {
                        if (text.Key == ovr.ID)
                            generateContract.DariusLongDescription = text.Value;
                    }
                    if (generateContract.DariusLongDescription == "")
                        generateContract.DariusLongDescription = "This is even unprofessional{Comma} even by my standards. Up to you: Do you want to launch{Comma} or not?";
                }
                else if (DariusText.ContainsKey(sWhy))
                {
                    List<KeyValuePair<string, string>> listProfessionText = DariusText[sWhy];
                    foreach (KeyValuePair<string, string> text in listProfessionText)
                    {
                        if (text.Key == ovr.ID)
                            generateContract.DariusLongDescription = text.Value;
                    }
                    if (generateContract.DariusLongDescription == "")
                        generateContract.DariusLongDescription = "Maybe I should offer to train them in the art of delivery. Its a training mission anyway{Comma} so you decide: Go{Comma} no go?";
                }
            }

            ReplaceTextDialogue(sWho, sWhat, sWhy, sHow, generateContract.EmployerShortDescription, generateContract.DariusLongDescription, out string shortDescription, out string longDescription, GenerateContractFactions.GetFactionValueFromString(generateContract.ContractTarget).FactionDef.ShortName, GenerateContractFactions.GetFactionValueFromString(generateContract.ContractEmployer).FactionDef.ShortName);
            generateContract.EmployerShortDescription = shortDescription;
            generateContract.DariusLongDescription = longDescription;

            if (ovr.contractName == "")
                ovr.contractName = contractListDetails.ContractType_Name.Value;
            PrintDialogue(ovr, generateContract, sCategory, sEthics, sAttDef);

            //UpdateMDStats(simGame, contractListDetails.ContractType_Name.Key, sCategory, sWho, sWhy, sEthics, ovr.difficulty);
            int i = 0;

            if (DeploymentInfo.DCInfo.ContainsKey(ovr.ID))
            {
                Log.Info($"Duplicate Key : {ovr.ID}");

                i++;
                while (DeploymentInfo.DCInfo.ContainsKey(ovr.ID + i) || i > 100) { }

                if (DeploymentInfo.DCInfo.ContainsKey(ovr.ID + (i + 1).ToString()))
                    Log.Info($"Unable to resolve Duplicate : {ovr.ID + (i + 1).ToString()}");
                else
                    DeploymentInfo.DCInfo.Add(ovr.ID + (i + 1).ToString(), new DeploymentContractInfo(generateContract.TargetOverride, generateContract.EmployerOverride, generateContract.ShortDescriptionStart + shortDescription, generateContract.LongDescriptionStart + longDescription, contractListDetails.ContractType_Name.Key, sCategory, sWho, sWhy, sEthics, ovr.difficulty));
            }
            else
            {
                DeploymentInfo.DCInfo.Add(ovr.ID, new DeploymentContractInfo(generateContract.TargetOverride, generateContract.EmployerOverride, generateContract.ShortDescriptionStart + shortDescription, generateContract.LongDescriptionStart + longDescription, contractListDetails.ContractType_Name.Key, sCategory, sWho, sWhy, sEthics, ovr.difficulty));
            }
        }

        public static void RetrieveContractToUpdate(SimGameState simGame, string contractID, MissionResult missionResult)
        {
            
            if (missionResult == MissionResult.Victory)
            {
                InfoClass.DeploymentInfo.MDStats.Victories++;
                if (DeploymentInfo.DCInfo.ContainsKey(contractID))
                {
                    UpdateMDStats(simGame, DeploymentInfo.DCInfo[contractID].ContractTypeID, DeploymentInfo.DCInfo[contractID].Category, DeploymentInfo.DCInfo[contractID].Who, DeploymentInfo.DCInfo[contractID].Why, DeploymentInfo.DCInfo[contractID].Ethics, DeploymentInfo.DCInfo[contractID].Difficulty);
                    DeploymentInfo.MDStats.VictorySkulls += DeploymentInfo.DCInfo[contractID].Difficulty / 2;
                }
                else
                {
                    Log.Info($"Issue with finding mission : {contractID}");
                    // Pay bonus still
                }
            }
            else if (missionResult == MissionResult.Retreat)
            {
                InfoClass.DeploymentInfo.MDStats.Retreats++;
            }
            else
            {
                InfoClass.DeploymentInfo.MDStats.Defeats++;
            }

            int bonusPayment = Main.Settings.DeploymentContractBonus;
            simGame.AddFunds(bonusPayment, null, false);
            Log.Info($"Deployment bonus increased funds by {bonusPayment}");
            InfoClass.DeploymentInfo.MDStats.BonusPaid += bonusPayment;
        }

        public static void UpdateMDStats(SimGameState simGame, string contractTypeID, string sCategory, string sWho, string sWhy, string sEthics, int difficulty)
        {
            if (sCategory == "Special")
            {
                if (sWho == "Professional")
                {
                    DeploymentInfo.MDStats.ProfessionalCount += 1;
                }
                else if (sWhy == "Training")
                {
                    DeploymentInfo.MDStats.TrainingCount += 1;
                    DeploymentInfo.MDStats.TrainingSkulls += difficulty / 2;
                    string[] pilots = sWho.Split(':');
                    foreach (string pilot in pilots)
                    {
                        if (int.TryParse(pilot, out int i))
                            DeploymentInfo.MDStats.PilotTraining[i] += 1;
                        else
                            DeploymentInfo.MDStats.PilotTraining[simGame.NetworkRandom.Int(1, 5 + 1)] += 1;
                    }
                }
            }
            else if (sCategory == "Military")
            {
                DeploymentInfo.MDStats.MilitaryCount += 1;
                DeploymentInfo.MDStats.MilitarySkulls += difficulty / 2;
            }
            else
            {
                DeploymentInfo.MDStats.PoliticalCount += 1;
                DeploymentInfo.MDStats.PoliticalSkulls += difficulty / 2;
            }
            DeploymentInfo.MDStats.EthicsSum += Main.Settings.MercDeployEthic[sEthics];

            if (DeploymentInfo.MDStats.ContractTypeCounts.ContainsKey(contractTypeID))
                DeploymentInfo.MDStats.ContractTypeCounts[contractTypeID] += 1;
            else
                DeploymentInfo.MDStats.ContractTypeCounts.Add(contractTypeID, 1);
        }

        public static void PrintDialogue(ContractOverride ovr, GenerateContract generateContract, string sCategory, string sEthics = "Unknown", string sAttDef = "Unknown")
        {
            string partDialogue = "";
            int iTmp = 101;
            bool bTmp = true;

            if (generateContract.ShortDescriptionStart != "")
            {
                Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Log.Info($"{generateContract.ShortDescriptionStart}");
            }
            else
            {
                Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Log.Info($"+++ Employer : {DeploymentInfo.DeploymentFactionID}");
                Log.Info($"+++ Contract : {ovr.contractName}");
                Log.Info($"+++ Target   : Davion");
                Log.Info($"+++ Biome    : Urban");
                Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                Log.Info($"+++  Employer Message  +++ SubType : {sAttDef.ToString()}   +++");
            }
            for (int i = 0; i < generateContract.EmployerShortDescription.Length - 1; i += iTmp)
            {
                iTmp = 101;
                if (generateContract.EmployerShortDescription.Length < i + iTmp)
                    bTmp = false;

                if (bTmp)
                {
                    partDialogue = generateContract.EmployerShortDescription.Substring(i, iTmp - 1);

                    iTmp = partDialogue.LastIndexOf(' ');
                    partDialogue = partDialogue.Substring(0, iTmp).TrimStart(' ');
                }
                else
                {
                    partDialogue = generateContract.EmployerShortDescription.Substring(i).TrimStart(' ');
                    iTmp = 101;
                }

                Log.Info($"+++ {partDialogue}");
            }
            bTmp = true;
            Log.Info("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Log.Info("+++   Darius Message   +++");
            Log.Info($"+++ Category : {sCategory} ++");
            Log.Info($"+++ Ethics   : {sEthics}     +++");
            Log.Info("++++++++++++++++++++++++++");
            for (int i = 0; i < generateContract.DariusLongDescription.Length - 1; i += iTmp)
            {
                iTmp = 101;
                if (generateContract.DariusLongDescription.Length < i + iTmp)
                    bTmp = false;

                if (bTmp)
                {
                    partDialogue = generateContract.DariusLongDescription.Substring(i, iTmp - 1);

                    iTmp = partDialogue.LastIndexOf(' ');
                    partDialogue = partDialogue.Substring(0, iTmp).TrimStart(' ');
                }
                else
                {
                    partDialogue = generateContract.DariusLongDescription.Substring(i).TrimStart(' ');
                    iTmp = 101;
                }

                Log.Info($"+++ {partDialogue}");
            }
            Log.Info("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
        }

        public static List<ContractOverride> GetContractDetails(Dictionary<int, List<ContractOverride>> listContractOvr)
        {
            List<ContractOverride> ListContractID = new List<ContractOverride>();

            // ContractListing

            List<ContractOverride> tmpLCOID = listContractOvr.SelectMany(x => x.Value).ToList<ContractOverride>();
            //List<string> tmpCLID = ContractListing.SelectMany(x => x.Value).Select(o => o.ContractID_Name.Key).ToList();
            //List<string> tmpCLID = ContractListing.Keys.ToList<string>();
            ListContractID = tmpLCOID.FindAll(x => ContractListing.ContainsKey(x.ID));

            foreach (ContractOverride ovr in ListContractID)
            {
                if (ContractListing.ContainsKey(ovr.ID))
                {
                    //Log.Info($"ContractID: {ovr.ID}");
                }
                else
                {
                    Log.Info($"***ERROR*** ContractID: {ovr.ID}");
                }
            }

            return ListContractID;
        }

        public static int MercGuildDeployment(GenerateContract generateContract, SimGameState simGame, FactionValue targetFaction, StarSystem targetSystem, int deploymentMissions)
        {
            string tmpDeployFaction = "";
            List<string> tmpFactionList = FactionEnumeration.ProceduralContractFactionList.Where(f => f.IsGreatHouse && !f.Name.Equals(targetFaction.Name)).ToList().ConvertAll<string>(x => x.Name);
            List<string> tmpBlacklist = new List<string>();

            tmpFactionList = tmpFactionList.Distinct().ToList();
            if (deploymentMissions > 0)
            {
                foreach (Contract contract in simGame.CurSystem.SystemContracts)
                {
                    string seedGUID = contract.Override.travelSeed + contract.encounterObjectGuid;
                    if (NonGlobalTravelContracts.GuidContralDetails.ContainsKey(seedGUID))
                    {
                        if (NonGlobalTravelContracts.GuidContralDetails[seedGUID].IsDeployment)
                        {
                            tmpBlacklist.Add(contract.Override.employerTeam.teamName);
                        }
                    }
                }
                if (tmpBlacklist.Count() < tmpFactionList.Count())
                    tmpFactionList.Where(x => !tmpFactionList.Contains(x)).ToList();
            }

            tmpDeployFaction = tmpFactionList.GetRandomElement();

            if (!targetFaction.FactionDef.Allies.Contains(tmpDeployFaction) && !targetFaction.Name.Equals(tmpDeployFaction))
            {
                generateContract.ShortDescriptionStart += $"** DEPLOYMENT ** This is a {Main.Settings.LengthDeploymentDays} Day Deployment, by accepting, you will travel to {targetSystem.Name} and begin {Main.Settings.LengthDeploymentDays}days of fighting.{Environment.NewLine}";
                generateContract.ShortDescriptionStart += $"+ You will be paid an escalating mission bonus, on top of usual rewards, starting at {Main.Settings.DeploymentContractBonus.ToString("C0")} CBills per mission.  With extra bonuses at 5, 10, 15 and 20 missions completed.{Environment.NewLine}";
                generateContract.ShortDescriptionStart += $"+ Your discretion on Political missions will be rewarded with a bonus to our relationship. This may have ramification on your other relationships.{Environment.NewLine}";
                generateContract.ShortDescriptionStart += $"+ Your assistance in training local recruits, will be appreciated. Good speed and thank you.{Environment.NewLine}{Environment.NewLine}";
                deploymentMissions++;
                Log.Info($"+++ DEPLOYMENT #{deploymentMissions} : {tmpDeployFaction} +++");
                generateContract.IsDeployment = true;
                generateContract.ContractEmployer = tmpDeployFaction;

                return deploymentMissions;
            }

            return 0;
        }
        public static void UpdateDeployment(SimGameState simGame)
        {
            try
            {
                DateTime currentDate = simGame.CurrentDate;

                int noGenMissions = 0;
                int totalMissions = 0;
                bool clearFirst = false;

                Log.Info("Game Date: " + currentDate.ToString("yyyy-MM-dd") + ":: DateHubUpdate: " + DeploymentInfo.DateLastRefresh.ToString("yyyy-MM-dd") + "(" + DeploymentInfo.IsDeployment + ")");
                if (currentDate > DeploymentInfo.DateLastRefresh.AddDays(Main.Settings.DeploymentContractRefresh) && DeploymentInfo.IsDeployment)
                {
                    if (currentDate > DeploymentInfo.DateDeploymentEnd)
                    {
                        MercDeployment_End(simGame);
                        ContractHiringHubs.UpdateTheHubs(simGame);
                    }
                    else
                    {

                        string currentSystem = simGame.CurSystem.Name;
                        string currentOwner = simGame.CurSystem.OwnerValue.Name;

                        Dictionary<int, List<ContractOverride>> listContractOvr = new Dictionary<int, List<ContractOverride>>();
                        List<ContractOverride> listContractID = new List<ContractOverride>();
                        ContractOverride ovr = new ContractOverride();

                        GenerateContract generateContract = new GenerateContract();

                        //generateContract.MaxDifficulty = (int)Traverse.Create(simGame.CurSystem.Def).Property("DefaultDifficulty").GetValue();
                        generateContract.MaxDifficulty = simGame.CurSystem.Def.GetDifficulty(simGame.SimGameMode) + 3;
                        generateContract.MinDifficulty = simGame.CurSystem.Def.GetDifficulty(simGame.SimGameMode) - 3;
                        listContractOvr = generateContract.ListProceduralContracts(simGame);
                        Log.Info($"listContractOvr.Count() : {listContractOvr.Count().ToString()}");
                        listContractID = GetContractDetails(listContractOvr);
                        if (DeploymentInfo.Wave == 0)
                        {
                            PauseNotification.Show($"START {Main.Settings.LengthDeploymentDays}DAY DEPLOYMENT", $"Well thats that Commander, I just finished signing us on to complete this {Main.Settings.LengthDeploymentDays}day Deployment for {DeploymentInfo.DeploymentFactionID}.{Environment.NewLine}"
                                                                                                                + $"Just like a Travel Contract there are some of the usual penalites for breaking a contract.{Environment.NewLine}"
                                                                                                                + $" If we break the deployment:{Environment.NewLine}"
                                                                                                                + $" + We will lose partial bonus payments and have to refund any costs that we have covered.{Environment.NewLine}"
                                                                                                                + $" + More importantly, our reputation with {DeploymentInfo.DeploymentFactionID} and the {FactionEnumeration.GetMercenaryReviewBoardFactionValue().FriendlyName} will be damaged.{Environment.NewLine}"
                                                                                                                + $" Although they may not be so harsh if we perform most of the deployment.{Environment.NewLine}"
                                                                                                                + $"{Environment.NewLine}"
                                                                                                                + $"Further note, we'll get a new set of contracts as we go and we get to select as many or as little as we like. Also, we'll receive contracts from other interested parties but only those endorsed by our employer.{Environment.NewLine}"
                                                                                                                + $"{Environment.NewLine}"
                                                                                                                + $"Could be some interesting players on both sides here Commander.", simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);
                        }

                        DeploymentInfo.Wave++;

                        List<string> listExclude = new List<string>();
                        DeploymentInfo.DCInfo.Clear();
                        //string longDesc = $"We'll be behind enemy lines here, we'll get generous salvage on this contract and our employer will cover jump costs and {PercentageExpenses * 100}% of our operating costs during travel. ";
                        for (int i = 0; totalMissions < Main.Settings.ActiveDeploymentContracts && i < Main.Settings.ActiveDeploymentContracts * 2; i++)
                        {
                            Log.Info("");
                            Log.Info("");

                            clearFirst = i == 0;

                            generateContract.MaxContracts = totalMissions + 1;
                            generateContract.SalaryPct = 0.5f;
                            generateContract.SalvagePct = 0.5f;
                            generateContract.BuffSalvage = false;
                            generateContract.strictTargetReq = generateContract.strictOwnerReq = false;

                            //generateContract.LongDescriptionStart = longDesc;
                            //generateContract.ContractEmployer = DeploymentInfo.DeploymentFactionID;
                            if (!listContractID.Where(x => !listExclude.Contains(x.ID.Substring(0, x.ID.IndexOf('_')))).Any())
                                listExclude.Clear();

                            if (simGame.NetworkRandom.Int(0, 100 + 1) <= Main.Settings.DeploymentAllyPct && Main.Settings.EnemyFactions.ContainsKey(simGame.CurSystem.OwnerValue.Name) && Main.Settings.EnemyFactions.ContainsKey(DeploymentInfo.DeploymentFactionID))
                            {
                                generateContract.ContractEmployer = Main.Settings.EnemyFactions[simGame.CurSystem.OwnerValue.Name].Where(x => !Main.Settings.EnemyFactions[DeploymentInfo.DeploymentFactionID].Contains(x) || x.Equals("Locals")).ToList().GetRandomElement();
                                generateContract.ShortDescriptionStart = $"[DEPLOYMENT PARTNER] :: ";
                            }

                            if (generateContract.ContractEmployer.Equals("") || generateContract.ContractEmployer == null)
                            {
                                generateContract.ContractEmployer = DeploymentInfo.DeploymentFactionID;
                                generateContract.ShortDescriptionStart = $"[STANDARD DEPLOYMENT :: ";
                            }

                            GenerateContractFactions.setContractFactionsBasedOnRandom(generateContract, simGame, simGame.CurSystem);

                            if (simGame.NetworkRandom.Int(0, 100 + 1) <= Main.Settings.DeploymentAllyPct + (DeploymentInfo.Wave * 3 - 12) && Main.Settings.AlliedFactions.ContainsKey(generateContract.ContractTarget) && DeploymentInfo.Wave > 2)
                            {
                                generateContract.ContractTgtAlly = generateContract.ContractTarget;
                                generateContract.ContractTarget = Main.Settings.AlliedFactions[generateContract.ContractTarget].GetRandomElement();
                                generateContract.ShortDescriptionStart += $"OFFWORLD ENEMY]{Environment.NewLine}";
                            }
                            else
                            {
                                generateContract.ShortDescriptionStart += $"INSYSTEM ENEMY]{Environment.NewLine}";
                            }

                            //int tmpPct = 1;
                            //if (simGame.CurSystem.Def.ContractTargetIDList.Count > 0)
                            //    tmpPct = simGame.CurSystem.Def.ContractTargetIDList.Count;

                            //tmpPct = (int)(Main.Settings.DeploymentAllyPct * (DeploymentInfo.Wave / 3) / tmpPct);

                            if (simGame.NetworkRandom.Int(0, 100 + 1) <= Main.Settings.DeploymentSpecialPct && Main.Settings.SpecialFactions.ContainsKey(generateContract.ContractTarget))
                            {
                                Log.Info($"Old Targets : {generateContract.ContractTarget} | {generateContract.ContractTgtAlly} for wave {DeploymentInfo.Wave} mission {totalMissions}");
                                generateContract.ContractTgtAlly = generateContract.ContractTarget;
                                generateContract.TargetOverride = Main.Settings.SpecialFactions[generateContract.ContractTarget].GetRandomElement();
                                generateContract.ShortDescriptionStart += $"[TARGET OF INTEREST :: ";
                            }
                            else
                            {
                                generateContract.ShortDescriptionStart += $"[STANDARD TARGET :: ";
                            }

                            if (simGame.NetworkRandom.Int(0, 100 + 1) <= Main.Settings.DeploymentSpecialPct + (DeploymentInfo.Wave * 4 - 16) && Main.Settings.SpecialFactions.ContainsKey(generateContract.ContractEmployer))
                            {
                                Log.Info($"Old Employers : {generateContract.ContractEmployer} | {generateContract.ContractEmpAlly} for wave {DeploymentInfo.Wave} mission {totalMissions}");
                                generateContract.ContractEmpAlly = generateContract.ContractEmployer;
                                generateContract.EmployerOverride = Main.Settings.SpecialFactions[generateContract.ContractEmployer].Where(x => x != generateContract.TargetOverride).GetRandomElement();
                                generateContract.ShortDescriptionStart += $"CLASSIFIED HIGH]{Environment.NewLine}{Environment.NewLine}";
                            }
                            else
                            {
                                generateContract.ShortDescriptionStart += $"CLASSIFIED MEDIUM-LOW]{ Environment.NewLine}{Environment.NewLine}";
                            }

                            

                            if ((DeploymentInfo.MDStats.ProfessionalCount < 3 && !DeploymentInfo.MDStats.ProfessionalFail) || !listContractID.Exists(x => x.ID.Equals(EndGame)))
                            {
                                //if (!listContractID.Exists(x => x.ID == endGame))
                                //    listExclude.Add(endGame);
                                ovr = listContractID.Where(x => !listExclude.Contains(x.ID.Substring(0, x.ID.IndexOf('_'))) && !x.ID.Equals(EndGame)).GetRandomElement();
                                //if (!listContractID.Exists(x => x.ID == endGame))
                                //    listExclude.Remove(MercGuildDictionary.EmpTextContractTypeID["EndGame"][0].Key);
                            }
                            else
                            {
                                Log.Info($"Attempt to Launch EndGame : {DeploymentInfo.MDStats.ProfessionalCount} ({!DeploymentInfo.MDStats.ProfessionalFail})");
                                ovr = listContractID.Find(x => x.ID == EndGame);
                                EndGame = "";
                                if (ovr == null)
                                    ovr = listContractID.Where(x => !listExclude.Contains(x.ID.Substring(0, x.ID.IndexOf('_'))) || !x.ID.Equals(EndGame)).GetRandomElement();
                            }
                            if (ovr == null)
                            {
                                Log.Info($"Clearing listExclude.Count() : {listExclude.Count()}");
                                ovr = listContractID.GetRandomElement();
                                listExclude.Clear();
                            }
                            listExclude.Add(ovr.ID.Substring(0, ovr.ID.IndexOf('_')));
                            Log.Info($"Build Contract: {ovr.ID} for {generateContract.ContractEmployer} vs {generateContract.ContractTarget}");

                            PrepareDialogue(simGame, ovr, generateContract);
                            noGenMissions = generateContract.BuildProceduralContracts(simGame, null, clearFirst, ovr);

                            //, out string shortDescriptionStart, out string employerShortDescription, out string dariusLongDescription
                            //generateContract.ContractID = GetContractDetails(listContractOvr, out shortDescriptionStart, out employerShortDescription, out dariusLongDescription);
                            //noGenMissions = generateContract.BuildProceduralContracts(simGame, null, clearFirst, true, false);

                            totalMissions = noGenMissions;
                            generateContract = new GenerateContract();
                        }
                        if (totalMissions >= Main.Settings.ActiveDeploymentContracts)
                            DeploymentInfo.DateLastRefresh = currentDate;

                        totalMissions = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static void MercDeployment_Start(SimGameState simGame, string employerFaction = "Phoenix Phire")
        {
            DeploymentInfo.ClearMDStats();
            DeploymentInfo.ClearInfo();

            DeploymentInfo.DateDeploymentEnd = simGame.CurrentDate.AddDays(Main.Settings.LengthDeploymentDays);
            DeploymentInfo.DeploymentFactionID = employerFaction;
            DeploymentInfo.IsDeployment = true;

            if (DeploymentInfo.IsGenInitContracts)
                ContractHiringHubs.UpdateTheHubs(simGame);
        }
        public static void MercDeployment_End(SimGameState simGame)
        {

            string msgText = $"Well Commander, that was a tough {Main.Settings.LengthDeploymentDays} days of fighting for sure.{Environment.NewLine}" +
                $"{InfoClass.DeploymentInfo.DeploymentFactionID} has sent us through the statistics for our deployment.{Environment.NewLine}" +
                $"And I have some further information on rewards we have gained.{Environment.NewLine}{Environment.NewLine}" +
                $"Stay tuned for further information.";
            string msgTitle = "Deployment is complete";
            
            PauseNotification.Show(msgTitle, msgText, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);
            Log.Info($"The {Main.Settings.LengthDeploymentDays} day {msgTitle} for {InfoClass.DeploymentInfo.DeploymentFactionID}");

            //DeploymentInfo.ClearMDStats();
            MercDeployment_Rewards(simGame);
            DeploymentInfo.ClearInfo();
            
            // Allow normal contract generation to appear again
            simGame.GeneratePotentialContracts(true, null);
            InfoClass.MercGuildInfo.IsGenInitContracts = true;
            ContractHiringHubs.UpdateTheHubs(simGame);
        }

        public static void MercDeployment_Cancel(SimGameState simGame, int daysRemaining)
        {
            simGame.SetReputation(GenerateContractFactions.GetFactionValueFromString(InfoClass.DeploymentInfo.DeploymentFactionID), -daysRemaining, StatCollection.StatOperation.Int_Add, null);
            simGame.SetReputation(FactionEnumeration.GetMercenaryReviewBoardFactionValue(), -daysRemaining*2, StatCollection.StatOperation.Int_Add, null);

            string msgText = $"This is not a good look Commander after {Main.Settings.LengthDeploymentDays-daysRemaining} days of fighting.{Environment.NewLine}{Environment.NewLine}" +
                $"Due to breaking the Deployment early:{Environment.NewLine}" +
                $" # We have taken a loss of {-daysRemaining} reputation with {InfoClass.DeploymentInfo.DeploymentFactionID}.{Environment.NewLine}" +
                $" # And also suffered a loss of {-daysRemaining * 2} with the MRB!{Environment.NewLine}{Environment.NewLine}" +
                $"It may not be a total loss though, as I'm waiting on the final report from the mission we did do.{Environment.NewLine}{Environment.NewLine}" +
                $"Stay tuned for further information.";
            string msgTitle = "Deployment Ended with Reputation Loss";

            PauseNotification.Show(msgTitle, msgText, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);
            Log.Info($"{msgTitle} : {InfoClass.DeploymentInfo.DeploymentFactionID} ({-daysRemaining}) and MRB ({-daysRemaining * 2})");

            MercDeployment_Rewards(simGame, daysRemaining);
            DeploymentInfo.ClearInfo();

            // Allow normal contract generation to appear again
            simGame.GeneratePotentialContracts(true, null);
            InfoClass.MercGuildInfo.IsGenInitContracts = true;
            ContractHiringHubs.UpdateTheHubs(simGame);
        }

        public static void MercDeployment_Rewards(SimGameState simGame, int daysRemaining = 0)
        {
            PostDeploymentStats(simGame, out int bonus, out KeyValuePair<int, int> moralityReward, out string houseView, out string rewardStats);

            // Trigger Rewards & Non-Rewards
            string msgText = "";
            string msgTitle = "";
            int repBonus = Main.Settings.DeploymentRepBonus;
            double repPenalty = 1;
            double penalty = 0;
            
            if (daysRemaining > 0)
            {
                int multiplier = (int)(daysRemaining / 10.1);
                penalty = ((multiplier * 15) + 10) / 100;
                bonus -= (int)(bonus * penalty);
                Log.Info($"Bonus after {penalty * 100}% penalty : {bonus}");
                msgText = $"The Employer has paid us our Deployment End Bonus of {bonus} C-Bills after deducting {penalty * 100}% for us ending the Deployment {daysRemaining} days early.{Environment.NewLine}{Environment.NewLine}" +
                    $"They had already paid us {DeploymentInfo.MDStats.BonusPaid} C-Bills, during the deployment.";
                msgTitle = "Bonus Payment with Penalty";
                repPenalty -= penalty;
            }
            else
            {
                msgText = $"The Employer has paid us our Deployment End Bonus of {bonus} C-Bills for our {DeploymentInfo.MDStats.Victories} victories.{Environment.NewLine}{Environment.NewLine}" +
                    $"They had already paid us {DeploymentInfo.MDStats.BonusPaid} C-Bills, during the deployment.";
                msgTitle = "Bonus Payment";
            }
            PauseNotification.Show(msgTitle, msgText, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);
            Log.Info($"{msgTitle} : {msgText}");
            simGame.AddFunds(bonus, null, true);

            List<FactionValue> tmpFactionList = FactionEnumeration.FactionList.Where(f => f.IsGreatHouse).ToList();
            tmpFactionList = tmpFactionList.Distinct().ToList();

            //List<string> sphereHouses = tmpFactionList.ConvertAll(x => x.Name);
            int repPirate = (int)(repBonus * moralityReward.Value / 100 * 2);
            int repHouse = (int)(repBonus * moralityReward.Key / 100);

            msgTitle = "Reputation Affect";
            msgText = $"Due to the ethical nature of our missions during this deployment, the other Houses see you and our unit as {houseView}.{Environment.NewLine}";
            
            if (repHouse < 0)
                msgText += $"This resulted in a loss of {repHouse} reputation with the Great Houses of: {Environment.NewLine}";
            else if (repHouse > 0)
                msgText += $"This resulted in a gain of {repHouse} reputation with the Great Houses of: {Environment.NewLine}";
            else
                msgText += $"This resulted in {repHouse} reputation gain with the Great Houses of: {Environment.NewLine}";

            int totalHouses = 0;
            foreach ( FactionValue factionValue in tmpFactionList)
            {
                if (!factionValue.Name.Equals(InfoClass.DeploymentInfo.DeploymentFactionID) && !factionValue.Name.Equals(Main.Settings.AlliedFactions[InfoClass.DeploymentInfo.DeploymentFactionID][0]))
                {
                    simGame.SetReputation(factionValue, repHouse, StatCollection.StatOperation.Int_Add, null);
                    totalHouses++;
                    msgText += $" + {factionValue.Name}{Environment.NewLine}";
                }
            }
            Log.Info($"House Reputation change : {repHouse}");
            
            simGame.SetReputation(FactionEnumeration.GetAuriganPiratesFactionValue(), repPirate, StatCollection.StatOperation.Int_Add, null);
            Log.Info($"Pirate Reputation change : {repPirate}");
            if (repPirate < 0)
                msgText += $"{Environment.NewLine}For the opposite reason this resulted in a loss of {repPirate} reputation with the Pirates who run the Black Market.{Environment.NewLine}";
            else if (repPirate > 0)
                msgText += $"{Environment.NewLine}For the opposite reason this resulted in a gain of {repPirate} reputation with the Pirates who run the Black Market.{Environment.NewLine}";
            else
                msgText += $"{Environment.NewLine}This resulted in {repPirate} reputation gain with the Pirates who run the Black Market.{Environment.NewLine}";

            if (repHouse < 0)
            {
                repBonus = (int)((totalHouses * (0 - repHouse) - repPirate) * repPenalty / 2);
            }
            else
            {
                repBonus = (int)(repHouse * repPenalty);
            }
            simGame.SetReputation(GenerateContractFactions.GetFactionValueFromString(InfoClass.DeploymentInfo.DeploymentFactionID), repBonus, StatCollection.StatOperation.Int_Add, null);
            simGame.SetReputation(GenerateContractFactions.GetFactionValueFromString(Main.Settings.AlliedFactions[InfoClass.DeploymentInfo.DeploymentFactionID][0]), repBonus, StatCollection.StatOperation.Int_Add, null);
            Log.Info($"Employer and Ally {Main.Settings.AlliedFactions[InfoClass.DeploymentInfo.DeploymentFactionID][0]} Reputation change : {repHouse}");
            if (repPenalty == 1 || penalty == 0)
                msgText += $"{Environment.NewLine}And finally our employer {InfoClass.DeploymentInfo.DeploymentFactionID} and their closest ally {Main.Settings.AlliedFactions[InfoClass.DeploymentInfo.DeploymentFactionID][0]} has rewarded us {repBonus} reputation for our discretion.{Environment.NewLine}";
            else
                msgText += $"{Environment.NewLine}And finally our employer {InfoClass.DeploymentInfo.DeploymentFactionID} and their closest ally {Main.Settings.AlliedFactions[InfoClass.DeploymentInfo.DeploymentFactionID][0]} has rewarded us {repBonus} reputation for our discretion which includes a {penalty * 100}% penalty for ending the deployment early.{Environment.NewLine}";

            PauseNotification.Show(msgTitle, msgText, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);
            Log.Info($"{msgTitle} : {msgText}");

            DeploymentInfo.ClearMDStats();
        }

        public static void PostDeploymentStats(SimGameState simGame, out int bonus, out KeyValuePair<int, int> moralityGauge, out string houseView, out string rewardStats)
        {
            rewardStats = "";
            string deploymentStats = "";
            string contractStats = "";
            string hiddenStats = "";

            int tmpTotalMissions = DeploymentInfo.MDStats.Victories; //DeploymentInfo.MDStats.MilitaryCount + DeploymentInfo.MDStats.PoliticalCount + DeploymentInfo.MDStats.TrainingCount;
            double tmpTotalSkulls = DeploymentInfo.MDStats.VictorySkulls; //DeploymentInfo.MDStats.MilitarySkulls + DeploymentInfo.MDStats.PoliticalSkulls + DeploymentInfo.MDStats.TrainingSkulls;

            PostDeploymentPayment(tmpTotalMissions, out bonus, out int paid);
            moralityGauge = PostDeploymentEthics(out string morality);
            houseView = morality;

            // For now Log the Stats
            deploymentStats += $"=== POST DEPLOYMENT STATS ==={Environment.NewLine}";
            deploymentStats += $"Total Mission Victories : {tmpTotalMissions}{Environment.NewLine}";
            deploymentStats += $"Total Victory Skulls : {tmpTotalSkulls}{Environment.NewLine}";
            deploymentStats += $" with {InfoClass.DeploymentInfo.MDStats.Retreats} Retreats and {DeploymentInfo.MDStats.Defeats} Defeats.{Environment.NewLine}{Environment.NewLine}";
            deploymentStats += $"Destroyed Mechs (Vees) : {DeploymentInfo.MDStats.MechsDestroyed} ({DeploymentInfo.MDStats.VeesDestroyed}) [FUTURE FEATURE]{Environment.NewLine}";
            //deploymentStats += $"Bonus Payment (Paid) : €{bonus} (€{paid}){Environment.NewLine}";
            
            PauseNotification.Show("DEPLOYMENT STATS I", deploymentStats, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);

            contractStats += $"=== CONTRACT STATS ==={Environment.NewLine}";
            contractStats += $"{GetContractTypeStats()}";

            PauseNotification.Show("DEPLOYMENT STATS II", contractStats, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);

            rewardStats += $"=== EXTRA STATS ==={Environment.NewLine}";
            rewardStats += $"Training Missions (Skulls) : {DeploymentInfo.MDStats.TrainingCount} ({DeploymentInfo.MDStats.TrainingSkulls}){Environment.NewLine}";
            rewardStats += $"Military Missions (Skulls) : {DeploymentInfo.MDStats.MilitaryCount} ({DeploymentInfo.MDStats.MilitarySkulls}){Environment.NewLine}";
            rewardStats += $"Political Missions (Skulls) : {DeploymentInfo.MDStats.PoliticalCount} ({DeploymentInfo.MDStats.PoliticalSkulls}){Environment.NewLine}";
            rewardStats += $"Morality Gauge (Result) : {morality} ({DeploymentInfo.MDStats.EthicsSum}){Environment.NewLine}";

            PauseNotification.Show("DEPLOYMENT STATS III", rewardStats, simGame.GetCrewPortrait(SimGameCrew.Crew_Darius), "", true);

            hiddenStats += $"=== [HIDDEN] Stats ==={Environment.NewLine}";
            hiddenStats += $"Professional Missions (Result) : {DeploymentInfo.MDStats.ProfessionalCount} ({!DeploymentInfo.MDStats.ProfessionalFail}){Environment.NewLine}";
            hiddenStats += $"Pilots Training Ratios : {DeploymentInfo.MDStats.PilotTraining[1]}[P1]:{DeploymentInfo.MDStats.PilotTraining[2]}[P2]:{DeploymentInfo.MDStats.PilotTraining[3]}[P3]:{DeploymentInfo.MDStats.PilotTraining[4]}[P4]:{DeploymentInfo.MDStats.PilotTraining[5]}[P5]{Environment.NewLine}";

            Log.Info($"{deploymentStats}{Environment.NewLine}");
            Log.Info($"{contractStats}{Environment.NewLine}");
            Log.Info($"{rewardStats}{Environment.NewLine}");
            Log.Info($"{hiddenStats}{Environment.NewLine}");
        }

        public static string GetContractTypeStats()
        {
            string deploymentStats = "";

            foreach (KeyValuePair<string, int> tmpStrInt in DeploymentInfo.MDStats.ContractTypeCounts)
            {
                deploymentStats += $"Total {tmpStrInt.Key} Contracts : {tmpStrInt.Value}{Environment.NewLine}";
            }

            return deploymentStats;
        }

        public static void PostDeploymentPayment(int totalMissions, out int bonus, out int paid)
        {
            bonus = 0;
            paid = DeploymentInfo.MDStats.BonusPaid;

            int tmpSegmentCount = 0;

            if (totalMissions >= 20) // 20+
            {
                tmpSegmentCount = totalMissions - 19;
                bonus += tmpSegmentCount * 250000;
                bonus += 250000;
                totalMissions = totalMissions - tmpSegmentCount;
            }
            if (totalMissions >= 15) // 15-19
            {
                tmpSegmentCount = totalMissions - 14;
                bonus += tmpSegmentCount * 200000;
                bonus += 200000;
                totalMissions = totalMissions - tmpSegmentCount;
            }
            if (totalMissions >= 10) // 10-14
            {
                tmpSegmentCount = totalMissions - 9;
                bonus += tmpSegmentCount * 150000;
                bonus += 150000;
                totalMissions = totalMissions - tmpSegmentCount;
            }
            if (totalMissions >= 5) // 5-9
            {
                tmpSegmentCount = totalMissions - 4;
                bonus += tmpSegmentCount * 100000;
                bonus += 100000;
                totalMissions = totalMissions - tmpSegmentCount;
            }
            tmpSegmentCount = totalMissions; // 0-4
            bonus += tmpSegmentCount * 100000;
            bonus -= paid;
        }

        public static KeyValuePair<int, int> PostDeploymentEthics(out string morality)
        {
            morality = "Mercenary";
            int houseRepPct = 0;
            int pirateRepPct = 0;

            switch (DeploymentInfo.MDStats.EthicsSum)
            {
                case 10:
                    houseRepPct = 100;
                    pirateRepPct = -100;
                    morality = "Righteous";
                    break;
                case 9:
                    houseRepPct = 100;
                    pirateRepPct = -80;
                    morality = "Pure";
                    break;
                case 8:
                    houseRepPct = 100;
                    pirateRepPct = -60;
                    morality = "Pure";
                    break;
                case 7:
                    houseRepPct = 100;
                    pirateRepPct = -40;
                    morality = "Lawful";
                    break;
                case 6:
                    houseRepPct = 100;
                    pirateRepPct = -20;
                    morality = "Lawful";
                    break;
                case 5:
                    houseRepPct = 100;
                    pirateRepPct = 0;
                    morality = "Honest";
                    break;
                case 4:
                    houseRepPct = 80;
                    pirateRepPct = 0;
                    morality = "Goodly";
                    break;
                case 3:
                    houseRepPct = 60;
                    pirateRepPct = 0;
                    morality = "Goodly";
                    break;
                case 2:
                    houseRepPct = 40;
                    pirateRepPct = 0;
                    morality = "Clean";
                    break;
                case 1:
                    houseRepPct = 20;
                    pirateRepPct = 0;
                    morality = "Clean";
                    break;
                case -10:
                    pirateRepPct = 100;
                    houseRepPct = -100;
                    morality = "Wicked";
                    break;
                case -9:
                    pirateRepPct = 100;
                    houseRepPct = -80;
                    morality = "Impure";
                    break;
                case -8:
                    pirateRepPct = 100;
                    houseRepPct = -60;
                    morality = "Impure";
                    break;
                case -7:
                    pirateRepPct = 100;
                    houseRepPct = -40;
                    morality = "Lawless";
                    break;
                case -6:
                    pirateRepPct = 100;
                    houseRepPct = -20;
                    morality = "Lawless";
                    break;
                case -5:
                    pirateRepPct = 100;
                    houseRepPct = 0;
                    morality = "Dishonest";
                    break;
                case -4:
                    pirateRepPct = 80;
                    houseRepPct = 0;
                    morality = "Dark";
                    break;
                case -3:
                    pirateRepPct = 60;
                    houseRepPct = 0;
                    morality = "Dark";
                    break;
                case -2:
                    pirateRepPct = 40;
                    houseRepPct = 0;
                    morality = "Edgy";
                    break;
                case -1:
                    pirateRepPct = 20;
                    houseRepPct = 0;
                    morality = "Edgy";
                    break;
            }

            return new KeyValuePair<int, int>(houseRepPct, pirateRepPct);
        }
}
}