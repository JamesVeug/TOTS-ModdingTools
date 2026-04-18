using System.IO;
using TotS.SaveGame;
using UnityEngine;

namespace TOTS_ModdingTools;

public class ModdedSaveSystem : MonoBehaviour, ISaveGameSystem
{
    public int GetSystemVersion => 1;
    public string GetSystemKey => "ModdedSaveSystem";

    public void WriteSaveGame(SaveGameContext context, BinaryWriter writer)
    {
        writer.Write(ModdedSaveManager.saveData.Count);
        foreach (ModdedSaveData data in ModdedSaveManager.saveData)
        {
            Debug.Log("Writing save: " + data.guid + " " + data.name);
            writer.Write(data.guid);
            writer.Write(data.name);
            
            writer.Write(data.intValues.Count);
            foreach (ModdedSaveData.Field<int> value in data.intValues)
            {
                writer.Write(value.key);
                writer.Write(value.value);
            }
            
            writer.Write(data.strValues.Count);
            foreach (ModdedSaveData.Field<string> value in data.strValues)
            {
                writer.Write(value.key);
                writer.Write(value.value);
            }
        }
        Debug.Log("Done writing modded save data");
    }
    
    public void ReadSaveGame(SaveGameContext context, BinaryReader reader, int version)
    {
        switch (version)
        {
            case 1:
                ReadVersionV1(reader);
                break;
        }
        Debug.Log("Done reading modded save data");
    }
    
    private void ReadVersionV1(BinaryReader reader)
    {
        ModdedSaveManager.saveData.Clear();
        
        int totalSaves = reader.ReadInt32();
        for (int i = 0; i < totalSaves; i++)
        {
            string guid = reader.ReadString();
            string name = reader.ReadString();
            Debug.Log("Loading save: " + guid + " " + name);

            ModdedSaveData saveData = new ModdedSaveData(guid, name);
            int totalIntValues = reader.ReadInt32();
            for (int j = 0; j < totalIntValues; j++)
            {
                string key = reader.ReadString();
                int value = reader.ReadInt32();
                saveData.SetInt(key, value);
            }
            
            int totalStrValues = reader.ReadInt32();
            for (int j = 0; j < totalStrValues; j++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();
                saveData.SetString(key, value);
            }
            
            
            ModdedSaveManager.saveData.Add(saveData);
        }
    }

    public void PostWriteSaveGame(SaveGameContext context)
    {
        
    }

    public void PreReadSaveGame(SaveGameContext context)
    {
        
    }

    public void ReadEmptySaveGame(SaveGameContext context)
    {
        
    }

    public void PostReadSaveGame(SaveGameContext context)
    {
        
    }
    
    public void PreWriteSaveGame(SaveGameContext context)
    {
        
    }
}