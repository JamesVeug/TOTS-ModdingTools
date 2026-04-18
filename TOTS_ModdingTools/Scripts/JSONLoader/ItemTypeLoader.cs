using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TOTS_ModdingTools;
using TOTS_ModdingTools.Helpers;
using TinyJson;
using TotS;
using TotS.BuySell;
using TotS.Cooking;
using TotS.Data;
using TotS.DateTime;
using TotS.DateTime.Weather;
using TotS.Fishing;
using TotS.Items;
using TotS.Schema;
using TotS.SharedMeals;
using UnityEngine;

public class ItemTypesLoader
{
    public const string fileExtension = "_itemType.json";
    
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
                APILogger.LogVerbose($"Loading JSON (ItemTypes) {file}");
                ItemTypeData data = JSONParser.FromFilePath<ItemTypeData>(file);
                if (data == null)
                {
                    APILogger.LogError($"Failed to load JSON (ItemTypes) {file}");
                    continue;
                }
                APILogger.LogVerbose($"Parsed JSON {file}");

                string fullName = string.IsNullOrEmpty(data.Guid) ? data.Name : data.Guid + "_" + data.Name;

                bool isNewItemType = false;
                ItemType model = null;
                if (ItemTypeManager.TryGetItemType(fullName, out model))
                {
                    APILogger.LogVerbose($"Found existing ItemTypes {fullName}");
                }
                else
                {
                    APILogger.LogVerbose($"Creating new ItemTypes {fullName}");
                    model = ItemTypeManager.New(data.Guid, data.Name).ItemType;
                    isNewItemType = true;
                }
                
                APILogger.LogVerbose($"Applying JSON (ItemTypes) {file} to ItemType {fullName}");
                Apply(model, data, true, fullName, isNewItemType);

