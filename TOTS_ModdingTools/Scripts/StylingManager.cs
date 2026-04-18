// using System.Collections.Generic;
// using TOTS_ModdingTools;
// using TotS.Styling;
// using UnityEngine;
// using UnityEngine.SceneManagement;
//
// public static class StylingManager
// {
//     private static bool houseLoaded = false;
//     public static List<StylingFeatureValueSwapper> swappers = new List<StylingFeatureValueSwapper>();
//     
//     public static void Initialize()
//     {
//         SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
//         SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
//     }
//
//     private static void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
//     {
//         if (arg0.name != "HouseInterior")
//         {
//             return;
//         }
//
//         houseLoaded = true;
//
//         foreach (GameObject o in arg0.GetRootGameObjects())
//         {
//             swappers.AddRange(o.GetComponentsInChildren<StylingFeatureValueSwapper>());
//         }
//         
//         APILogger.LogInfo("House Is loaded!");
//
//         if (Configs.ExportGameToJSON)
//         {
//             StylingLoader.ExportAll();
//         }
//     }
//
//     private static void SceneManagerOnsceneUnloaded(Scene arg0)
//     {
//         if (arg0.name != "HouseInterior")
//         {
//             return;
//         }
//         
//         houseLoaded = false;
//         swappers.Clear();
//         APILogger.LogInfo("House Is unloaded!");
//     }
// }