using System.Collections.Generic;
using System.Linq;
using TOTS_ModdingTools;
using HarmonyLib;
using TOTS_ModdingTools.Helpers;
using TotS;
using TotS.Customisation;
using UnityEngine;

[HarmonyPatch]
public static class CustomisationManager
{
    public static NewColorConfig Hair = new NewColorConfig("Hair");
    public static NewColorConfig Eyes = new NewColorConfig("Eyes");
    // public static NewColorConfig Skin = new NewColorConfig("Skin"); // Requires UMATextRecipe support via a manager
    
    internal static Dictionary<string, CustomisationColor> s_lookupAllColorByName = new Dictionary<string, CustomisationColor>();
    internal static Dictionary<string, CustomisationColorDependency> s_lookupAllDependenciesByName = new Dictionary<string, CustomisationColorDependency>();
    
    private static CharacterCustomisationManager manager;
    private static bool s_initialized = false;
    internal static bool s_dirty = false;

    public static void Initialize(CharacterCustomisationManager customisationManager)
    {
        manager = customisationManager;
        
        Hair.Initialize(manager.m_CustomisationHairColorConfig);
        Eyes.Initialize(manager.m_CustomisationEyeColorConfig);
        // Skin.Initialize(manager.m_CustomisationSkinColorConfig);
        
        s_initialized = true;
    }
    
    public static void Tick()
    {
        if (!s_dirty || !s_initialized)
        {
            return;
        }
        
        s_dirty = false;

        Hair.Tick();
        Eyes.Tick();
        // Skin.Tick();
    }
    
    [HarmonyPatch(typeof(CharacterCustomisationRebuilder), nameof(CharacterCustomisationRebuilder.CompareEquippedColor))]
    [HarmonyPrefix]
    public static void CharacterCustomisationRebuilder_CompareEquippedColor(CharacterCustomisationRebuilder __instance, string slotName, Color primaryColor)
    {
        APILogger.LogInfo("CharacterCustomisationRebuilder_CompareEquippedColor");
        APILogger.LogInfo("slotName: " + slotName + 
                          "\nprimaryColor: " + primaryColor
                          );

        CharacterCustomisationRebuilder.CustomisableSlot slot = __instance.m_CustomisableSlots.Find((x => x.SlotName == slotName));
        if (slot == null)
        {
            APILogger.LogInfo("No slot with name: " + slotName);
        }
        else
        {
            APILogger.LogInfo(slot.EquippedColor == null ? "No EquippedColor" : "EquippedColor: " + slot.EquippedColor);
            APILogger.LogInfo(slot.EquippedColor.Colors == null ? "No Colors" : "Colors: " + slot.EquippedColor.Colors.Length);
            
            
        }
    }
}

public class NewColorConfig
{
    public CustomisationColor[] AllColors => config.m_Colors;
    public IReadOnlyList<NewColor> NewColors => s_NewColors;
    public IReadOnlyDictionary<string, NewColor> NewColorsLookup => s_NewColorsLookup;
    
    public CustomisationColorDependency[] AllDependencies => config.m_ColorDependencies;
    public IReadOnlyList<NewColorDependency> NewDependencies => s_NewDependencies;
    public IReadOnlyDictionary<string, NewColorDependency> NewDependenciesLookup => s_NewDependenciesLookup;

    // Color
    private List<NewColor> s_NewColors = new List<NewColor>();
    private List<NewColor> s_pendingNewColors = new List<NewColor>();
    private Dictionary<string, NewColor> s_NewColorsLookup = new Dictionary<string, NewColor>();
    private Dictionary<string, CustomisationColor> s_lookupColorByName = new Dictionary<string, CustomisationColor>();

    // Color Dependency
    private List<NewColorDependency> s_NewDependencies = new List<NewColorDependency>();
    private List<NewColorDependency> s_pendingNewDependencies = new List<NewColorDependency>();
    private Dictionary<string, NewColorDependency> s_NewDependenciesLookup = new Dictionary<string, NewColorDependency>();
    private Dictionary<string, CustomisationColorDependency> s_lookupDependenciesByName = new Dictionary<string, CustomisationColorDependency>();

    public readonly string ColorType;
    private CustomisationColorConfig config;

    public bool TryGetColor(string name, out CustomisationColor color)
    {
        foreach (CustomisationColor allColor in AllColors)
        {
            if (allColor.name == name)
            {
                color = allColor;
                return true;
            }
        }
        
        color = null;
        return false;
    }

    public NewColorConfig(string colorType)
    {
        ColorType = colorType;
    }

