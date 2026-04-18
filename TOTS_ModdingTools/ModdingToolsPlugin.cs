using System.Collections.Generic;
using System.IO;
using System.Linq;
using TOTS_ModdingTools.Helpers;
using BepInEx;
using HarmonyLib;
using TotS;
using TotS.Customisation;
using TotS.Data;
using TotS.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using LocalizationManager = TOTS_ModdingTools.Localization.LocalizationManager;

namespace TOTS_ModdingTools;

[HarmonyPatch]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
internal class ModdingToolsPlugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "TOTS.ModdingTools";
    public const string PLUGIN_NAME = "TOTS Modding Tools";
    public const string PLUGIN_VERSION = "0.2.0";
    
    public static SafeAction PostTick = new SafeAction();
    
    public static string ExportPath => Path.Combine(PluginDirectory, "Exported");
    
    public static ModdingToolsPlugin Instance;
    public static string PluginDirectory;
    public static string JSONLoaderDirectory = "";
    public static string BepInExDirectory = "";

    internal Transform PrefabContainer;

    private Harmony harmony;

    private void Awake()
    {
        APILogger.logger = Logger;
        Instance = this;
        JSONLoaderDirectory = Path.GetDirectoryName(Info.Location);
        
        int bepInExIndex = Info.Location.LastIndexOf("BepInEx");
        if (bepInExIndex > 0)
        {
            BepInExDirectory = Info.Location.Substring(0, bepInExIndex);
        }
        else
        {
            BepInExDirectory = Directory.GetParent(JSONLoaderDirectory)?.FullName ?? "";
        }
        PluginDirectory = Path.GetDirectoryName(Info.Location); 

        Configs.InitializeConfigs(Config);
        ModdedSaveManager.Initialize();
        // StylingManager.Initialize();
        StartLoadingAddressables();
        
        harmony = Harmony.CreateAndPatchAll(typeof(ModdingToolsPlugin).Assembly, PLUGIN_GUID);
        
        Logger.LogInfo($"Tales of the Shire v{Application.version}");
        Util.CompareUnityVersions("2022", "3", "38f1");

        // Stops Unity from destroying it for some reason. Same as Setting the BepInEx config HideManagerGameObject to true.
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        
        PrefabContainer = new GameObject("ATS_API_PrefabContainer").transform;
        PrefabContainer.SetParent(transform);
        PrefabContainer.gameObject.SetActive(false);
    }

    private void StartLoadingAddressables()
    {
        int counter = 3;
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<GameObject>("Assets/TotS/Prefabs/General/ItemManager.prefab");
        handle.Completed += operationHandle =>
        {
            if (!operationHandle.IsValid())
            {
                APILogger.LogError("Failed to load ItemManager via Addressables.");
            }
            else
            {
                APILogger.LogInfo("Loaded ItemManager via Addressables.");
                object operationHandleResult = operationHandle.Result;
                APILogger.LogInfo("Loaded: " + operationHandleResult);
                
                ItemManager itemManager = (operationHandleResult as GameObject).GetComponent<ItemManager>();
                
                
                ItemTypeManager.Initialize(itemManager);
                TraitManager.Initialize(itemManager);
            }

            if (--counter == 0)
            {
                LoadAllFiles();
            }
        };
        
        AsyncOperationHandle handle2 = Addressables.LoadAssetAsync<ShopStockDatabase>("Assets/TotS/Scriptable Objects/Tools/DataPipeline/ShopStockDatabase.asset");
        handle2.Completed += operationHandle =>
        {
            if (!operationHandle.IsValid())
            {
                APILogger.LogError("Failed to ShopStockManager via Addressables.");
            }
            else
            {
                APILogger.LogInfo("Loaded ShopStockManager via Addressables.");
                object operationHandleResult = operationHandle.Result;
                APILogger.LogInfo("Loaded: " + operationHandleResult);
                APILogger.LogInfo("Loaded Name: " + operationHandleResult.GetType().Name);
                
                ShopStockDatabase shopStockDatabase = (operationHandleResult as ShopStockDatabase);
                ShopStockManager.Initialize(shopStockDatabase);
            }
                
            if (--counter == 0)
            {
                LoadAllFiles();
            }
        };
        
        AsyncOperationHandle handle3 = Addressables.LoadAssetAsync<GameObject>("Assets/TotS/Prefabs/General/CharacterCustomisationManager.prefab");
        handle3.Completed += operationHandle =>
        {
            if (!operationHandle.IsValid())
            {
                APILogger.LogError("Failed to CharacterCustomisationManager via Addressables.");
            }
            else
            {
                APILogger.LogInfo("Loaded CharacterCustomisationManager via Addressables.");
                object operationHandleResult = operationHandle.Result;
                APILogger.LogInfo("Loaded: " + operationHandleResult);
                APILogger.LogInfo("Loaded Name: " + operationHandleResult.GetType().Name);
                
                CharacterCustomisationManager characterCustomisationManager = (operationHandleResult as GameObject).GetComponent<CharacterCustomisationManager>();
                CustomisationManager.Initialize(characterCustomisationManager);
            }
                
            if (--counter == 0)
            {
                LoadAllFiles();
            }
        };
    }

    private void LateUpdate()
    {
        Hotkeys.Update();
        
        ItemTypeManager.Tick();
        InventoryManager.Tick();
        CustomisationManager.Tick();
        
        // This is last
        LocalizationManager.Tick();
        
        // PostTick to set up links objects between each other since we can't guarantee they will be loaded in order.
        PostTick.Invoke();
        PostTick.ClearListeners();
    }
    
    [HarmonyPatch(typeof(BootStrap), nameof(BootStrap.StartLoaded))]
    [HarmonyPostfix]
    private static void BootstrapLoaded(BootStrap __instance)
    {
        Instance.Logger.LogInfo($"BootStrap.Loaded");
    }
        
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.InstantiateManagers))]
    [HarmonyPostfix]
    private static void GameManager_Loaded(BootStrap __instance)
    {
        Instance.Logger.LogInfo($"GameManager_Loaded");

        if (Configs.ExportGameToJSON)
        {
            ExportAllFiles();
        }
        
        Instance.Logger.LogInfo($"Done");
    }

    // private static void InsertCustomHairColor()
    // {
    //     Instance.Logger.LogInfo($"Inserting custom hair color");
    //     CustomisationColorConfig config = CharacterCustomisationManager.Instance.m_CustomisationHairColorConfig;
    //     CustomisationColor[] colors = config.m_Colors;
    //     foreach (CustomisationColor color in colors)
    //     {
    //         Color mColor = color.m_Color.m_Colors[0];
    //         Instance.Logger.LogInfo("color.name " + color.name + " " + mColor.r + " " + mColor.g + " " + mColor.b + " " + mColor.a);
    //     }
    //     
    //     foreach (CustomisationColorDependency color in config.m_ColorDependencies)
    //     {
    //         Instance.Logger.LogInfo("color.BaseSlotName " + color.BaseSlotName);
    //         Instance.Logger.LogInfo("color.m_BaseColor " + color.m_BaseColor);
    //         foreach (CustomisationColorDependency.DependentColor dependentColor in color.m_DependentColorPairs)
    //         {
    //             Instance.Logger.LogInfo("color.m_DependentColorPairs.SlotName " + dependentColor.SlotName);
    //             Instance.Logger.LogInfo("color.m_DependentColorPairs.Color.name " + dependentColor.Color.name);
    //         }
    //     }
    //
    //     // Hair
    //     CustomisationColor customisationColor = ScriptableObject.CreateInstance<CustomisationColor>();
    //     customisationColor.name = "CustomHairColor";
    //     
    //     Color[] colorsz = new[]
    //     {
    //         new Color(0.75f, 0f, 0f, 1f),
    //         new Color(0.5f, 0f, 0f, 1f),
    //     };
    //     customisationColor.m_Color = new CharacterCustomisationRebuilder.CustomisationUMAColor(colorsz);
    //     config.m_Colors = colors.ForceAdd(customisationColor).ToArray();
    //     
    //     // Dependency
    //     CustomisationColorDependency dependency = new CustomisationColorDependency();
    //     dependency.m_BaseSlotName = "Hair";
    //     dependency.m_BaseColor = customisationColor;
    //     dependency.m_DependentColorPairs = new[]
    //     {
    //         new CustomisationColorDependency.DependentColor(){SlotName = "FootHair", Color = customisationColor},
    //         new CustomisationColorDependency.DependentColor(){SlotName = "Eyebrows", Color = customisationColor},
    //     };
    //     config.m_ColorDependencies = config.m_ColorDependencies.ForceAdd(dependency).ToArray();
    // }
    //
    // private static void InsertCustomEyeColor()
    // {
    //     Instance.Logger.LogInfo($"Inserting custom eye color");
    //     CustomisationColorConfig config = CharacterCustomisationManager.Instance.m_CustomisationEyeColorConfig;
    //     CustomisationColor[] colors = config.m_Colors;
    //     foreach (CustomisationColor color in colors)
    //     {
    //         Color mColor = color.m_Color.m_Colors[0];
    //         Instance.Logger.LogInfo("color.name " + color.name + " " + mColor.r + " " + mColor.g + " " + mColor.b + " " + mColor.a);
    //     }
    //     
    //     foreach (CustomisationColorDependency color in config.m_ColorDependencies)
    //     {
    //         Instance.Logger.LogInfo("color.BaseSlotName " + color.BaseSlotName);
    //         Instance.Logger.LogInfo("color.m_BaseColor " + color.m_BaseColor);
    //         foreach (CustomisationColorDependency.DependentColor dependentColor in color.m_DependentColorPairs)
    //         {
    //             Instance.Logger.LogInfo("color.m_DependentColorPairs.SlotName " + dependentColor.SlotName);
    //             Instance.Logger.LogInfo("color.m_DependentColorPairs.Color.name " + dependentColor.Color.name);
    //         }
    //     }
    //
    //     // Hair
    //     CustomisationColor customisationColor = ScriptableObject.CreateInstance<CustomisationColor>();
    //     customisationColor.name = "CustomHairColor";
    //     
    //     Color[] colorsz = new[]
    //     {
    //         new Color(0.75f, 0f, 0f, 1f),
    //         new Color(0.5f, 0f, 0f, 1f),
    //     };
    //     customisationColor.m_Color = new CharacterCustomisationRebuilder.CustomisationUMAColor(colorsz);
    //     config.m_Colors = colors.ForceAdd(customisationColor).ToArray();
    //     
    //     // Dependency
    //     CustomisationColorDependency dependency = new CustomisationColorDependency();
    //     dependency.m_BaseSlotName = "Hair";
    //     dependency.m_BaseColor = customisationColor;
    //     dependency.m_DependentColorPairs = new[]
    //     {
    //         new CustomisationColorDependency.DependentColor(){SlotName = "FootHair", Color = customisationColor},
    //         new CustomisationColorDependency.DependentColor(){SlotName = "Eyebrows", Color = customisationColor},
    //     };
    //     config.m_ColorDependencies = config.m_ColorDependencies.ForceAdd(dependency).ToArray();
    // }
    
    private static List<string> GetAllJLDRFiles()
    {
        string exportedFolder = Path.Combine(JSONLoaderDirectory, "Exported");
        string examplesFolder = Path.Combine(JSONLoaderDirectory, "Examples");
        return Directory.GetFiles(Paths.PluginPath, "*.json", SearchOption.AllDirectories)
            .Where(a=> !a.Contains(exportedFolder) && !a.Contains(examplesFolder))
            .ToList();
    }

    public static void LoadAllFiles()
    {
        List<string> files = GetAllJLDRFiles();
        if (Configs.APILogLevel >= APILogger.APILogLevel.Verbose)
        {
            APILogger.LogInfo("Loading all JSON files:" + string.Join(", ", files.Select(Path.GetFileName)));
        }

        foreach (string file in files)
        {
            APILogger.LogInfo("File: " + file);
        }

        ItemTypeLoader.LoadAll(files);
        ShopStockLoader.LoadAll(files);
        CustomisationColorLoader.LoadAll(files);
    }

    private static void ExportAllFiles()
    {
        ImportExportUtils.SetDebugPath("");
        if (!Directory.Exists(ExportPath))
        {
            Directory.CreateDirectory(ExportPath);
        }
        
        APILogger.LogInfo($"Exporting all files to {ExportPath}... Grab a coffee... this will take a long time!");

        CustomisationColorLoader.ExportAll();
        ExportArticy.Export();
        JSONSchemaGenerator.GenerateAndExport();
        ShopStockLoader.ExportAll();
        ItemTypeLoader.ExportAll(); 
    }
}