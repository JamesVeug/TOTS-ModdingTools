using System.Collections.Generic;
using TOTS_ModdingTools;
using TotS;
using TotS.Items;

public static class TraitManager
{
    public static IReadOnlyList<Trait> AllTraits => s_Traits.AsReadOnly();
    
    private static List<Trait> s_Traits = new List<Trait>();
    private static Dictionary<string, Trait> s_traitLookup = new Dictionary<string, Trait>();

    public static void Initialize(ItemManager itemManagerAsset)
    {
        foreach (Trait trait in itemManagerAsset.m_Traits)
        {
            AddTrait(trait);
        }
        
        foreach (ItemTypeSet set in itemManagerAsset.ItemTypeSets)
        {
            foreach (ItemType type in set.m_ItemTypes)
            {
                if (type.TryGetAspect(out CookingAspect cookingAspect))
                {
                    if (cookingAspect.CookingData != null && cookingAspect.CookingData.m_CookingTraits != null)
                    {
                        foreach (Trait cookingTrait in cookingAspect.CookingData.m_CookingTraits)
                        {
                            AddTrait(cookingTrait);
                        }
                    }
                }
                if (type.TryGetAspect(out BuyableAspect buyableAspect))
                {
                    AddTrait(buyableAspect.m_CategoryTrait);
                }
                if (type.TryGetAspect(out RecipeAspect recipeAspect))
                {
                    AddTrait(recipeAspect.m_RecipeTrait);
                }
                if (type.TryGetAspect(out PlaceableAspect placeableAspect))
                {
                    if (placeableAspect.m_Traits != null)
                    {
                        foreach (Trait placeableTrait in placeableAspect.m_Traits)
                        {
                            AddTrait(placeableTrait);
                        }
                    }
                }
            }
        }
        
        APILogger.LogInfo("TraitManager initialized with " + s_traitLookup.Count + " traits.");
        void AddTrait(Trait trait)
        {
            if (trait != null)
            {
                if (s_traitLookup.TryAdd(trait.name, trait))
                {
                    s_Traits.Add(trait);
                    APILogger.LogInfo("Added trait: " + trait.name);
                }
                
            }
        }
    }
    
    public static bool TryGetTrait(string name, out Trait trait)
    {
        if (s_traitLookup.TryGetValue(name, out trait))
        {
            return true;
        }
        
        trait = null;
        return false;
    }
}