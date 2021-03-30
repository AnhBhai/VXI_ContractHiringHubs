using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BattleTech;
using Helpers;
using static Helpers.InfoClass;

namespace VXIContractHiringHubs
{
    public struct ContractListDetails
    {
        public KeyValuePair<string, string> ContractType_Name;
        public KeyValuePair<string, string> Category_Type;
        public string Who;
        public string What;
        public string Why;
        public string How;
        public string Ethics;
        public string EmployerFinalWords;

        public ContractListDetails(KeyValuePair<string, string> contractType_Name, KeyValuePair<string, string> category_Type, string who = "Random", string what = "Random", string why = "Random", string how = "Random", string ethics = "Neutral", string employerFinalWords = "")
        {
            ContractType_Name = contractType_Name;
            Category_Type = category_Type;
            Who = who;
            What = what;
            Why = why;
            How = how;
            Ethics = ethics;
            EmployerFinalWords = employerFinalWords;
    }
    }
    public static class MercDeployDictionary
    {
        public static Dictionary<string, ContractListDetails> ContractListing = new Dictionary<string, ContractListDetails>();

        public static Dictionary<string, KeyValuePair<string, string>> FlavourTextByTypeCategory = new Dictionary<string, KeyValuePair<string, string>>();
        
        public static Dictionary<string, List<string>> TextByContractTypeID = new Dictionary<string, List<string>>();

        //public static Dictionary<string, List<string>> EmpStartingTextFlavour = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<KeyValuePair<string, string>>> EmpTextContractTypeID = new Dictionary<string, List<KeyValuePair<string, string>>>();
        public static Dictionary<string, List<KeyValuePair<string, string>>> DariusText = new Dictionary<string, List<KeyValuePair<string,string>>>();

        public const int ATTACK = 0;
        public const int STEAL = 0;
        public const int SHUTDOWN = 0;
        public const int DEFEND = 1;
        public const int RESCUE = 1;
        public const int DISRUPT = 1;

        public static bool IsBuildDictionary = false;

        public static List<string> ListFlavourTextByTypeCategory(List<string> initialList, string type, string category)
        {
            List<string> listFlavourText = new List<string>();

            
            foreach (string initial in initialList)
            {
                if (GetFlavourTextTypeCategory(initial, false) == type && GetFlavourTextTypeCategory(initial, true) == category)
                    listFlavourText.Add(initial);
            }
            if (listFlavourText.Count < 1)
            {
                if (type == "Who")
                    listFlavourText.Add("Undisclosed group");
                else if (type == "What")
                    listFlavourText.Add("Undisclosed items");
                else if (type == "Why")
                    listFlavourText.Add("Undisclosed");
                else if (type == "How")
                    listFlavourText.Add("Undisclosed goals");
                else
                    listFlavourText.Add("ERROR");
            }

            return listFlavourText;
        }

        public static string GetFlavourTextTypeCategory(string flavourText, bool returnKey = false)
        {
            KeyValuePair<string, string> tmpPair = new KeyValuePair<string, string>("ERROR","ERROR");

            if (FlavourTextByTypeCategory.ContainsKey(flavourText))
                tmpPair = FlavourTextByTypeCategory[flavourText];
            else
                Log.Info($"The FlavourText {flavourText} does not exist in FlavourTextByTypeCategory");

            if (returnKey)
                return tmpPair.Key;
            else
                return tmpPair.Value;
        }

        public static void ReplaceTextDialogue(string sWho, string sWhat, string sWhy, string sHow, string employerShortDesc, string dariusLongDesc, out string shortDescription, out string longDescription, string targetFactionID, string employerFactionID)
        {
            employerShortDesc = employerShortDesc.Replace("{Comma}", ",");
            dariusLongDesc = dariusLongDesc.Replace("{Comma}", ",");

            employerShortDesc = employerShortDesc.Replace("{Who}", sWho);
            dariusLongDesc = dariusLongDesc.Replace("{Who}", sWho);

            employerShortDesc = employerShortDesc.Replace("{What}", sWhat);
            dariusLongDesc = dariusLongDesc.Replace("{What}", sWhat);

            employerShortDesc = employerShortDesc.Replace("{Why}", sWhy);
            dariusLongDesc = dariusLongDesc.Replace("{Why}", sWhy);

            employerShortDesc = employerShortDesc.Replace("{How}", sHow);
            dariusLongDesc = dariusLongDesc.Replace("{How}", sHow);

            employerShortDesc = employerShortDesc.Replace("{TargetFaction}", targetFactionID);
            dariusLongDesc = dariusLongDesc.Replace("{TargetFaction}", targetFactionID);
            employerShortDesc = employerShortDesc.Replace("{EmployerFaction}", employerFactionID);
            dariusLongDesc = dariusLongDesc.Replace("{EmployerFaction}", employerFactionID);

            shortDescription = employerShortDesc;
            longDescription = dariusLongDesc;
        }

