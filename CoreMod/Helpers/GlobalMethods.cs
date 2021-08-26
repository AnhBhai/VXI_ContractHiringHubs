using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace Helpers
{
    class GlobalMethods
    {
        internal static bool TryLoadAssembly(string modFolder, string nameDLL, string assemblyClass, out dynamic dynamicInstance)
        {
            string assemblyFullName = Path.Combine(Directory.GetParent($"{ VXIContractHiringHubs.Main.Settings.modDirectory}").FullName + modFolder, nameDLL);

            if (File.Exists(assemblyFullName))
            {
                var RealAssembly = Assembly.LoadFrom(assemblyFullName);
                var theClass = RealAssembly.GetType(assemblyClass);
                dynamicInstance = Activator.CreateInstance(theClass);

                return true;
            }
            else
            {
                dynamicInstance = new GlobalMethods();
                
                return false;
            }
        }

        internal static bool ContainsKeyValue(Dictionary<string, string> dictionary, string expectedKey, string expectedValue)
        {
            string actualValue;
            if (!dictionary.TryGetValue(expectedKey, out actualValue))
            {
                return false;
            }
            return actualValue == expectedValue;
        }

        //internal static bool ContainsKeyValue( dictionary, string expectedKey, string expectedValue)
        //{
        //    KeyValuePair<string, Dictionary<string, List<string>>> actualValue;
        //    if (!dictionary.TryGetValue(expectedKey, out actualValue))
        //    {
        //        return false;
        //    }
            
        //    return actualValue.Key == expectedValue;
        //}

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        /// <returns>The field value from the object.</returns>
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        internal static object GetMethodToInvoke(Type type, object instance, string methodName, params object [] parameters)
        {
            var func = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            
            return func.Invoke(instance, parameters);
        }

        internal static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }
    }
}
