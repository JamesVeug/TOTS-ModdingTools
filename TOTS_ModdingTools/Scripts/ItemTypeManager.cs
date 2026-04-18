using System.Collections.Generic;
using System.Linq;
using TOTS_ModdingTools;
using HarmonyLib;
using TotS;
using TotS.BuySell;
using TotS.Cooking;
using TotS.Data;
using TotS.Input;
using TotS.Inventory;
using TotS.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[HarmonyPatch]
public static class ItemTypeManager
{
    public static IReadOnlyList<NewItemType> NewItemTypes => s_newItemTypes;
    public static IReadOnlyDictionary<string, NewItemType> NewItemTypesLookup => s_newItemTypesLookup;

    private static List<NewItemType> s_newItemTypes = new List<NewItemType>();
    private static List<NewItemType> s_pendingNewItemTypes = new List<NewItemType>();
    private static Dictionary<string, NewItemType> s_newItemTypesLookup = new Dictionary<string, NewItemType>();
    
    private static Dictionary<string, ItemType> s_lookupItemTypeByName = new Dictionary<string, ItemType>();

    private static ItemManager itemManagerAsset;
    private static bool s_initialized = false;
    private static bool s_dirty = false;

    public static void Initialize(ItemManager itemManager)
    {
        itemManagerAsset = itemManager;
        
        bool initializedPrefabFactory = false;
        foreach (ItemTypeSet set in itemManagerAsset.m_ItemTypeSets)
        {
            foreach (ItemType itemType in set.m_ItemTypes)
            {
                s_lookupItemTypeByName[itemType.name] = itemType;

                if (!initializedPrefabFactory && itemType.TryGetAspect(out PlaceableAspect placeableAspect))
                {
                    if (placeableAspect.m_Prefab != null)
                    {
                        var highlighter = placeableAspect.m_Prefab.GetComponentInChildren<MaterialHighlighter>();
                        if (highlighter.m_Config != null)
                        {
                            PrefabFactory.MaterialHighlighterConfig = highlighter.m_Config;
                            
                            var kinetic = placeableAspect.m_Prefab.GetComponentInChildren<Kinetic>();
                            if (kinetic != null)
                            {
                                PrefabFactory.KineticConfig = kinetic.m_Config;
                                initializedPrefabFactory = true;
                            }
                        }
                    }
                }
            }
        }

        s_initialized = true;
    }
    
    public static NewItemType New(string modGUID, string name)
    {
        ItemType ItemType = ScriptableObject.CreateInstance<ItemType>();
        ItemType.m_Aspects = new List<Aspect>();
        
        // TODO: Localisation?
        return Add(modGUID, name, ItemType);
    }
    
    public static NewItemType Add(string guidGUID, string name, ItemType itemType)
    {
        itemType.name = string.IsNullOrEmpty(guidGUID) ? name : guidGUID + "_" + name;
        itemType.m_TypeID = SerialisedID.Create(itemType.name);
        APILogger.IsFalse(s_newItemTypes.Any(a=>a.ItemType.name == itemType.name), $"Adding ItemType with name {itemType.name} that already exists!");
        
        NewItemType newGood = new NewItemType
        {
            modGUID = guidGUID,
            ItemType = itemType
        };
        s_newItemTypes.Add(newGood);
        s_newItemTypesLookup[itemType.name] = newGood;
        s_pendingNewItemTypes.Add(newGood);
        s_dirty = true;

        
        APILogger.LogInfo("Registered new ItemType: '" + itemType.name + "'");
        return newGood;
    }

    public static ItemType GetItemType(string modGUID, string name)
    {
        string fullName = string.IsNullOrEmpty(modGUID) ? name : modGUID + "_" + name;
        s_lookupItemTypeByName.TryGetValue(fullName, out ItemType itemType);
        return itemType;
    }

    public static bool TryGetItemType(string fullName, out ItemType itemType)
    {
        if (s_newItemTypesLookup.TryGetValue(fullName, out NewItemType newItemType))
        {
            itemType = newItemType.ItemType;
            return true;
        }
        
        s_lookupItemTypeByName.TryGetValue(fullName, out itemType);
        return itemType != null;
    }
    
    public static void Tick()
    {
        if (!s_dirty || !s_initialized)
        {
            return;
        }
        
        s_dirty = false;

        List<ItemTypeSet> sets = itemManagerAsset.m_ItemTypeSets
            .Where(a=>a.name.ToLower().Contains("itemtypeset"))
            .ToList();
        ItemTypeSet mealSet = sets.First(a => a.name.ToLower().Contains("meal"));
        ItemTypeSet gardenableSet = sets.First(a => a.name.ToLower().Contains("gardenable"));
        ItemTypeSet mealRecipeSet = sets.First(a => a.name.ToLower().Contains("mealrecipe"));
        ItemTypeSet seedPacketSet = sets.First(a => a.name.ToLower().Contains("seedpacket"));
        ItemTypeSet ingredientSet = sets.First(a => a.name.ToLower().Contains("ingredient"));
        ItemTypeSet forageableSet = sets.First(a => a.name.ToLower().Contains("forageable"));
        // ItemTypeSet flowerSet = sets.FirstOrDefault(a => a.name.ToLower().Contains("flower"));
        foreach (NewItemType newItem in s_pendingNewItemTypes)
        {
            ItemType itemType = newItem.ItemType;
            s_lookupItemTypeByName[itemType.name] = itemType;
            if (ItemManager.Exists)
            {
                ItemManager.Instance.Register(itemType);
            }

            if (itemType.HasAspect<RecipeAspect>())
            {
                mealRecipeSet.TryAdd(itemType);
            }
            else if (itemType.HasAspect<MealAspect>())
            {
                mealSet.TryAdd(itemType);
            }
            else if (itemType.HasAspect<GardenableAspect>())
            {
                gardenableSet.TryAdd(itemType);
            }
            else if (itemType.HasAspect<CookingAspect>())
            {
                ingredientSet.TryAdd(itemType);
            }
            else if (itemType.HasAspect<ForageableAspect>())
            {
                forageableSet.TryAdd(itemType);
            }
            else if (itemType.TryGetAspect(out StorageAspect seedAspect) && seedAspect.DefaultInventory == "SeedBag")
            {
                seedPacketSet.TryAdd(itemType);
            }
        }

        s_pendingNewItemTypes.Clear();
        InventoryManager.SetDirty();
    }
    
