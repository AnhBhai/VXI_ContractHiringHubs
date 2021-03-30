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
    public struct Raid
    {
        public string System;
        public string SystemOwner;
        public Dictionary<string, List<string>> Targets;

        public Raid(string system, string faction, Dictionary<string, List<string>> target)
        {
            System = system;
            SystemOwner = faction;
            Targets = target;
        }
    }
    public static class MercGuildDictionary
    {
        public static List<Raid> RaidSystems = new List<Raid>();
        public static bool IsBuildDictionary = false;

        public static void BuildMercDictionaries()
        {
            if (!IsBuildDictionary)
            {
                GetRaidListDictionary();
                Log.Info("Merc Dictionary Built");
                IsBuildDictionary = true; IsBuildDictionary = true;
            }
        }

        public static void ClearMercDictionaries()
        {
            if (IsBuildDictionary)
            {
                RaidSystems.Clear();
                Log.Info("Merc Dictionary Cleared");
                IsBuildDictionary = false;
            }
        }
        public static void GetRaidListDictionary()
        {
            string filePath = Path.Combine(Main.Settings.modDirectory, "Raids", "RaidSystems.csv");

            var reader = new StreamReader(filePath);

            string system = "";
            string owner = "";


            if (!reader.EndOfStream)
            {
                var lineFirst = reader.ReadLine();
                var valuesFirst = lineFirst.Split(',');

                system = valuesFirst[0];
                owner = valuesFirst[1];
            }

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values[0] == "" && !reader.EndOfStream)
                {
                    Dictionary<string, List<string>> targetDict = new Dictionary<string, List<string>>();

                    while (!reader.EndOfStream )
                    {
                        var secLine = reader.ReadLine();
                        var SecValues = secLine.Split(',');

                        if (SecValues[0] != "")
                        {
                            string targetFaction = values[1];
                            List<string> targetSystems = new List<string>();

                            for (int i = 2; i < SecValues.Count(); i++)
                            {
                                targetSystems.Add(SecValues[i]);
                            }

                            if (targetDict.ContainsKey(targetFaction))
                            {
                                targetDict[targetFaction].AddRange(targetSystems);
                            }
                            else
                            {
                                targetDict.Add(targetFaction, targetSystems);
                            }
                        }
                        else
                        {
                            RaidSystems.Add(new Raid(system, owner, targetDict));

                            if (!reader.EndOfStream)
                            {
                                system = SecValues[0];
                                owner = SecValues[1];
                            }
                        }
                    }
                }
            }
        }
    }
}