                APILogger.LogVerbose($"Loaded JSON ItemType {fullName}");
            }
            catch (Exception e)
            {
                APILogger.LogError($"Error loading JSON (ItemTypes) {file}\n{e}");
            }
        }
    }

    public static void Apply(ItemType model, ItemTypeData data, bool toAspect, string modelName, bool isNewItemType)
    {
        ImportExportUtils.SetID(modelName);
        
        string oldName = model.name;
        ImportExportUtils.ApplyLocalizedText(oldName.Replace(" ", "") + ".DisplayName", ref data.DisplayName, toAspect, "DisplayName");
        ImportExportUtils.ApplyLocalizedText(oldName.Replace(" ", "") + ".Description", ref data.Description, toAspect, "Description");

        CreateStorageAspect(model, data, toAspect);
        CreateSellableAspect(model, data, toAspect);
        CreateBuyableAspect(model, data, toAspect);
        CreateGardenableAspect(model, data, toAspect);
        CreateProduceAspect(model, data, toAspect);
        CreatePreviewAspect(model, data, toAspect);
        CreateCookingAspect(model, data, toAspect);
        CreateFishableAspect(model, data, toAspect);
        CreateFlavourAspect(model, data, toAspect);
        CreateForageableAspect(model, data, toAspect);
        CreateForageableProduceAspect(model, data, toAspect);
        CreateGrowthMediumAspect(model, data, toAspect);
        CreateMealAspect(model, data, toAspect);
        CreatePlaceableAspect(model, data, toAspect);
        CreateQualityAspect(model, data, toAspect);
        CreateRecipeAspect(model, data, toAspect);
        CreateSeedAspect(model, data, toAspect);
        CreateWateringCanAspect(model, data, toAspect);
    }

    private static void CreateStorageAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out StorageAspect aspect, ref data.StorageAspect, ref data.HasStorageAspect, toAspect))
        {
            StorageAspectData d = data.StorageAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_DefaultInventory, ref d.DefaultInventory, toAspect, "ItemTypes", "DefaultInventory");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_InfiniteStackSize, ref d.InfiniteStackSize, toAspect, "ItemTypes", "InfiniteStackSize");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_MaxStacks, ref d.MaxStacks, toAspect, "ItemTypes", "MaxStacks");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_StackSize, ref d.StackSize, toAspect, "ItemTypes", "StackSize");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_ValidInventories, ref d.ValidInventories, toAspect, "ItemTypes", "ValidInventories");
        }
    }

    private static void CreateSellableAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out SellableAspect aspect, ref data.SellableAspect, ref data.HasSellableAspect, toAspect))
        {
            SellableAspectData d = data.SellableAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_BaseSellValue, ref d.BaseSellValue, toAspect, "ItemTypes", "BaseSellValue");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_QualitySellMultiplier1, ref d.QualitySellMultiplier1, toAspect, "ItemTypes", "QualitySellMultiplier1");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_QualitySellMultiplier2, ref d.QualitySellMultiplier2, toAspect, "ItemTypes", "QualitySellMultiplier2");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_QualitySellMultiplier3, ref d.QualitySellMultiplier3, toAspect, "ItemTypes", "QualitySellMultiplier3");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SellCountMax, ref d.SellCountMax, toAspect, "ItemTypes", "SellCountMax");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SpringSellMultiplier, ref d.SpringSellMultiplier, toAspect, "ItemTypes", "SpringSellMultiplier");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SummerSellMultiplier, ref d.SummerSellMultiplier, toAspect, "ItemTypes", "SummerSellMultiplier");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_AutumnSellMultiplier, ref d.AutumnSellMultiplier, toAspect, "ItemTypes", "AutumnSellMultiplier");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_WinterSellMultiplier, ref d.WinterSellMultiplier, toAspect, "ItemTypes", "WinterSellMultiplier");
        }
    }

    private static void CreateBuyableAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out BuyableAspect aspect, ref data.BuyableAspect, ref data.HasBuyableAspect, toAspect))
        {
            BuyableAspectData d = data.BuyableAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_ShopOwnerID, ref d.ShopOwnerID, toAspect, "ItemTypes", "ShopOwnerID");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_TraderID, ref d.TraderID, toAspect, "ItemTypes", "TraderID");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_CategoryTrait, ref d.CategoryTrait, toAspect, "ItemTypes", "CategoryTrait");
        }
    }

    private static void CreateGardenableAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out GardenableAspect aspect, ref data.GardenableAspect, ref data.HasGardenableAspect, toAspect))
        {
            GardenableAspectData d = data.GardenableAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_WaterRequirement, ref d.WaterRequirement, toAspect, "ItemTypes", "WaterRequirement");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_NotWateredMultiplier, ref d.NotWateredMultiplier, toAspect, "ItemTypes", "NotWateredMultiplier");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_GrowthPlant, ref d.GrowthPlant, toAspect, "ItemTypes", "GrowthPlant");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_GrowthCrop, ref d.GrowthCrop, toAspect, "ItemTypes", "GrowthCrop");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_LifetimeDays, ref d.LifetimeDays, toAspect, "ItemTypes", "LifetimeDays");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_HarvestCount, ref d.HarvestCount, toAspect, "ItemTypes", "HarvestCount");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_HarvestYield, ref d.HarvestYield, toAspect, "ItemTypes", "HarvestYield");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_GoesToSeed, ref d.GoesToSeed, toAspect, "ItemTypes", "GoesToSeed");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SeedCountWeighting, ref d.SeedCountWeighting, toAspect, "ItemTypes", "SeedCountWeighting");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_CompanionMutliplier, ref d.CompanionMutliplier, toAspect, "ItemTypes", "CompanionMutliplier");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_AvoidSubtractive, ref d.AvoidSubtractive, toAspect, "ItemTypes", "AvoidSubtractive");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_WateredQuality, ref d.WateredQuality, toAspect, "ItemTypes", "WateredQuality");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_NotWateredQuality, ref d.NotWateredQuality, toAspect, "ItemTypes", "NotWateredQuality");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_IsFlower, ref d.IsFlower, toAspect, "ItemTypes", "IsFlower");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_ClubQualityExperience, ref d.ClubQualityExperience, toAspect, "ItemTypes", "ClubQualityExperience");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_PlantableSeasons, ref d.PlantableSeasons, toAspect, "ItemTypes", "PlantableSeasons");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_SeedItem, (a)=>aspect.m_SeedItem = a, ()=>d.SeedItem, (b)=>d.SeedItem = b, toAspect, "ItemTypes", "SeedItem");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_HarvestedItemType, (a)=>aspect.m_HarvestedItemType = a, ()=> d.HarvestedItemType, (b)=>d.HarvestedItemType = b, toAspect, "ItemTypes", "HarvestedItemType");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_CompanionPlantItemTypes, (a)=>aspect.m_CompanionPlantItemTypes = a, ()=> d.CompanionPlantItemTypes, (b)=>d.CompanionPlantItemTypes = b, toAspect, "ItemTypes", "CompanionPlantItemTypes");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_AvoidPlantItemTypes, (a)=>aspect.m_AvoidPlantItemTypes = a, ()=> d.AvoidPlantItemTypes, (b)=>d.AvoidPlantItemTypes = b, toAspect, "ItemTypes", "AvoidPlantItemTypes");
        }
    }

    private static void CreateProduceAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out ProduceAspect aspect, ref data.ProduceAspect, ref data.HasProduceAspect, toAspect))
        {
            ProduceAspectData d = data.ProduceAspect;
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_SeedItem, (a)=>aspect.m_SeedItem = a, ()=>d.SeedItem, (b)=>d.SeedItem = b, toAspect, "ItemTypes", "SeedItem");
        }
    }

    private static void CreatePreviewAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out PreviewAspect previewAspect, ref data.PreviewAspect, ref data.HasPreviewAspect, toAspect))
        {
            PreviewAspectData d = data.PreviewAspect;
            ImportExportUtils.ApplyValueNoNull(ref previewAspect.m_Icon, ref d.Icon, toAspect, "ItemTypes", "Icon");
            ImportExportUtils.ApplyValueNoNull(ref previewAspect.m_HighResIcon, ref d.HighResIcon, toAspect, "ItemTypes", "HighResIcon");
            ImportExportUtils.ApplyValueNoNull(ref previewAspect.m_AutoGeneratedIcon, ref d.AutoGeneratedIcon, toAspect, "ItemTypes", "AutoGeneratedIcon");
            ImportExportUtils.ApplyPrefab(model, ref previewAspect.m_DisplayPrefab, ref d.Prefab, toAspect, false, "ItemTypes", "Prefab");
        }
    }

    private static void CreateCookingAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out CookingAspect cookingAspect, ref data.CookingAspect, ref data.HasCookingAspect, toAspect))
        {
            CookingAspectData d = data.CookingAspect;
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_IsSeasoning, ref d.IsSeasoning, toAspect, "ItemTypes", "IsSeasoning");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_ChoppingBoard, ref d.ChoppingBoard, toAspect, "ItemTypes", "ChoppingBoard");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_FryingPan, ref d.FryingPan, toAspect, "ItemTypes", "FryingPan");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_Saucepan, ref d.Saucepan, toAspect, "ItemTypes", "Saucepan");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_MortarPestle, ref d.MortarPestle, toAspect, "ItemTypes", "MortarPestle");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_PicklingJar, ref d.PicklingJar, toAspect, "ItemTypes", "PicklingJar");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_MixingBowl, ref d.MixingBowl, toAspect, "ItemTypes", "MixingBowl");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_AssemblyType, ref d.AssemblyType, toAspect, "ItemTypes", "AssemblyType");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_CookingTraits, ref d.m_Traits, toAspect, "ItemTypes", "Traits");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_StartTexture.m_ChunkySmooth, ref d.StartTextureChunkySmooth, toAspect, "ItemTypes", "StartTextureChunkySmooth");
            ImportExportUtils.ApplyValueNoNull(ref cookingAspect.m_CookingData.m_StartTexture.m_CrispTender, ref d.StartTextureCrispTender, toAspect, "ItemTypes", "StartTextureCrispTender");
            ImportExportUtils.ApplyPrefab(model, ref cookingAspect.m_Prefab, ref d.Prefab, toAspect, false, "ItemTypes", "Prefab");
            
            // if (toAspect)
            // {
            //     if (d.Prefab != null)
            //     {
            //         if (d.Prefab.displayItem != null)
            //         {
            //             Sprite sprite = null;
            //             ImportExportUtils.ApplyValueNoNull(ref sprite, ref d.Prefab.displayItem.image, toAspect, "ItemTypes", "DisplayItem.Image");
            //             if (sprite == null)
            //             {
            //                 APILogger.LogInfo("CookingAspect for " + model.name + " has no sprite!");
            //             }
            //             else
            //             {
            //                 APILogger.LogVerbose("CookingAspect for " + model.name + " has a sprite.");
            //             }
            //             
            //             float scaleMultiplier = d.Prefab.displayItem.imageScale;
            //             Vector3 scale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
            //             cookingAspect.m_Prefab = PrefabFactory.CreatePrefab(model, sprite, scale, model.name);
            //             APILogger.LogVerbose("CookingAspect for prefab created " + cookingAspect.m_Prefab.name);
            //         }
            //         else
            //         {
            //             APILogger.LogVerbose("CookingAspect for " + model.name + " has no displayItem.");
            //         }
            //     }
            //     else
            //     {
            //         APILogger.LogVerbose("CookingAspect for " + model.name + " has no prefab.");
            //     }
            // }
        }
    }

    private static void CreateFishableAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out FishableAspect fishableAspect, ref data.FishableAspect, ref data.HasFishableAspect, toAspect))
        {
            FishableAspectData d = data.FishableAspect;
            ImportExportUtils.ApplyValueNoNull(ref fishableAspect.m_SeasonAvailabilities, ref d.SeasonAvailabilities, toAspect, "ItemTypes", "SeasonAvailabilities");
            ImportExportUtils.ApplyValueNoNull(ref fishableAspect.m_PreferredWeatherCondition, ref d.PreferredWeatherCondition, toAspect, "ItemTypes", "PreferredWeatherCondition");
            ImportExportUtils.ApplyValueNoNull(ref fishableAspect.m_PreferredDayPeriod, ref d.PreferredDayPeriod, toAspect, "ItemTypes", "PreferredDayPeriod");
            ImportExportUtils.ApplyValueNoNull(ref fishableAspect.m_PreferredLocation, ref d.PreferredLocation, toAspect, "ItemTypes", "PreferredLocation");
        }
    }

    private static void CreateFlavourAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out FlavourAspect flavourAspect, ref data.FlavourAspect, ref data.HasFlavourAspect, toAspect))
        {
            FlavourAspectData d = data.FlavourAspect;
            ImportExportUtils.ApplyValueNoNull(ref flavourAspect.m_Flavour, ref d.Flavour, toAspect, "ItemTypes", "Flavour");
        }
    }

    private static void CreateForageableAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out ForageableAspect aspect, ref data.ForageableAspect, ref data.HasForageableAspect, toAspect))
        {
            ForageableAspectData d = data.ForageableAspect;
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_HarvestItemType, (a)=>aspect.m_HarvestItemType = a, ()=>d.HarvestItemType, (b)=>d.HarvestItemType = b, toAspect, "ItemTypes", "HarvestItemType");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_HarvestTool, (a)=>aspect.m_HarvestTool = a, ()=>d.HarvestTool, (b)=>d.HarvestTool = b, toAspect, "ItemTypes", "HarvestTool");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_HarvestCountMin, ref d.HarvestCountMin, toAspect, "ItemTypes", "HarvestCountMin");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_HarvestCountMax, ref d.HarvestCountMax, toAspect, "ItemTypes", "HarvestCountMax");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_ClubXPQuality, ref d.ClubXPQuality, toAspect, "ItemTypes", "ClubXPQuality");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_IsAchievement, ref d.IsAchievement, toAspect, "ItemTypes", "IsAchievement");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SeasonAvailabilities, ref d.SeasonAvailabilities, toAspect, "ItemTypes", "SeasonAvailabilities");

            // if (toAspect)
            // {
            //     aspect.m_SeasonAvailabilities = d.SeasonAvailabilities.Select(a=>new SeasonAvailability(a, true)).ToList();
            // }
            // else
            // {
            //     d.SeasonAvailabilities = aspect.m_SeasonAvailabilities.Where(a=>a.m_Available).Select(a=>a.m_Season).ToArray();
            // }
        }
    }

    private static void CreateForageableProduceAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out ForageableProduceAspect aspect, ref data.ForageableProduceAspect, ref data.HasForageableProduceAspect, toAspect))
        {
            ForageableProduceAspectData d = data.ForageableProduceAspect;
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_ForageableItem, (a)=>aspect.m_ForageableItem = a, ()=>d.ForageableItem, (b)=>d.ForageableItem = b, toAspect, "ItemTypes", "ForageableItem");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SeasonAvailabilities, ref d.SeasonAvailabilities, toAspect, "ItemTypes", "SeasonAvailabilities");
        }
    }

    private static void CreateGrowthMediumAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out GrowthMediumAspect growthMediumAspect, ref data.GrowthMediumAspect, ref data.HasGrowthMediumAspect, toAspect))
        {
            GrowthMediumAspectData aspect = data.GrowthMediumAspect;
            ImportExportUtils.ApplyValueNoNull(ref growthMediumAspect.m_InitialWaterLevel, ref aspect.InitialWaterLevel, toAspect, "ItemTypes", "InitialWaterLevel");
            ImportExportUtils.ApplyValueNoNull(ref growthMediumAspect.m_WaterNeededToBeConsideredWatered, ref aspect.WaterNeededToBeConsideredWatered, toAspect, "ItemTypes", "WaterNeededToBeConsideredWatered");
            ImportExportUtils.ApplyValueNoNull(ref growthMediumAspect.m_MaxDaysOfWaterCanHold, ref aspect.MaxDaysOfWaterCanHold, toAspect, "ItemTypes", "MaxDaysOfWaterCanHold");
        }
    }

    private static void CreateMealAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out MealAspect aspect, ref data.MealAspect, ref data.HasMealAspect, toAspect))
        {
            MealAspectData d = data.MealAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_CravingMultiplier, ref d.CravingMultiplier, toAspect, "ItemTypes", "CravingMultiplier");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_MealTrait, ref d.MealTrait, toAspect, "ItemTypes", "MealTrait");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_Recipe, (a)=>aspect.m_Recipe = a, ()=>d.Recipe, (b)=>d.Recipe = b, toAspect, "ItemTypes", "Recipe");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SeasonAvailabilities, ref d.SeasonAvailabilities, toAspect, "ItemTypes", "SeasonAvailabilities");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_RelationshipXPQuality, ref d.RelationshipXPQuality, toAspect, "ItemTypes", "RelationshipXPQuality");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_NpcFondnesses, ref d.NpcFondnesses, toAspect, "ItemTypes", "NpcFondnesses");
            // ImportExportUtils.ApplyValueNoNull(ref aspect.m_PrefsMultipliers, ref d.PrefsMultipliers, toAspect, "ItemTypes", "PrefsMultipliers");
            // ImportExportUtils.ApplyValueNoNull(ref aspect.m_SeasonMultipliers, ref d.SeasonMultipliers, toAspect, "ItemTypes", "SeasonMultipliers");
        }
    }

    private static void CreatePlaceableAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out PlaceableAspect aspect, ref data.PlaceableAspect, ref data.HasPlaceableAspect, toAspect))
        {
            PlaceableAspectData d = data.PlaceableAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_Traits, ref d.Traits, toAspect, "ItemTypes", "Traits");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_SortIndex, ref d.SortIndex, toAspect, "ItemTypes", "SortIndex");
            ImportExportUtils.ApplyPrefab(model, ref aspect.m_Prefab, ref d.Prefab, toAspect, true, "ItemTypes", "Prefab");
        }
    }

    private static void CreateQualityAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out QualityAspect qualityAspect, ref data.QualityAspect, ref data.HasQualityAspect, toAspect))
        {
            QualityAspectData aspect = data.QualityAspect;
            ImportExportUtils.ApplyValueNoNull(ref qualityAspect.m_MaxQuality, ref aspect.MaxQuality, toAspect, "ItemTypes", "MaxQuality");
        }
    }

    private static void CreateRecipeAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out RecipeAspect aspect, ref data.RecipeAspect, ref data.HasRecipeAspect, toAspect))
        {
            RecipeAspectData d = data.RecipeAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_RecipeTrait, ref d.RecipeTrait, toAspect, "ItemTypes", "RecipeTrait");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_FixedIngredients, (a)=>aspect.m_FixedIngredients = a, ()=>d.FixedIngredients, (b)=>d.FixedIngredients = b, toAspect, "ItemTypes", "FixedIngredients");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_VariableIngredients, ref d.VariableTraits, toAspect, "ItemTypes", "VariableTraits");
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_FinishedProduct, (a)=>aspect.m_FinishedProduct = a, ()=>d.FinishedProduct, (b)=>d.FinishedProduct = b, toAspect, "ItemTypes", "FinishedProduct");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_ValidSeasons, ref d.ValidSeasons, toAspect, "ItemTypes", "ValidSeasons");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_PreparationType, ref d.PreparationType, toAspect, "ItemTypes", "PreparationType");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_IdealTextureRange, ref d.IdealTextureRange, toAspect, "ItemTypes", "IdealTextureRange");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_Difficulty, ref d.Difficulty, toAspect, "ItemTypes", "m_Difficulty");
        }
    }

    private static void CreateSeedAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out SeedAspect aspect, ref data.SeedAspect, ref data.HasSeedAspect, toAspect))
        {
            SeedAspectData d = data.SeedAspect;
            ImportExportUtils.DelayedApplyValueNoNull(()=>aspect.m_PlantToGrow, (a)=>aspect.m_PlantToGrow = a, ()=>d.PlantToGrow, (b)=>d.PlantToGrow = b, toAspect, "ItemTypes", "PlantToGrow");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_PlantableSeasons, ref d.SeasonAvailabilities, toAspect, "ItemTypes", "SeasonAvailabilities");
        }
    }

    private static void CreateWateringCanAspect(ItemType model, ItemTypeData data, bool toAspect)
    {
        if (GetAspectAndData(model, out WateringCanAspect aspect, ref data.WateringCanAspect, ref data.HasWateringCanAspect, toAspect))
        {
            WateringCanAspectData d = data.WateringCanAspect;
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_DefaultWaterLevel, ref d.DefaultWaterLevel, toAspect, "ItemTypes", "DefaultWaterLevel");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_WaterCapacity, ref d.WaterCapacity, toAspect, "ItemTypes", "WaterCapacity");
            ImportExportUtils.ApplyValueNoNull(ref aspect.m_WaterPourPerSecond, ref d.WaterPourPerSecond, toAspect, "ItemTypes", "WaterPourPerSecond");
        }
    }

    private static bool GetAspectAndData<T, D>(ItemType itemType, out T aspect, ref D data, ref bool? hasData, bool toAspect) where T : Aspect where D : class, new()
    {
        if (toAspect)
        {
            // From Data to Aspect
            if (!hasData.HasValue)
            {
                // Nothing in the json, so do nothing
                aspect = null;
                return false;
            }
                
            if (!hasData.Value)
            {
                if (itemType.TryGetAspect(out T ignoredAspect))
                {
                    itemType.m_Aspects.Remove(ignoredAspect);
                }

                // Result is there is no aspect in the final game
                aspect = null;
                return false;
            }
            else
            {
                if (!itemType.TryGetAspect(out aspect))
                {
                    aspect = AspectFactory.CreateAspect<T>(itemType);
                    itemType.m_Aspects.Add(aspect);
                }

                return true;
            }
        }
        else
        {
            if(itemType.TryGetAspect(out T existingAspect))
            {
                aspect = existingAspect;
                data = new D();
                hasData = true;
                return true;
            }
            else
            {
                // Result is there is no aspect in the final game
                aspect = null;
                data = null;
                hasData = false;
                return false;
            }
        }
    }


    public static void ExportAll()
    {
        foreach (ItemTypeSet set in ItemManager.Instance.ItemTypeSets)
        {
            string setName = set.name.Replace("Items_ItemTypeSet", "");
            APILogger.LogInfo($"Exporting {set.m_ItemTypes.Count} {setName} ItemTypes.");
            foreach (ItemType itemType in set.m_ItemTypes)
            {
                // GoodsTypes ItemTypesTypes = itemType.name.ToGoodsTypes();
                ItemTypeData data = new ItemTypeData();
                data.Initialize();

                if (ItemTypeManager.NewItemTypesLookup.TryGetValue(itemType.name, out NewItemType newModel))
                {
                    data.Guid = newModel.modGUID;
                    data.Name = newModel.ItemType.name;
                }
                else
                {
                    data.Name = itemType.name;
                }

                Apply(itemType, data, false, itemType.name, false);

                string file = Path.Combine(Plugin.ExportPath, "ItemTypes", setName, itemType.name + fileExtension);
                if (Directory.Exists(Path.GetDirectoryName(file)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                }

                string json = JSONParser.ToJSON(data);
                File.WriteAllText(file, json);
            }
        }
    }
}

