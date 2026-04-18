using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using TotS;
using TotS.Audio;
using TotS.SaveGame;
using UnityEngine;

namespace TOTS_ModdingTools;

[HarmonyPatch]
public static class ModdedSaveManager
{
    internal static List<ModdedSaveData> saveData = new List<ModdedSaveData>();
    internal static List<ISaveGameSystem> customSaveSystems = new List<ISaveGameSystem>();

    private static ModdedSaveSystem saveSystem;
    
    public static void Initialize()
    {
        GameObject saveGameSystem = new GameObject("SaveGameSystem");
        saveGameSystem.transform.SetParent(ModdingToolsPlugin.Instance.transform);

        saveSystem = saveGameSystem.AddComponent<ModdedSaveSystem>();
    }

    public static ModdedSaveData GetOrCreate(string guid, string name)
    {
        foreach (ModdedSaveData data in saveData)
        {
            if (!data.guid.Equals(guid, StringComparison.OrdinalIgnoreCase)) continue;
            if (!data.name.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
            
            return data;
        }

        ModdedSaveData moddedSaveData = new ModdedSaveData(guid, name);
        saveData.Add(moddedSaveData);
        return moddedSaveData;
    }

    public static void AddCustomSaveSystem(ISaveGameSystem saveSystem)
    {
        customSaveSystems.Add(saveSystem);
        if (SaveGameManager.Exists)
        {
            SaveGameManager.Instance.AddSystem(saveSystem);
        }
    }

    [HarmonyPatch(typeof(AudioManager), nameof(AudioManager.EnableSFX))]
    [HarmonyPostfix]
    private static void AudioManager_EnableSFX_Postfix()
    {
        SaveGameManager.Instance.AddSystem(saveSystem);
        foreach (ISaveGameSystem system in customSaveSystems)
        {
            SaveGameManager.Instance.AddSystem(system);
        }
    }
}