    [HarmonyPatch(typeof(DecorateInput), nameof(DecorateInput.SwitchItemToPlaceableContext))]
    [HarmonyPrefix]
    public static void DecorateInput_SwitchItemToPlaceableContext(DecorateInput __instance, Item item, ref Placeable placeable, DecorateInput.GrabInfo grabInfo)
    {
        APILogger.LogInfo("DecorateInput.SwitchItemToPlaceableContext");
        APILogger.LogInfo($"item: {item}");
        APILogger.LogInfo($"item.ContextGameObject: {item?.ContextGameObject}");
        APILogger.LogInfo($"item.ItemType: {item?.ItemType}");
        APILogger.LogInfo($"item.ItemType.name: {item?.ItemType?.name}");
        APILogger.LogInfo($"placeable: {placeable}");
        APILogger.LogInfo($"grabInfo: {grabInfo}");
    }
    
    // [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.Awake))]
    // [HarmonyPostfix]
    // public static void ItemManagerAwake()
    // {
    //     s_initialized = true;
    //     Tick();
    // }
    
    // [HarmonyPatch(typeof(RecipeSelectUIScreen), nameof(RecipeSelectUIScreen.Init))]
    // [HarmonyPrefix]
    // public static void RecipeSelectUIScreen_Init(RecipeSelectUIScreen __instance)
    // {
    //     APILogger.LogInfo("RecipeSelectUIScreen.Init - Marking ItemManager as dirty to ensure new ItemTypes are added to sets.");
    //     APILogger.LogInfo($"displayTraits: {__instance.m_DisplayTraits}");
    //     APILogger.LogInfo($"displayTraits Count: {__instance.m_DisplayTraits.Length}");
    // }
    
    // [HarmonyPatch(typeof(RecipeSelectNavBar), nameof(RecipeSelectNavBar.Init))]
    // [HarmonyPrefix]
    // public static void RecipeSelectNavBar_Init(RecipeSelectNavBar __instance, List<Trait> traits, GameObject recipeUIPrefab, GameObject emptyUIPrefab, RecipeSelectUIScreen screen)
    // {
    //     APILogger.LogInfo("RecipeSelectNavBar.Init");
    //     APILogger.LogInfo($"traits: {traits}");
    //     APILogger.LogInfo($"traits Count: {traits.Count}");
    // }
    //
    // [HarmonyPatch(typeof(RecipeSelectNavBar), nameof(RecipeSelectNavBar.UpdateGrid))]
    // [HarmonyPrefix]
    // public static void RecipeSelectNavBar_UpdateGrid(RecipeSelectNavBar __instance)
    // {
    //     APILogger.LogInfo("RecipeSelectNavBar_UpdateGrid");
    //     APILogger.LogInfo($"m_NavIndex: {__instance.m_NavIndex}");
    //     APILogger.LogInfo($"m_DisplayTraits: {__instance.m_DisplayTraits.Count}");
    //     APILogger.LogInfo($"m_DisplayTraits[m_NavIndex]; {__instance.m_DisplayTraits[__instance.m_NavIndex]}");
    //
    //     try
    //     {
    //         APILogger.LogInfo($"getting index");
    //         var m_RecipeItemIndex = new ItemIndex(Singleton<TotS.Inventory.InventoryManager>.Instance.Recipes.Items,
    //             ItemIndex.Collate.InventoriesSeparated, null,
    //             TotS.Inventory.InventoryManager.SortAlphabeticallyAndByQuality, __instance.GetType().Name);
    //         APILogger.LogInfo($"m_RecipeItemIndex: {m_RecipeItemIndex.StackCount}");
    //     }
    //     catch (System.Exception e)
    //     {
    //         APILogger.LogError($"Error creating ItemIndex in RecipeSelectNavBar_UpdateGrid");
    //         APILogger.LogException(e);
    //     }
    // }
    
    // [HarmonyPatch(typeof(RecipeSelectNavBar), nameof(RecipeSelectNavBar.UpdateGrid))]
    // [HarmonyPostfix]
    // public static void RecipeSelectNavBar_UpdateGrid_Postfix(RecipeSelectNavBar __instance)
    // {
    //     APILogger.LogInfo("RecipeSelectNavBar_UpdateGrid_Postfix");
    //     APILogger.LogInfo($"m_NavIndex: {__instance.m_NavIndex}");
    //     APILogger.LogInfo($"m_DisplayTraits: {__instance.m_DisplayTraits.Count}");
    //     APILogger.LogInfo($"m_DisplayTraits[m_NavIndex]; {__instance.m_DisplayTraits[__instance.m_NavIndex]}");
    //     APILogger.LogInfo($"m_RecipeItemIndex: {__instance.m_RecipeItemIndex.StackCount}");
    // }
}

public class NewItemType
{
    public string modGUID;
    public ItemType ItemType;
}