public class SchemaHasAspectAttribute : SchemaFieldAttribute
{
    public SchemaHasAspectAttribute() : base(false, "If true then the aspect is added/updated, if false then the aspect is removed, if not defined in the JSON  then no change to the aspect")
    {
    }
}

[GenerateSchema("ItemTypes", "Anything that goes into an inventory", ItemTypesLoader.fileExtension)]
public class ItemTypeData : IInitializable
{
    [SchemaGuid] 
    public string Guid;

    [SchemaName] 
    public string Name;
    
    [SchemaDisplayName] 
    public LocalizableField DisplayName;
    
    [SchemaDescription] 
    public LocalizableField Description;
    
    [SchemaHasAspect]
    public bool? HasStorageAspect;
    public StorageAspectData StorageAspect;
    
    [SchemaHasAspect]
    public bool? HasSellableAspect;
    public SellableAspectData SellableAspect;
    
    [SchemaHasAspect]
    public bool? HasBuyableAspect;
    public BuyableAspectData BuyableAspect;
    
    [SchemaHasAspect]
    public bool? HasGardenableAspect;
    public GardenableAspectData GardenableAspect;
    
    [SchemaHasAspect]
    public bool? HasProduceAspect;
    public ProduceAspectData ProduceAspect;

    [SchemaHasAspect]
    public bool? HasPreviewAspect;
    public PreviewAspectData PreviewAspect;
    