        public static void PrintDialogues(string employerShortDesc, string dariusLongDesc, string TargetFaction, string EmployerFaction, string contractName, string sCategory, string sEthics = "Unknown", string sAttDef = "Unknown")
        {
            string partDialogue = "";
            int iTmp = 101;
            bool bTmp = true;
            
            Log.Info("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Log.Info($"+++ Employer : {EmployerFaction}");
            Log.Info($"+++ Contract : {contractName}");
            Log.Info($"+++ Target   : {TargetFaction}");
            Log.Info($"+++ Biome    : Urban");
            Log.Info("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Log.Info("+++  Employer Message  +++");
            Log.Info($"+++ SubType : {sAttDef.ToString()}   +++");
            Log.Info("++++++++++++++++++++++++++");
            for (int i = 0; i < employerShortDesc.Length - 1; i += iTmp)
            {
                iTmp = 101;
                if (employerShortDesc.Length < i + iTmp)
                    bTmp = false;

                if (bTmp)
                {
                    partDialogue = employerShortDesc.Substring(i, iTmp - 1);

                    iTmp = partDialogue.LastIndexOf(' ');
                    partDialogue = partDialogue.Substring(0, iTmp).TrimStart(' ');
                }
                else
                {
                    partDialogue = employerShortDesc.Substring(i).TrimStart(' ');
                    iTmp = 101;
                }

                Log.Info($"+++ {partDialogue}");
            }
            bTmp = true;
            Log.Info("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Log.Info("+++   Darius Message   +++");
            Log.Info($"+++ Category : {sCategory} ++");
            Log.Info($"+++ Ethics   : {sEthics}     +++");
            Log.Info("++++++++++++++++++++++++++");
            for (int i = 0; i < dariusLongDesc.Length - 1; i += iTmp)
            {
                iTmp = 101;
                if (dariusLongDesc.Length < i + iTmp)
                    bTmp = false;

                if (bTmp)
                {
                    partDialogue = dariusLongDesc.Substring(i, iTmp - 1);

                    iTmp = partDialogue.LastIndexOf(' ');
                    partDialogue = partDialogue.Substring(0, iTmp).TrimStart(' ');
                }
                else
                {
                    partDialogue = dariusLongDesc.Substring(i).TrimStart(' ');
                    iTmp = 101;
                }

                Log.Info($"+++ {partDialogue}");
            }
            Log.Info("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
        }

        public static void PrepareDialogues(int howMany = -99)
        {
            if (howMany == -99) // Print All TextByContractTypeID
            {
                string sWho = "";
                string sWhat = "";
                string sWhy = "";
                string sHow = "";

                string employerText = "";
                string dariusText = "";

                string sCategory = "Political";
                string sEthics = "Neutral";
                string sAttDef = "Defend";
                
                string shortDescription = "";
                string longDescription = "";

                List<string> tmpList = new List<string>();

                foreach (string contractTypeID in TextByContractTypeID.Keys)
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        if (sCategory == "Military")
                            sCategory = "Political";
                        else
                            sCategory = "Military";

                        if (sEthics == "Good")
                            sEthics = "Bad";
                        else if (sEthics == "Bad")
                            sEthics = "Neutral";
                        else
                            sEthics = "Good";

                        if (sAttDef == "Defend")
                            sAttDef = "Attack";
                        else
                            sAttDef = "Defend";

                        Log.Info("==================================================================");
                        Log.Info($"=== [{sCategory}]::[{contractTypeID}] ===");
                        
                        // Miitary
                        sWho = ListFlavourTextByTypeCategory(TextByContractTypeID[contractTypeID].ToList<string>(), "Who", sCategory).GetRandomElement<string>();
                        sWhat = ListFlavourTextByTypeCategory(TextByContractTypeID[contractTypeID].ToList<string>(), "What", sCategory).GetRandomElement<string>();
                        sWhy = ListFlavourTextByTypeCategory(TextByContractTypeID[contractTypeID].ToList<string>(), "Why", sCategory).GetRandomElement<string>();
                        sHow = ListFlavourTextByTypeCategory(TextByContractTypeID[contractTypeID].ToList<string>(), "How", sCategory).GetRandomElement<string>();

                        Log.Info($"=== [{sWho}]::[{sWhat}] ===");
                        Log.Info($"=== [{sWhy}]::[{sHow}] ===");
                        Log.Info("==================================================================");

                        // Type 1
                        //employerText = EmpStartingTextFlavour.GetRandomElement<KeyValuePair<string, List<string>>>().Value.GetRandomElement<string>();
                        //employerText += " ";
                        if (sAttDef == "Attack")
                            employerText = EmpTextContractTypeID[contractTypeID][ATTACK].Value;
                        else
                            employerText = EmpTextContractTypeID[contractTypeID][DEFEND].Value;

                        dariusText = DariusText[sCategory].Find(f => f.Key == sEthics).Value;

                        ReplaceTextDialogue(sWho, sWhat, sWhy, sHow, employerText, dariusText, out shortDescription, out longDescription, "Davion", "Kurita");
                        PrintDialogues(shortDescription, longDescription, contractTypeID, "Davion", "Kurita", sCategory, sEthics, sAttDef);
                    }
                }
            }
        }

        public static void BuildDeployDictionaries()
        {
            if (!IsBuildDictionary)
            {
                GetContractListingDictionary();
                GetTextFlavourDictionary();
                GetDialogueDarius();
                GetDialogueEmployer();
                GetTextByContractTypeID();
                Log.Info("Deploy Dictionaries Built");
                IsBuildDictionary = true;
            }
        }

        public static void ClearDeployDictionaries()
        {
            if (IsBuildDictionary)
            {
                ContractListing.Clear();
                FlavourTextByTypeCategory.Clear();
                DariusText.Clear();
                EmpTextContractTypeID.Clear();
                TextByContractTypeID.Clear();
                Log.Info("Deploy Dictionaries Cleared");
                IsBuildDictionary = false;
            }
        }
        public static void GetContractListingDictionary()
        {
            string filePath = Path.Combine(Main.Settings.modDirectory, "ContractBuilding", "ContractList.csv");

            var reader = new StreamReader(filePath);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0] != "")
                {
                    if (values.Count<string>() >= 10)
                    {
                        string contractID = values[0];
                        KeyValuePair<string, string> contractTypeName = new KeyValuePair<string, string>(values[1], values[2]);
                        KeyValuePair<string, string> categoryType = new KeyValuePair<string, string>(values[3], values[4]);
                        ContractListDetails contractListDetails = new ContractListDetails(contractTypeName, categoryType, values[5], values[6], values[7], values[8], values[9], values[10]);

                        if (!ContractListing.ContainsKey(contractID))
                            ContractListing.Add(contractID, contractListDetails);
                    }
                    else
                    {
                        Log.Info("Line: " + line);
                    }
                }
            }
        }
        

