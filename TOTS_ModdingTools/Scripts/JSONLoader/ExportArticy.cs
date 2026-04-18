using System.IO;
using System.Reflection;
using Articy.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TOTS_ModdingTools;
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
            
                string file = Path.Combine(Plugin.ExportPath, "Articy", package.name, mObject.TechnicalName + ".json");
                if (Directory.Exists(Path.GetDirectoryName(file)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                }

                File.WriteAllText(file, json);
            }

        }
    }
}