    [SchemaHasAspect]
    public bool? HasCookingAspect;
    public CookingAspectData CookingAspect;
    
    [SchemaHasAspect]
    public bool? HasFishableAspect;
    public FishableAspectData FishableAspect;
    
    [SchemaHasAspect]
    public bool? HasFlavourAspect;
    public FlavourAspectData FlavourAspect;
    
    [SchemaHasAspect]
    public bool? HasForageableAspect;
    public ForageableAspectData ForageableAspect;
    
    [SchemaHasAspect]
    public bool? HasForageableProduceAspect;
    public ForageableProduceAspectData ForageableProduceAspect;
    
    [SchemaHasAspect]
    public bool? HasGrowthMediumAspect;
    public GrowthMediumAspectData GrowthMediumAspect;
    
    [SchemaHasAspect]
    public bool? HasMealAspect;
    public MealAspectData MealAspect;
    
    [SchemaHasAspect]
    public bool? HasPlaceableAspect;
    public PlaceableAspectData PlaceableAspect;
    
    [SchemaHasAspect]
    public bool? HasQualityAspect;
    public QualityAspectData QualityAspect;
    
    [SchemaHasAspect]
    public bool? HasRecipeAspect;
    public RecipeAspectData RecipeAspect;
    
    [SchemaHasAspect]
    public bool? HasSeedAspect;
    public SeedAspectData SeedAspect;
    
