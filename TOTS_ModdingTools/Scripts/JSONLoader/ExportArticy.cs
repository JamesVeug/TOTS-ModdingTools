using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Articy.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TOTS_ModdingTools;
using TotS.Utils;
using UnityEngine;

public class ExportArticy
{
    public static void Export()
    {
        ArticyDatabase database = ArticyDatabase.Instance;
        foreach (ArticyPackageDefinition aDefinition in database.mPackages)
        {
            ArticyPackage package = Resources.Load(aDefinition.PackagePath, typeof (ArticyPackage)) as ArticyPackage;

            foreach (ArticyObject mObject in package.mObjects)
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new DefaultContractResolver
                    {
                        DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    }
                };
                string json = JsonUtility.ToJson(mObject, true);
            
                string file = Path.Combine(ModdingToolsPlugin.ExportPath, "Articy", package.name, mObject.TechnicalName + ".json");
                if (Directory.Exists(Path.GetDirectoryName(file)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                }

                File.WriteAllText(file, json);
            }

        }

        
        ExportStringTables();
        
        GC.Collect(); // Because we create way too much garbage here
    }

    private static void ExportStringTables()
    {
        ArticyLocalizationPackage[] packages = Resources.LoadAll<ArticyLocalizationPackage>("");
        
        Dictionary<string, List<(string, ArticyLocalizationPackage)>> stringTables = new Dictionary<string, List<(string, ArticyLocalizationPackage)>>();
        foreach (ArticyLocalizationPackage package in packages)
        {
            string packageName = package.name;
            int indexOf = packageName.LastIndexOf("_");
            string tableName = packageName.Substring(0, indexOf);
            string languageCode = packageName.Substring(indexOf + 1, packageName.Length - indexOf - 1);
            if (!stringTables.ContainsKey(tableName))
            {
                stringTables.Add(tableName, new List<(string, ArticyLocalizationPackage)>());
            }
            
            stringTables[tableName].Add((languageCode, package));
        }
        APILogger.LogInfo($"Exporting {packages.Length} packages as {stringTables.Count} stringTables");


        foreach (KeyValuePair<string, List<(string, ArticyLocalizationPackage)>> table in stringTables)
        {
            HashSet<string> keys = new HashSet<string>();
            foreach ((string, ArticyLocalizationPackage) pair in table.Value)
            {
                keys.AddRange(pair.Item2.LocaKeys);
            }
            
            string[] sortedKeys = keys.OrderBy(a => a).ToArray();
            CSVBuilder builder = new CSVBuilder();
            foreach (string key in sortedKeys)
            {
                builder.AddValue("Key", key, 0);

                for (var i = 0; i < table.Value.Count; i++)
                {
                    var (languageCode, package) = table.Value[i];
                    int index = package.mLocaKeys.IndexOf(key);
                    if (index >= 0)
                    {
                        builder.AddValue(languageCode, package.mLocaValues[index], 1 + i);
                    }
                    else
                    {
                        builder.AddValue(languageCode, "", 1 + i);
                    }
                }
                builder.NextRow();
            }
            
            string directory = Path.Combine(ModdingToolsPlugin.ExportPath, "StringTables");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string stringTablePath = Path.Combine(directory, $"{table.Key}.csv");
            builder.SaveAsCSV(stringTablePath);
            APILogger.LogInfo("StringTable saved to: " + stringTablePath);
        }
    }
}