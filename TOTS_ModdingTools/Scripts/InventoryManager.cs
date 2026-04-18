using TOTS_ModdingTools;
using HarmonyLib;
using TotS;
using TotS.Inventory;
using TotS.Items;

[HarmonyPatch]
public static class InventoryManager
{
    private static bool s_initialized;
    private static bool s_dirty;

    public static void Tick()
    {
        if (!s_initialized || !s_dirty)
            return;

        s_dirty = false;
        
        if (ItemTypeManager.NewItemTypes.Count > 0)
        {
            Inventory recipes = TotS.Inventory.InventoryManager.Instance.Recipes;
            foreach (NewItemType item in ItemTypeManager.NewItemTypes)
            {
                if (item.ItemType.HasAspect<RecipeAspect>())
                {
                    if (!recipes.Contains(item.ItemType))
                    {
                        APILogger.LogInfo("Adding new recipe item: " + item.ItemType.name);
                        
                        Item newItem = ItemManager.Instance.CreateItem(item.ItemType);
                        recipes.Add(newItem);
                    }
                }
            }
        }
    }

    public static void SetDirty()
    {
        s_dirty = true;
        APILogger.LogInfo("InventoryManager marked dirty");
    }

    [HarmonyPatch(typeof(TotS.Inventory.InventoryManager), nameof(TotS.Inventory.InventoryManager.EndChanges))]
    [HarmonyPrefix]
    public static void InventoryManagerAwake()
    {
        APILogger.LogInfo("Initializing inventory manager");
        s_initialized = true;
        Tick();
    }
}