    [SchemaHasAspect]
    public bool? HasWateringCanAspect;
    public WateringCanAspectData WateringCanAspect;
    
    public void Initialize()
    {
        DisplayName = new LocalizableField("displayName");
        Description = new LocalizableField("description");
    }
}

public class GameObjectData
{
    public float[] PrimaryChunkRGB;
    public float[] SecondaryChunkRGB;
    public DisplayItemPrefab DisplayItem; // Show an image
}

public class DisplayItemPrefab
{
    public string Image;
    public float? ImageScale;
}

public class CookingAspectData
{
    public GameObjectData Prefab;
    public bool? IsSeasoning;
    public string[] m_Traits;
    public CookingStationOption ChoppingBoard;
    public CookingStationOption FryingPan;
    public CookingStationOption Saucepan;
    public CookingStationOption MortarPestle;
    public CookingStationOption PicklingJar;
    public CookingStationOption MixingBowl;
    public AssemblyType AssemblyType;
    public float? StartTextureChunkySmooth;
    public float? StartTextureCrispTender;
}

public class StorageAspectData
{
    public int? MaxStacks;
    public int? StackSize;
    public bool? InfiniteStackSize;
    public string[] ValidInventories;
    public string DefaultInventory;
}

public class SellableAspectData
{
    public float? BaseSellValue;
    public float? SpringSellMultiplier;
    public float? SummerSellMultiplier;
    public float? AutumnSellMultiplier;
    public float? WinterSellMultiplier;
    public float? QualitySellMultiplier1;
    public float? QualitySellMultiplier2;
    public float? QualitySellMultiplier3;
    public int? SellCountMax;
}

