using TOTS_ModdingTools;
using TOTS_ModdingTools.Helpers;
using TOTS_ModdingTools.Localization;
using UnityEngine;

public static class Placeholders
{
    // Reuse the same key. This is a property in case we decide not to cache the sprite in the future
    public static Sprite EffectIcon => EffectIconCache;
    private static Sprite EffectIconCache = TextureHelper.GetWhiteTexture(TextureHelper.SpriteType.EffectIcon).ConvertTexture(TextureHelper.SpriteType.EffectIcon);
    
    public static Texture2D BlackTexture => BlackTextureCache;
    private static Texture2D BlackTextureCache = TextureHelper.GetTexture(Color.black, 1,1);
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string DisplayNameKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "displayName", "Missing DisplayName");
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string PluralDisplayNameKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "pluralDisplayName", "Missing PluralDisplayName");
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string DescriptionKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "description", "Missing Description");
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string GradeKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "grade", "Missing Grade");
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string ShortDescriptionKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "shortDescription", "Missing Short Description");
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string LabelKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "label", "Missing Label");
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string PassiveEffectDescKey = "Common_None_NoDash";
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string TownNameKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "townName", "Missing Town Name");
    
    // Create new object but not new key in case someone tries changing it
    public static readonly string TownDescriptionKey = LocalizationManager.NewString(ModdingToolsPlugin.PLUGIN_GUID, "placeHolders", "townDescription", "Missing Town Description");
}