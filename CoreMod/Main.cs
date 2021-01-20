using System;
using System.Collections;
using System.Reflection;
using Harmony;
using Newtonsoft.Json;
using Helpers;
using static VXIContractHiringHubs.MercGuildDictionary;

namespace VXIContractHiringHubs
{
    public static class Main
    {
        #region Init

        public static void Init(string modDir, string settings)
        {
            var harmony = HarmonyInstance.Create("BattleTech.VXI.ContractHiringHubs");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // read settings
            try
            {
                Settings = JsonConvert.DeserializeObject<ModSettings>(settings);
                Settings.modDirectory = modDir;
            }
            catch (Exception)
            {
                Settings = new ModSettings();
            }

            // blank the logfile
            Log.Clear();
            Log.Info("VXIContractHiringHubs DIR: " + modDir);
            PrintObjectFields(Settings, "Settings");

            try
            {
                BuildAllDictionaries();
                //PrepareDialogues(-99);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
        }
        // logs out all the settings and their values at runtime
        internal static void PrintObjectFields(object obj, string name)
        {
            Log.Debug($"[START {name}]");

            var settingsFields = typeof(ModSettings)
                .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in settingsFields)
            {
                if (field.GetValue(obj) is IEnumerable &&
                    !(field.GetValue(obj) is string))
                {
                    Log.Debug(field.Name);
                    foreach (var item in (IEnumerable)field.GetValue(obj))
                    {
                        Log.Debug("\t" + item);
                    }
                }
                else
                {
                    Log.Debug($"{field.Name,-30}: {field.GetValue(obj)}");
                }
            }

            Log.Debug($"[END {name}]");
        }

        #endregion

        internal static ModSettings Settings;
    }
    
}