public class BuyableAspectData
{
    public string ShopOwnerID;
    public string TraderID;
    public string CategoryTrait;
}

public class ProduceAspectData
{
    public string SeedItem;
}

public class FlavourAspectData
{
    public FlavourProfile.FlavourType Flavour;
}

public class WateringCanAspectData
{
    public float? DefaultWaterLevel;
    public float? WaterCapacity;
    public float? WaterPourPerSecond;
}

public class ForageableAspectData
{
    public string HarvestItemType;
    public string HarvestTool;
    public int? HarvestCountMin;
    public int? HarvestCountMax;
    public int[] ClubXPQuality;
    public ShireDateTimeConstants.Season[] SeasonAvailabilities;
    public bool? IsAchievement;
}

public class ForageableProduceAspectData
{
    public ShireDateTimeConstants.Season[] SeasonAvailabilities;
    public string ForageableItem;
}

public class FishableAspectData
{
    public ShireDateTimeConstants.Season[] SeasonAvailabilities;
    public WeatherCondition PreferredWeatherCondition;
    public ShireDateTimeConstants.DayPeriod PreferredDayPeriod;
    public FishingController.FishableLocation PreferredLocation;
}

public class PreviewAspectData
{
    public string Icon;
    public string HighResIcon;
    public string AutoGeneratedIcon;
    public GameObjectData Prefab;
}