    public void Initialize(CustomisationColorConfig config)
    {
        this.config = config;
        foreach (CustomisationColor set in config.m_Colors)
        {
            s_lookupColorByName[set.name] = set;
            CustomisationManager.s_lookupAllColorByName[set.name] = set;
        }
            
        foreach (CustomisationColorDependency set in config.m_ColorDependencies)
        {
            string dependencyName = config.name + "_" + set.m_BaseSlotName;
            s_lookupDependenciesByName[dependencyName] = set;
            CustomisationManager.s_lookupAllDependenciesByName[dependencyName] = set;
        }
    }
        
    public NewColor NewColor(string modGUID, string name)
    {
        CustomisationColor CustomisationColor = ScriptableObject.CreateInstance<CustomisationColor>();
        CustomisationColor.m_Color = new (new Color[] { Color.black, Color.gray });
            
        return AddColor(modGUID, name, CustomisationColor);
    }
    
    public NewColor AddColor(string guidGUID, string name, CustomisationColor CustomisationColor)
    {
        CustomisationColor.name = string.IsNullOrEmpty(guidGUID) ? name : guidGUID + "_" + name;
        APILogger.IsFalse(s_NewColors.Any(a=>a.color.name == CustomisationColor.name), $"Adding CustomisationColor with name {CustomisationColor.name} that already exists!");
            
        NewColor newColor = new NewColor
        {
            modGUID = guidGUID,
            color = CustomisationColor
        };
        s_NewColors.Add(newColor);
        s_NewColorsLookup[CustomisationColor.name] = newColor;
        s_lookupColorByName[CustomisationColor.name] = CustomisationColor;
        s_lookupColorByName[CustomisationColor.name] = CustomisationColor;
            
        s_pendingNewColors.Add(newColor);
        CustomisationManager.s_dirty = true;
            
        APILogger.LogInfo($"Registered new {ColorType} Color: '{CustomisationColor.name}'");
        return newColor;
    }
        
    public NewColorDependency NewColorDependency(string modGUID, string name, SlotData slot, params SlotData[] dependencies)
    {
        if (!CustomisationManager.s_lookupAllDependenciesByName.TryGetValue(slot.slotName, out var slotCustomisationColor))
        {
            Debug.LogError($"Failed to find ColorDependency with name '{name}'");
            return null;
        }

        var dependencyColors =  new List<CustomisationColorDependency.DependentColor>();
        foreach (SlotData color in dependencies)
        {
            if(!CustomisationManager.s_lookupAllDependenciesByName.TryGetValue(color.slotName, out CustomisationColorDependency colorDependency))
            {
                Debug.LogError($"Failed to find CustomisationColor with name '{color.slotName}'");
                return null;
            }
                
            dependencyColors.Add(new CustomisationColorDependency.DependentColor()
            {
                SlotName = color.slotName,
                Color =  colorDependency.BaseColor,
            });
        }
            
        CustomisationColorDependency dependency = new CustomisationColorDependency();
        dependency.m_BaseSlotName = slot.slotName;
        dependency.m_BaseColor = slotCustomisationColor.m_BaseColor;
        dependency.m_DependentColorPairs = dependencyColors.ToArray();
            
        return AddDependency(modGUID, name, dependency);
    }
        
    public NewColorDependency AddDependency(string guidGUID, string name, CustomisationColorDependency dependency)
    {
        string dependencyName = string.IsNullOrEmpty(guidGUID) ? name : guidGUID + "_" + name;
        APILogger.IsFalse(s_NewDependencies.Any(a=>a.name == dependencyName), $"Adding {ColorType} ColorDependency with name {dependencyName} that already exists!");
            
        NewColorDependency newDependency = new NewColorDependency
        {
            name = dependencyName,
            modGUID = guidGUID,
            dependency = dependency
        };
        s_NewDependencies.Add(newDependency);
        s_NewDependenciesLookup[dependencyName] = newDependency;
        s_pendingNewDependencies.Add(newDependency);
        CustomisationManager.s_dirty = true;

            
        APILogger.LogInfo("Registered new ColorDependency: '" + dependencyName + "'");
        return newDependency;
    }

    public void Tick()
    {
        foreach (NewColor newColor in s_pendingNewColors)
        {
            CustomisationColor color = newColor.color;
            config.m_Colors = config.m_Colors.ForceAdd(color);
        }
        s_pendingNewColors.Clear();

        foreach (NewColorDependency newDependency in s_pendingNewDependencies)
        {
            CustomisationColorDependency dependency = newDependency.dependency;
            config.m_ColorDependencies = config.m_ColorDependencies.ForceAdd(dependency);
        }
        s_pendingNewDependencies.Clear();
    }
}

public class NewColor
{
    public string modGUID;
    public CustomisationColor color;
}

public class NewColorDependency
{
    public string name;
    public string modGUID;
    public CustomisationColorDependency dependency;
}

public class SlotData
{
    public string slotName = "Hair";
    public string colorName = "Test";
}