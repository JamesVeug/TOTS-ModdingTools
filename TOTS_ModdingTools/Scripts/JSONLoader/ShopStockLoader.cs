using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TOTS_ModdingTools;
using TinyJson;
using TotS.BuySell;
using TotS.Data;
using TotS.DateTime;
using ShopStockDatabaseEntry = TotS.Data.ShopStockDatabase.ShopStockDatabaseEntry;

public class ShopStockLoader
{
    public const string fileExtension = "_shopStock.json";
    
    public static void LoadAll(List<string> files)
    {
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            if (!file.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            ImportExportUtils.SetDebugPath(file);
            files.RemoveAt(i--);
            
            try
            {
                APILogger.LogVerbose($"Loading JSON (ShopStock) {file}");
                ShopStockData data = JSONParser.FromFilePath<ShopStockData>(file);
                if (data == null)
                {
                    APILogger.LogError($"Failed to load JSON (ShopStock) {file}");
                    continue;
                }
                APILogger.LogVerbose($"Parsed JSON {file}");

                string fullName = data.OwnerID + "_" + data.ItemName;

                bool isNewItemType = false;
                ShopStockDatabaseEntry[] models = null;
                if (ShopStockManager.TryGetShopStocks(data.OwnerID, data.ItemName, out models))
                {
                    APILogger.LogVerbose($"Found existing ShopStocks {fullName}");
                    if(models.Length != data.Items.Length)
                    {
                        APILogger.LogWarning($"Mismatched ShopStock count for {fullName}, JSON has {data.Items.Length} but game has {models.Length}. Overwriting existing entries.");
                        ShopStockDatabaseEntry[] newModels = new ShopStockDatabaseEntry[data.Items.Length];
                        Array.Copy(models, newModels, Math.Min(models.Length, newModels.Length));
                        for (int j = models.Length; j < newModels.Length; j++)
                        {
                            newModels[j] = ShopStockManager.New(data.OwnerID, data.ItemName);
                        }
                        
                        isNewItemType = true;
                        models = newModels;
                        ShopStockManager.ReplaceNPCShopData(data.OwnerID, newModels);
                    }
                }
                else
                {
                    APILogger.LogVerbose($"Creating new ShopStock {fullName}");
                    models = new ShopStockDatabaseEntry[data.Items.Length];
                    for (int j = 0; j < data.Items.Length; j++)
                    {
                        models[j] = ShopStockManager.New(data.OwnerID, data.ItemName);
                    }
                    isNewItemType = true;
                }
                
                for (var j = 0; j < models.Length; j++)
                {
                    APILogger.LogVerbose($"Applying JSON (ShopStock) {file} to ShopStockDatabase.ShopStockDatabaseEntry index {j} {fullName}");
                    ShopStockDatabaseEntry entry = models[j];
                    Apply(entry, data.Items[j], true, fullName, j, isNewItemType);
                }

                APILogger.LogVerbose($"Loaded JSON ShopStockDatabase.ShopStockDatabaseEntry {fullName}");
            }
            catch (Exception e)
            {
                APILogger.LogError($"Error loading JSON (ShopStock) {file}\n{e}");
            }
        }
    }

    public static void Apply(ShopStockDatabaseEntry models, ItemData data, bool toAspect, string modelName, int index, bool isNewItemType)
    {
        ImportExportUtils.SetID(modelName + "_" + index);

        ShopStockEntryData d = models.m_ShopStockEntryData;
        ImportExportUtils.ApplyValueNoNull(ref d.m_UnlockID, ref data.UnlockID, toAspect, "ShopStock", "UnlockID");
        ImportExportUtils.ApplyValueNoNull(ref d.m_ConditionOnlyIfNotOwned, ref data.ConditionOnlyIfNotOwned, toAspect, "ShopStock", "ConditionOnlyIfNotOwned");
        ImportExportUtils.ApplyValueNoNull(ref d.m_MinRelationshipLevel, ref data.MinRelationshipLevel, toAspect, "ShopStock", "MinRelationshipLevel");
        ImportExportUtils.ApplyValueNoNull(ref d.m_MaxRelationshipLevel, ref data.MaxRelationshipLevel, toAspect, "ShopStock", "MaxRelationshipLevel");
        ImportExportUtils.ApplyValueNoNull(ref d.m_SeasonalChances, ref data.SeasonalChances, toAspect, "ShopStock", "SeasonalChances");
        ImportExportUtils.ApplyValueNoNull(ref d.m_QualityWeightings, ref data.QualityWeightings, toAspect, "ShopStock", "QualityWeightings");
        ImportExportUtils.ApplyValueNoNull(ref d.m_MinStockCount, ref data.MinStockCount, toAspect, "ShopStock", "MinStockCount");
        ImportExportUtils.ApplyValueNoNull(ref d.m_MaxStockCount, ref data.MaxStockCount, toAspect, "ShopStock", "MaxStockCount");
        ImportExportUtils.ApplyValueNoNull(ref d.m_BaseCost, ref data.BaseCost, toAspect, "ShopStock", "BaseCost");
        ImportExportUtils.ApplyValueNoNull(ref d.m_QualityCostMultipliers, ref data.QualityCostMultipliers, toAspect, "ShopStock", "QualityCostMultipliers");
        ImportExportUtils.ApplyValueNoNull(ref d.m_SeasonalCostMultipliers, ref data.SeasonalCostMultipliers, toAspect, "ShopStock", "SeasonalCostMultipliers");
    }

