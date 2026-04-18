// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using TOTS_ModdingTools;
// using TinyJson;
// using TotS.BuySell;
// using TotS.Data;
// using TotS.DateTime;
// using TotS.Streaming;
// using TotS.Styling;
// using UnityEngine;
// using ShopStockDatabaseEntry = TotS.Data.ShopStockDatabase.ShopStockDatabaseEntry;
//
// public partial class StylingLoader
// {
//     public const string fileExtension = "_styling.json";
//     
//     public static void LoadAll(List<string> files)
//     {
//         // for (int i = 0; i < files.Count; i++)
//         // {
//         //     string file = files[i];
//         //     if (!file.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
//         //     {
//         //         continue;
//         //     }
//         //     
//         //     ImportExportUtils.SetDebugPath(file);
//         //     files.RemoveAt(i--);
//         //     
//         //     try
//         //     {
//         //         APILogger.LogVerbose($"Loading JSON (Styling) {file}");
//         //         ShopStockData data = JSONParser.FromFilePath<ShopStockData>(file);
//         //         if (data == null)
//         //         {
//         //             APILogger.LogError($"Failed to load JSON (Styling) {file}");
//         //             continue;
//         //         }
//         //         APILogger.LogVerbose($"Parsed JSON {file}");
//         //
//         //         string fullName = data.OwnerID + "_" + data.ItemName;
//         //
//         //         bool isNewItemType = false;
//         //         ShopStockDatabaseEntry[] models = null;
//         //         if (ShopStockManager.TryGetShopStocks(data.OwnerID, data.ItemName, out models))
//         //         {
//         //             APILogger.LogVerbose($"Found existing ShopStocks {fullName}");
//         //             if(models.Length != data.Items.Length)
//         //             {
//         //                 APILogger.LogWarning($"Mismatched Styling count for {fullName}, JSON has {data.Items.Length} but game has {models.Length}. Overwriting existing entries.");
//         //                 ShopStockDatabaseEntry[] newModels = new ShopStockDatabaseEntry[data.Items.Length];
//         //                 Array.Copy(models, newModels, Math.Min(models.Length, newModels.Length));
//         //                 for (int j = models.Length; j < newModels.Length; j++)
//         //                 {
//         //                     newModels[j] = ShopStockManager.New(data.OwnerID, data.ItemName);
//         //                 }
//         //                 
//         //                 isNewItemType = true;
//         //                 models = newModels;
//         //                 ShopStockManager.ReplaceNPCShopData(data.OwnerID, newModels);
//         //             }
//         //         }
//         //         else
//         //         {
//         //             APILogger.LogVerbose($"Creating new Styling {fullName}");
//         //             models = new ShopStockDatabaseEntry[data.Items.Length];
//         //             for (int j = 0; j < data.Items.Length; j++)
//         //             {
//         //                 models[j] = ShopStockManager.New(data.OwnerID, data.ItemName);
//         //             }
//         //             isNewItemType = true;
//         //         }
//         //         
//         //         for (var j = 0; j < models.Length; j++)
//         //         {
//         //             APILogger.LogVerbose($"Applying JSON (Styling) {file} to ShopStockDatabase.ShopStockDatabaseEntry index {j} {fullName}");
//         //             ShopStockDatabaseEntry entry = models[j];
//         //             Apply(entry, data.Items[j], true, fullName, j, isNewItemType);
//         //         }
//         //
//         //         APILogger.LogVerbose($"Loaded JSON ShopStockDatabase.ShopStockDatabaseEntry {fullName}");
//         //     }
//         //     catch (Exception e)
//         //     {
//         //         APILogger.LogError($"Error loading JSON (Styling) {file}\n{e}");
//         //     }
//         // }
//     }
//
//     public static void Apply(ShopStockDatabaseEntry models, ItemData data, bool toAspect, string modelName, int index, bool isNewItemType)
//     {
//         ImportExportUtils.SetID(modelName + "_" + index);
//
//         ShopStockEntryData d = models.m_ShopStockEntryData;
//         ImportExportUtils.ApplyValueNoNull(ref d.m_UnlockID, ref data.UnlockID, toAspect, "Styling", "UnlockID");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_ConditionOnlyIfNotOwned, ref data.ConditionOnlyIfNotOwned, toAspect, "Styling", "ConditionOnlyIfNotOwned");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_MinRelationshipLevel, ref data.MinRelationshipLevel, toAspect, "Styling", "MinRelationshipLevel");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_MaxRelationshipLevel, ref data.MaxRelationshipLevel, toAspect, "Styling", "MaxRelationshipLevel");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_SeasonalChances, ref data.SeasonalChances, toAspect, "Styling", "SeasonalChances");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_QualityWeightings, ref data.QualityWeightings, toAspect, "Styling", "QualityWeightings");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_MinStockCount, ref data.MinStockCount, toAspect, "Styling", "MinStockCount");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_MaxStockCount, ref data.MaxStockCount, toAspect, "Styling", "MaxStockCount");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_BaseCost, ref data.BaseCost, toAspect, "Styling", "BaseCost");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_QualityCostMultipliers, ref data.QualityCostMultipliers, toAspect, "Styling", "QualityCostMultipliers");
//         ImportExportUtils.ApplyValueNoNull(ref d.m_SeasonalCostMultipliers, ref data.SeasonalCostMultipliers, toAspect, "Styling", "SeasonalCostMultipliers");
//     }
//
//     public static void ExportAll()
//     {
//         // NOTE: 1 file per color style. Key them by static id-category
//         for (var i = 0; i < StylingManager.swappers.Count; i++)
//         {
//             StylingFeatureValueSwapper swapper = StylingManager.swappers[i];
//             foreach (StylingFeatureValueSwapper.FeatureValueData valueData in swapper.m_StylingFeatures)
//             {
//                 foreach (ColourStyleValues style in valueData.ColourStyles)
//                 {
//                     Texture texture = style.StyleMaterial.mainTexture;
//                     data.StyleCategory = valueData.StyleCategory;
//                     data.ColorStyleCategory = style.ColorStyleCategory;
//                     data.Material = style.StyleMaterial.name;
//                     Texture texture = style.StyleMaterial.mainTexture;
//                 }
//                 
//                 StylingFeatureValueSwapperData data = new StylingFeatureValueSwapperData();
//                 data.Initialize();
//                 data.StaticID = swapper.GetComponent<StaticStreamedItem>().StaticID;
//                 data.Name = swapper.gameObject.name;
//                 data.Items = new ItemData[entry.Count()];
//                 
//
//                 string file = Path.Combine(Plugin.ExportPath, "Styling", ownerID + "_" + itemType + fileExtension);
//                 if (Directory.Exists(Path.GetDirectoryName(file)) == false)
//                 {
//                     Directory.CreateDirectory(Path.GetDirectoryName(file));
//                 }
//
//                 string json = JSONParser.ToJSON(data);
//                 File.WriteAllText(file, json);
//             }
//             ShopStockDatabaseEntry first = entry.First();
//             string ownerID = first.m_OwnerID;
//             string itemType = first.m_ItemType.name;
//             APILogger.LogInfo($"Exporting {ownerID} {itemType} {entry.Count()} Styling.");
//
//     }
// }
//
// [GenerateSchema("Styling", "Styling", ItemTypesLoader.fileExtension)]
// public class StylingFeatureValueSwapperData : IInitializable
// {
//     public string Name;
//     public int StaticID;
//     public string Category;
//     
//     public string StyleCategory;
//     public string ColorStyleCategory;
//     public string Material;
//     
//     public void Initialize()
//     {
//         
//     }
// }