using System.Linq;
using TOTS_ModdingTools;
using TotS.Cooking;
using TotS.Data;
using TotS.Data.Balancing;
using TotS.DateTime;
using TotS.Items;
using TotS.SharedMeals;
using UnityEngine;

public class SeasonAvailabilitiesSerializer : JSONSerializer<ShireDateTimeConstants.Season[], SeasonAvailabilities>, JSONSerializer<SeasonAvailabilities, ShireDateTimeConstants.Season[]>
{
    public SeasonAvailabilities Convert(ShireDateTimeConstants.Season[] from)
    {
        return new SeasonAvailabilities(from.Select(a=>new SeasonAvailability(a,true)));
    }

    public ShireDateTimeConstants.Season[] Convert(SeasonAvailabilities from)
    {
        return from.m_SeasonAvailabilities.Where(a => a.IsAvailable).Select(a => a.m_Season).ToArray();
    }
}

public class TraitSerializer : JSONSerializer<string, Trait>, JSONSerializer<Trait, string>
{
    public Trait Convert(string from)
    {
        if (!TraitManager.TryGetTrait(from, out var trait))
        {
            APILogger.LogError($"Trait '{from}' not found.");
        }
        
        return trait;
    }

    public string Convert(Trait from)
    {
        return from?.name;
    }
}

public class ItemTypeSerializer : JSONSerializer<string, ItemType>, JSONSerializer<ItemType, string>
{
    public ItemType Convert(string from)
    {
        if (!ItemTypeManager.TryGetItemType(from, out var itemType))
        {
            APILogger.LogError($"ItemType '{from}' not found.");
        }
        
        return itemType;
    }

    public string Convert(ItemType from)
    {
        return from?.name;
    }
}

public class QualityXPSerializer : JSONSerializer<MealAspectData.QualityXPData, QualityXP>, JSONSerializer<QualityXP, MealAspectData.QualityXPData>
{
    public QualityXP Convert(MealAspectData.QualityXPData from)
    {
        return new QualityXP(from.Quality, from.XP);
    }

    public MealAspectData.QualityXPData Convert(QualityXP from)
    {
        return new MealAspectData.QualityXPData { Quality = from.Quality, XP = from.XP };
    }
}

public class NPCMealFondnessSerializer : JSONSerializer<MealAspectData.NPCMealFondnessData, NPCMealFondness>, JSONSerializer<NPCMealFondness, MealAspectData.NPCMealFondnessData>
{
    public NPCMealFondness Convert(MealAspectData.NPCMealFondnessData from)
    {
        return new NPCMealFondness(from.NPC, from.Fondness);
    }

    public MealAspectData.NPCMealFondnessData Convert(NPCMealFondness from)
    {
        return new MealAspectData.NPCMealFondnessData { NPC = from.NPC_ID, Fondness = from.Fondness };
    }
}

public class NPCMealPrefsMultiplierSerializer : JSONSerializer<MealAspectData.NPCMealPrefsMultiplierData, NPCMealPrefsMultiplier>, JSONSerializer<NPCMealPrefsMultiplier, MealAspectData.NPCMealPrefsMultiplierData>
{
    public NPCMealPrefsMultiplier Convert(MealAspectData.NPCMealPrefsMultiplierData from)
    {
        return new NPCMealPrefsMultiplier { NPC_ID = from.NPC, Multiplier = from.Multiplier };
    }

    public MealAspectData.NPCMealPrefsMultiplierData Convert(NPCMealPrefsMultiplier from)
    {
        return new MealAspectData.NPCMealPrefsMultiplierData { NPC = from.NPC_ID, Multiplier = from.Multiplier };
    }
}


public class MealTextureIdealRangeSerializer : JSONSerializer<MealTextureIdealRangeData, MealTextureIdealRange>, JSONSerializer<MealTextureIdealRange, MealTextureIdealRangeData>
{
    public MealTextureIdealRange Convert(MealTextureIdealRangeData from)
    {
        return new MealTextureIdealRange(from.ChunkySmoothRangeMin, from.ChunkySmoothRangeMax, from.CrispTenderRangeMin, from.CrispTenderRangeMax);
    }

    public MealTextureIdealRangeData Convert(MealTextureIdealRange from)
    {
        return new MealTextureIdealRangeData
        {
            ChunkySmoothRangeMin = from.ChunkySmoothRange.x,
            ChunkySmoothRangeMax = from.ChunkySmoothRange.y,
            CrispTenderRangeMin = from.CrispTenderRange.x,
            CrispTenderRangeMax = from.CrispTenderRange.y
        };
    }
}


public class SeasonMultiplierSerializer : JSONSerializer<SeasonMultiplierData[], SeasonMultiplier>, JSONSerializer<SeasonMultiplier, SeasonMultiplierData[]>
{
    public SeasonMultiplier Convert(SeasonMultiplierData[] from)
    {
        return new SeasonMultiplier()
        {
            m_SeasonMultipliers = from
                .Select(a => new SeasonMultiplier.SeasonMultiplierTuple(a.Season, a.Multiplier))
                .ToList()
        };
    }

    public SeasonMultiplierData[] Convert(SeasonMultiplier from)
    {
        return from.m_SeasonMultipliers
            .Select(a => new SeasonMultiplierData { Season = a.m_Season, Multiplier = a.m_Multiplier })
            .ToArray();
    }
}