public class GardenableAspectData
{
    public string SeedItem;
    public int? WaterRequirement;
    public float? NotWateredMultiplier;
    public int? GrowthPlant;
    public int? GrowthCrop;
    public int? LifetimeDays;
    public int? HarvestCount;
    public int? HarvestYield;
    public bool? GoesToSeed;
    public int[] SeedCountWeighting;
    public string[] CompanionPlantItemTypes;
    public string[] AvoidPlantItemTypes;
    public float? CompanionMutliplier;
    public float? AvoidSubtractive;
    public float? WateredQuality;
    public float? NotWateredQuality;
    public string HarvestedItemType;
    public ShireDateTimeConstants.Season[] PlantableSeasons;
    public bool IsFlower;
    public int[] ClubQualityExperience;
    private SeasonMultiplierData[] SeasonMultiplier;
}

public class GrowthMediumAspectData
{
    public float? InitialWaterLevel;
    public float? WaterNeededToBeConsideredWatered;
    public float? MaxDaysOfWaterCanHold;
}

public class PlaceableAspectData
{
    // public string Prefab;
    public string[] Traits;
    public int? SortIndex;
    public GameObjectData Prefab;
}

public class QualityAspectData
{
    public int? MaxQuality;
}

public class SeedAspectData
{
    // public GameObject SeedBagPrefab;
    // public GameObject PlantingPreviewPrefab;
    public string PlantToGrow;
    public ShireDateTimeConstants.Season[] SeasonAvailabilities;
}

