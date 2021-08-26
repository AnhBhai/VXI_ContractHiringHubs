using System;
using System.Collections;
using System.Reflection;
using Harmony;
using Newtonsoft.Json;
using Helpers;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.IO;

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
            Log.Info($"VXIContractHiringHubs {Settings.Version} DIR: {modDir}");
            PrintObjectFields(Settings, "Settings");
            
            try
            {
                //if (GlobalMethods.TryLoadAssembly("/FullXotlTables/", "FullXotlTables.dll", "FullXotlTables.Core", out XOTL_Settings))
                //{
                //    XOTL_Loaded = true;
                //    Type known = ((ObjectHandle)XOTL_Settings).Unwrap().GetType();
                //    Dictionary<string, string> unitToFactionCollection = GlobalMethods.GetInstanceField(known, XOTL_Settings, "Settings.UnitToFactionCollection") as Dictionary<string, string>;
                //    Dictionary<string, string> unitToFactionVeeCollection = GlobalMethods.GetInstanceField(known, XOTL_Settings, "Settings.UnitToFactionVeeCollection") as Dictionary<string, string>;


                //    if (Settings.FullXotlTables_UnitToFactionCollection.Count > 0)
                //    {
                //        foreach (KeyValuePair<string, string> keyValue in Settings.FullXotlTables_UnitToFactionCollection)
                //        {
                //            if (!unitToFactionCollection.ContainsKey(keyValue.Key))
                //            {
                //                unitToFactionCollection.Add(keyValue.Key, keyValue.Value);
                //            }
                //        }
                //    }

                //    if (Settings.FullXotlTables_UnitToFactionVeeCollection.Count > 0)
                //    {
                //        foreach (KeyValuePair<string, string> keyValue in Settings.FullXotlTables_UnitToFactionVeeCollection)
                //        {
                //            if (!unitToFactionVeeCollection.ContainsKey(keyValue.Key))
                //            {
                //                unitToFactionVeeCollection.Add(keyValue.Key, keyValue.Value);
                //            }
                //        }
                //    }
                //}

                MercDeployDictionary.BuildDeployDictionaries();
                MercGuildDictionary.BuildMercDictionaries();
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
        //internal static bool XOTL_Loaded = false;
        //internal static dynamic XOTL_Settings;
    }
    
}

