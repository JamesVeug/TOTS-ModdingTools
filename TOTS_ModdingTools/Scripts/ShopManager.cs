using System.Collections.Generic;
using System.Linq;
using TOTS_ModdingTools;
using TOTS_ModdingTools.Helpers;
using HarmonyLib;
using TotS.BuySell;
using TotS.Data;
using TotS.DateTime;
using TotS.Items;

[HarmonyPatch]
public static class ShopStockManager
{
    private static ShopStockDatabase _shopStockDatabase;
    public static void Initialize(ShopStockDatabase shopStockDatabase)
    {
        _shopStockDatabase = shopStockDatabase;
    }
    
    public static bool TryGetShopStocks(string ownerID, string itemName, out ShopStockDatabase.ShopStockDatabaseEntry[] models)
    {
        // ShopStockDatabase stockDatabase = BuySellManager.Instance.m_StockHandler.m_ShopStockDatabase;
        models = _shopStockDatabase.m_Entries.Where(a => a.m_OwnerID == ownerID && a.m_ItemType.name == itemName).ToArray();
        return models != null && models.Length > 0;
    }

    public static ShopStockDatabase.ShopStockDatabaseEntry New(string ownerID, string itemName)
    {
        ShopStockDatabase stockDatabase = _shopStockDatabase;//.Instance.m_StockHandler.m_ShopStockDatabase;

        ShopStockEntryData data = new ShopStockEntryData
        (
            "",
            false,
            1,
            10,
            new List<SeasonalChance>()
            {
                new SeasonalChance(ShireDateTimeConstants.Season.Autumn, 1),
                new SeasonalChance(ShireDateTimeConstants.Season.Spring, 1),
                new SeasonalChance(ShireDateTimeConstants.Season.Summer, 1),
                new SeasonalChance(ShireDateTimeConstants.Season.Winter, 1),
            },
            new List<QualityWeighting>()
            {
                new QualityWeighting(1, 20),
                new QualityWeighting(2, 30),
                new QualityWeighting(3, 40),
            },
            5, 
            7,
            16,
            new List<QualityCostMultiplier>()
            {
              new QualityCostMultiplier(1, 1),  
              new QualityCostMultiplier(2, 1.25f),  
              new QualityCostMultiplier(3, 1.50f),  
            },
            new List<SeasonalCostMultiplier>()
            {
                new SeasonalCostMultiplier(ShireDateTimeConstants.Season.Autumn, 1),  
                new SeasonalCostMultiplier(ShireDateTimeConstants.Season.Spring, 1),  
                new SeasonalCostMultiplier(ShireDateTimeConstants.Season.Summer, 1),  
                new SeasonalCostMultiplier(ShireDateTimeConstants.Season.Winter, 1),
            }
        );
        ShopStockDatabase.ShopStockDatabaseEntry model = new ShopStockDatabase.ShopStockDatabaseEntry
        (
            null,
            ownerID,
            data
        );
        stockDatabase.m_Entries = stockDatabase.m_Entries.ForceAdd(model);
        APILogger.LogInfo("Created new ShopStockDatabaseEntry for " + ownerID + " with item " + itemName);

        // if (BuySellManager.Instance != null && BuySellManager.Instance.m_StockHandler != null)
        // {
        //     ShopStockHandler.ShopStock shop = BuySellManager.Instance.m_StockHandler.m_ShopStocks
        //         .FirstOrDefault(a => a.ShopsDatabaseEntry.m_ShopOwnerID == ownerID);
        //     if (shop != null)
        //     {
        //         shop.m_StockDatabaseEntries = stockDatabase.m_Entries.ToList();
        //         APILogger.LogInfo("Added new ShopStockDatabaseEntry to ShopStock for " + ownerID);
        //     }
        // }

        Plugin.PostTick.AddListener(()=>GetItemType(model, itemName));
        return model;
    }

    public static void ReplaceNPCShopData(string ownerID, ShopStockDatabase.ShopStockDatabaseEntry[] models)
    {
        ShopStockDatabase stockDatabase = _shopStockDatabase;//.Instance.m_StockHandler.m_ShopStockDatabase;
        
        // Remove all shop data for this NPC
        stockDatabase.m_Entries = stockDatabase.m_Entries.Where(a=>a.m_OwnerID != ownerID).ToArray();
        
        // Add only the new models
        stockDatabase.m_Entries = stockDatabase.m_Entries.Concat(models).ToArray();
    }

    private static void GetItemType(ShopStockDatabase.ShopStockDatabaseEntry model, string itemName)
    {
        if (!ItemTypeManager.TryGetItemType(itemName, out ItemType itemType))
        {
            APILogger.LogWarning($"Could not find ItemType {itemName} for ShopStockDatabaseEntry owned by {model.m_OwnerID}");
        }
        model.m_ItemType = itemType;
    }
    
    // [HarmonyPatch(typeof(ShopStockHandler.ShopStock), nameof(ShopStockHandler.ShopStock.RefreshStock))]
    // [HarmonyPrefix]
    // public static void ShopStockHandler_Refresh_Prefix(ShopStockHandler.ShopStock __instance)
    // {
    //     APILogger.LogWarning($"Refreshing ShopStock for {__instance.ShopsDatabaseEntry.m_ShopOwnerID}");
    //     foreach (ShopStockDatabase.ShopStockDatabaseEntry databaseEntry in __instance.m_StockDatabaseEntries)
    //     {
    //         bool valid = ShopStockHandler.ShopStock.CheckShopStockEntryValid(databaseEntry);
    //         if (!valid)
    //         {
    //             APILogger.LogWarning($" - Invalid ShopStock entry for {databaseEntry.m_OwnerID} {databaseEntry.m_ItemType?.name ?? "null"}");
    //         }
    //         else
    //         {
    //             APILogger.LogWarning($" - Valid ShopStock entry for {databaseEntry.m_OwnerID} {databaseEntry.m_ItemType?.name ?? "null"}");
    //         }
    //     }
    //     
    //     
    //     APILogger.LogWarning($"Refreshing ShopStock done!");
    // }
}