public class RecipeAspectData
{
    public string RecipeTrait;
    public string[] FixedIngredients;
    public string[] VariableTraits;
    public string FinishedProduct;
    public ShireDateTimeConstants.Season[] ValidSeasons;
    public RecipeAspect.PreparationType PreparationType;
    public MealTextureIdealRangeData IdealTextureRange;
    public RecipesDatabase.RecipeDifficulty Difficulty;
}

public class MealTextureIdealRangeData
{
    public float ChunkySmoothRangeMin;
    public float ChunkySmoothRangeMax;
    public float CrispTenderRangeMin;
    public float CrispTenderRangeMax;
}

public class MealAspectData
{
    public class QualityXPData
    {
        public int Quality;
        public float XP;
    }
    
    public class NPCMealFondnessData
    {
        public string NPC;
        public Fondness Fondness;
    }
    
    public class NPCMealPrefsMultiplierData
    {
        public string NPC;
        public float Multiplier;
    }
    
    public QualityXPData[] RelationshipXPQuality;
    public float CravingMultiplier;
    public NPCMealFondnessData[] NpcFondnesses;
    public NPCMealPrefsMultiplierData[] PrefsMultipliers;
    public SeasonMultiplierData[] SeasonMultipliers;
    public string MealTrait;
    public ShireDateTimeConstants.Season[] SeasonAvailabilities;
    public string Recipe;
}

public class SeasonMultiplierData
{
    public ShireDateTimeConstants.Season Season;
    public float Multiplier;
}