        public static void GetTextFlavourDictionary()
        {
            string filePath = Path.Combine(Main.Settings.modDirectory, "ContractBuilding", "TextTypeCategory.csv");

            var reader = new StreamReader(filePath);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0] != "")
                {
                    string flavourText = values[0];
                    string flavourType = values[1];
                    string flavourCategory = values[2];
                    KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(flavourCategory, flavourType);

                    FlavourTextByTypeCategory.Add(flavourText, keyValuePair);
                }

            }
        }

        public static void GetDialogueDarius()
        {
            string filePath = Path.Combine(Main.Settings.modDirectory, "ContractBuilding", "Darius_LongDescription.csv");

            var reader = new StreamReader(filePath);

            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0] != "")
                {
                    string sType = values[0];
                    string sSubType = values[1];
                    string sText = values[2];
                    List<string> tmpList = new List<string>();
                    tmpList.Add(sText);

                    KeyValuePair<string, string> tmpPair1 = new KeyValuePair<string, string>(sSubType, sText);
                        List<KeyValuePair<string, string>> tmpListPair1 = new List<KeyValuePair<string, string>>();
                    if (DariusText.ContainsKey(sType))
                    {
                        DariusText[sType].Add(tmpPair1);
                    }
                    else
                    {
                        tmpListPair1.Add(tmpPair1);
                        DariusText.Add(sType, tmpListPair1);
                    }
                }
            }
        }
        
        public static void GetDialogueEmployer()
        {
            string filePath = Path.Combine(Main.Settings.modDirectory, "ContractBuilding", "Employer_ShortDescription.csv");

            var reader = new StreamReader(filePath);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0] != "")
                {
                    string sType = values[0];
                    string sSubType = values[1];
                    string sText = values[2];

                    List<string> tmpList = new List<string>();
                    tmpList.Add(sText);

                    //if (sType == "Employer_Start")
                    //{
                    //    List<string> tmpList1 = new List<string>();
                    //    if (EmpStartingTextFlavour.ContainsKey(sSubType))
                    //    {
                    //        EmpStartingTextFlavour[sSubType].Add(sText);
                    //    }
                    //    else
                    //    {
                    //        tmpList1.Add(sText);
                    //        EmpStartingTextFlavour.Add(sSubType, tmpList1);
                    //    }
                    //}
                    //else
                    //{
                        KeyValuePair<string, string> tmpPair2 = new KeyValuePair<string, string>(sSubType, sText);
                        List<KeyValuePair<string, string>> tmpListPair2 = new List<KeyValuePair<string, string>>();
                        if (EmpTextContractTypeID.ContainsKey(sType))
                        {
                            EmpTextContractTypeID[sType].Add(tmpPair2);
                        }
                        else
                        {
                            tmpListPair2.Add(tmpPair2);
                            EmpTextContractTypeID.Add(sType, tmpListPair2);
                        }
                    //}
                }
            }
        }
                    
        public static void GetTextByContractTypeID()
        {
            string filePath = Path.Combine(Main.Settings.modDirectory, "ContractBuilding", "ContractTypeID.csv");

            var reader = new StreamReader(filePath);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0] != "")
                {
                    string contractTypeID = values[0];
                    List<string> flavourText = new List<string>();

                    for (int i = 1; i < values.Count(); i++)
                    {
                        flavourText.Add(values[i]);
                    }
                    TextByContractTypeID.Add(contractTypeID, flavourText);
                }
            }
        }
    }
}
