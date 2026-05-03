using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Articy.Tots;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TOTS_ModdingTools;
using TotS.Utils;
using UnityEngine;
using Path = System.IO.Path;

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


        foreach (var pair in stringTables)
        {
            List<(string, ArticyLocalizationPackage)> tuples = pair.Value;
            tuples.Sort((a,b)=>a.Item1.CompareTo(b.Item1));
            
            int englishIndex = tuples.FindIndex(a=>a.Item1 == "en");
            if (englishIndex > 0)
            {
                var cache = tuples[englishIndex];
                tuples.RemoveAt(englishIndex);
                tuples.Insert(0, cache);
            }
        }



        KeySorter sorter = new KeySorter();
        foreach ( var(tableName, table) in stringTables)
        {
            HashSet<string> keys = new HashSet<string>();
            foreach ((string, ArticyLocalizationPackage) pair in table)
            {
                keys.AddRange(pair.Item2.LocaKeys);
            }
            
            List<string> sortedKeys = keys.ToList();
            sortedKeys.Sort((a,b)=>a.CompareTo(b));
            
            CSVBuilder builder = new CSVBuilder();
            foreach (string key in sortedKeys)
            {
                builder.AddValue("Key", key, 0);
                if (sorter.GetArticyObjectFromKey(key, out ArticyObject articyObject))
                {
                    sorter.GetQuestNameAndSpeaker(articyObject, out string questName, out string speaker);
                    builder.AddValue("Quest", questName, 1);
                    builder.AddValue("Speaker", speaker, 2);
                }
                else
                {
                    builder.AddValue("Quest", "", 1);
                    builder.AddValue("Speaker", "", 2);
                    
                }
                

                for (var i = 0; i < table.Count; i++)
                {
                    var (languageCode, package) = table[i];
                    int index = package.mLocaKeys.IndexOf(key);
                    if (index >= 0)
                    {
                        builder.AddValue(languageCode, package.mLocaValues[index], 3 + i);
                    }
                    else
                    {
                        builder.AddValue(languageCode, "", 3 + i);
                    }
                }
                builder.NextRow();
            }
            
            string directory = Path.Combine(ModdingToolsPlugin.ExportPath, "StringTables");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string stringTablePath = Path.Combine(directory, $"{tableName}.csv");
            builder.SaveAsCSV(stringTablePath);
            APILogger.LogInfo("StringTable saved to: " + stringTablePath);
        }

    }
    
    /// <summary>
    /// Works but not perfect
    /// Sorting might not be deterministic enough and need sorting by parent properly or something like that.
    /// Too tired rn to get it working
    /// </summary>
    private class KeySorter
    {
        private Dictionary<string, ArticyObject> _localizableObjects = new Dictionary<string, ArticyObject>();
        public KeySorter()
        {
            _localizableObjects = new Dictionary<string, ArticyObject>();

            Add(ArticyDatabase.Instance.mLiveObjects);
            void Add(List<ArticyObject> objects)
            {
                APILogger.LogInfo("add " + objects.Count); // <--- #ForFutureJames: This is 0. So that's why everything breaks
                foreach (ArticyObject articyObject in objects)
                {
                    if (articyObject is IObjectWithLocalizableText localizableText)
                    {
                        _localizableObjects[localizableText.LocaKey_Text] = articyObject;
                    }

                    // Add(articyObject.children);
                }
            }

            APILogger.LogInfo("_localizableObjects has " + _localizableObjects.Count); // <--- #ForFutureJames: This is 0. So that's why everything breaks
        }

        public bool GetArticyObjectFromKey(string key, out ArticyObject articyObject)
        {
            return _localizableObjects.TryGetValue(key, out articyObject);
        }
        
        public int Compare(string localeKeyA, string localeKeyB)
        {
            if (!GetArticyObjectFromKey(localeKeyA, out ArticyObject objA) || !GetArticyObjectFromKey(localeKeyB, out ArticyObject objB))
            {
                return string.Compare(localeKeyA, localeKeyB, StringComparison.Ordinal);
            }
            
            if (objA.ParentId == objB.ParentId)
            {
                GetChildIndexes(objA, objB, out int indexA, out int indexB);
                return indexA.CompareTo(indexB);
            }
            
            return objA.ParentId.CompareTo(objB.ParentId);
        }

        private bool HasChild(ArticyObject objA, ArticyObject objB)
        {
            if (objA.children.Contains(objB))
            {
                return true;
            }

            foreach (ArticyObject child in objA.children)
            {
                if (HasChild(child, objB))
                {
                    return true;
                }
            }
            
            return false;
        }

        private void GetChildIndexes(ArticyObject objA, ArticyObject objB, out int aIndex, out int bIndex)
        {
            aIndex = objA.Parent.children.IndexOf(objA);
            bIndex = objB.Parent.children.IndexOf(objB);
        }
        
        private void GetChildIndexesDeprecated(ArticyObject objA, ArticyObject objB, out int aIndex, out int bIndex)
        {
            aIndex = -1;
            bIndex = -1;
            
            ulong a = objA.id;
            ulong b = objB.id;
            
            ArticyObject aParent = objA.Parent;
            IInputPinsOwner pinsOwner = (IInputPinsOwner)aParent;
            foreach (IInputPin inputPin in pinsOwner.GetInputPins())
            {
                foreach (IOutgoingConnection connection in inputPin.GetOutgoingConnections())
                {
                    int depth = 0;
                    if (connection.Target.id == a)
                    {
                        aIndex = depth;
                    }
                    if (connection.Target.id == b)
                    {
                        bIndex = depth;
                    }

                    IOutputPinsOwner connectionTarget = (IOutputPinsOwner)connection.Target;
                    foreach (IOutputPin pin in connectionTarget.GetOutputPins())
                    {
                        foreach (IOutgoingConnection outgoingConnection in pin.GetOutgoingConnections())
                        {
                            if (outgoingConnection.TargetPin == a)
                            {
                                
                            }
                            
                        }
                    }
                }
            }
        }

        public void GetQuestNameAndSpeaker(ArticyObject articyObject, out string questName, out string speakerName)
        {
            questName = "";
            speakerName = "";
            if (articyObject is IObjectWithSpeaker iSpeaker)
            {
                if (iSpeaker.Speaker != null && iSpeaker.Speaker is IObjectWithDisplayName objectWithDisplayName)
                {
                    speakerName = ArticyTextExtension.Resolve(objectWithDisplayName.DisplayName);
                }
            }
            else
            {
                Debug.Log("Speaker not found for object: " + articyObject.GetType().Name);
            }
            
            ArticyObject p = articyObject;
            while(p != null)
            {
                if (p is IObjectWithFeatureTaskTrackerRoot s)
                {
                    questName = s.GetFeatureTaskTrackerRoot().TaskTrackerDisplayName;
                    break;
                }
                p = p.Parent;
            }
        }
    }
}