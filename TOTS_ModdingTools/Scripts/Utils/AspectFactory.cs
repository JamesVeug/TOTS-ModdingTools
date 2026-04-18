using TOTS_ModdingTools;
using TotS;
using TotS.Cooking;
using TotS.Items;
using UnityEngine;

public static class AspectFactory
{
    public static T CreateAspect<T>(ItemType itemType) where T : Aspect
    {
        T aspect = ScriptableObject.CreateInstance<T>();
        aspect.m_ItemType = itemType;
        aspect.m_TypeID = itemType.name.GetHashCode();

        if (aspect is CookingAspect cookingAspect)
        {
            cookingAspect.m_CookingData = new CookingItemData(false, 
                null, 
                CookingStationOption.None,
                CookingStationOption.None, 
                CookingStationOption.None, 
                CookingStationOption.None,
                CookingStationOption.None, 
                CookingStationOption.None, 
                new MealTextureValues(0.1f, 0.1f), 
                AssemblyType.Normal);
        }

        return aspect;
    }
}