    public static void ExportAll()
    {
        BuySellManager.Instance.m_StockHandler.m_ShopStockDatabase.m_Entries.GroupBy(a => a.m_OwnerID);
        
        foreach (IGrouping<string, ShopStockDatabaseEntry> entry in BuySellManager.Instance.m_StockHandler.m_ShopStockDatabase.m_Entries.GroupBy(a => a.m_OwnerID + "_" + a.m_ItemType.name))
        {
            ShopStockDatabaseEntry first = entry.First();
            string ownerID = first.m_OwnerID;
            string itemType = first.m_ItemType.name;
            APILogger.LogInfo($"Exporting {ownerID} {itemType} {entry.Count()} ShopStock.");
            
            ShopStockData data = new ShopStockData();
            data.Initialize();
            data.OwnerID = ownerID;
            data.ItemName = itemType;
            data.Items = new ItemData[entry.Count()];

            int index = 0;
            foreach (ShopStockDatabaseEntry databaseEntry in entry)
            {
                data.Items[index] = new ItemData();
                ItemData itemData = data.Items[index];
                string fullName = ownerID + "_" + itemType + "_" + index;
                Apply(databaseEntry, itemData, false, fullName, index, false);
                index++;
            }

            string file = Path.Combine(ModdingToolsPlugin.ExportPath, "ShopStock", ownerID + "_" + itemType + fileExtension);
            if (Directory.Exists(Path.GetDirectoryName(file)) == false)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file));
            }

            string json = JSONParser.ToJSON(data);
            File.WriteAllText(file, json);
        }
    }
}

[GenerateSchema("ShopStock", "Things that NPCs sell", ItemTypeLoader.fileExtension)]
public class ShopStockData : IInitializable
{
    public string OwnerID;
    public string ItemName;
    public ItemData[] Items;
    
    public void Initialize()
    {
        
    }
}

public class ItemData
{
    public string UnlockID;
    public bool ConditionOnlyIfNotOwned;
    public int MinRelationshipLevel;
    public int MaxRelationshipLevel;
    public SeasonalChanceData[] SeasonalChances;
    public QualityWeightingData[] QualityWeightings;
    public int MinStockCount;
    public int MaxStockCount;
    public float BaseCost;
    public QualityCostMultiplierData[] QualityCostMultipliers;
    public SeasonalCostMultiplierData[] SeasonalCostMultipliers;
}

public class SeasonalChanceData : JSONSerializer<SeasonalChanceData, SeasonalChance>, JSONSerializer<SeasonalChance, SeasonalChanceData>
{
    public ShireDateTimeConstants.Season Season;
    public float Chance;
    
    public SeasonalChance Convert(SeasonalChanceData from)
    {
        return new SeasonalChance(from.Season, from.Chance);
    }

    public SeasonalChanceData Convert(SeasonalChance from)
    {
        return new SeasonalChanceData()
        {
            Season = from.Season,
            Chance = from.Chance
        };
    }
}

public class QualityWeightingData : JSONSerializer<QualityWeightingData, QualityWeighting>, JSONSerializer<QualityWeighting, QualityWeightingData>
{
    public int Quality;
    public float Weighting;
    
    public QualityWeighting Convert(QualityWeightingData from)
    {
        return new QualityWeighting(from.Quality, from.Weighting);
    }

    public QualityWeightingData Convert(QualityWeighting from)
    {
        return new QualityWeightingData()
        {
            Quality = from.Quality,
            Weighting = from.Weighting
        };
    }
}

public class QualityCostMultiplierData : JSONSerializer<QualityCostMultiplierData, QualityCostMultiplier>, JSONSerializer<QualityCostMultiplier, QualityCostMultiplierData>
{
    public int Quality;
    public float Multiplier;
    
    public QualityCostMultiplier Convert(QualityCostMultiplierData from)
    {
        return new QualityCostMultiplier(from.Quality, from.Multiplier);
    }

    public QualityCostMultiplierData Convert(QualityCostMultiplier from)
    {
        return new QualityCostMultiplierData()
        {
            Quality = from.Quality,
            Multiplier = from.Multiplier
        };
    }
}

public class SeasonalCostMultiplierData : JSONSerializer<SeasonalCostMultiplierData, SeasonalCostMultiplier>, JSONSerializer<SeasonalCostMultiplier, SeasonalCostMultiplierData>
{
    public ShireDateTimeConstants.Season Season;
    public float Multiplier;
    
    public SeasonalCostMultiplier Convert(SeasonalCostMultiplierData from)
    {
        return new SeasonalCostMultiplier(from.Season, from.Multiplier);
    }

    public SeasonalCostMultiplierData Convert(SeasonalCostMultiplier from)
    {
        return new SeasonalCostMultiplierData()
        {
            Season = from.Season,
            Multiplier = from.Multiplier
        };
    }
}