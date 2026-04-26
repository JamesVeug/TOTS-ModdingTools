using System.Collections.Generic;
using System.Linq;
using Articy.Unity;
using HarmonyLib;
using UnityEngine;


namespace TOTS_ModdingTools.Localization;

[HarmonyPatch]
public static partial class LocalizationManager
{
    [HarmonyPatch(typeof(ArticyLocalizationManager), nameof(ArticyLocalizationManager.Awake))]
    [HarmonyPostfix]
    private static void PostLoadLocalisation(ArticyLocalizationManager __instance)
    {
        m_isInitialized = true;
        
        // Already PostChangeLanguage.
        // ValidateAllLanguagesAreCorrect();
    }

    [HarmonyPatch(typeof(ArticyLocalizationManager), nameof(ArticyLocalizationManager.SwitchLanguage))]
    [HarmonyPostfix]
    private static void PostChangeLanguage(ArticyLocalizationManager __instance, string aLanguage, bool aForceClear = false)
    {
        if (m_currentLanguageCode == aLanguage)
        {
            return;
        }
        
        APILogger.LogError($"Changed language to {aLanguage}");
        m_currentLanguageCode = aLanguage;
        m_currentLanguage = CodeToLanguage(aLanguage);
        if (m_isInitialized)
        {
            SystemLanguage systemLanguage = CodeToLanguage(aLanguage);
            foreach (KeyValuePair<string, Dictionary<SystemLanguage, string>> pair in s_newStrings)
            {
                AddKeyToTextService(pair.Key, pair.Value, systemLanguage);
            }
        }
    }

    private static void ValidateAllLanguagesAreCorrect()
    {
        ArticyLocalizationPackage[] assetToUnload1 = Resources.LoadAll<ArticyLocalizationPackage>($"general_*");
        string[] supportedLanguages = new string[assetToUnload1.Length];
        for (int i = 0; i < assetToUnload1.Length; i++)
        {
            supportedLanguages[i] = assetToUnload1[i].name.Substring("general_".Length);
        }
        
        foreach (string configLanguageCode in supportedLanguages)
        {
            if (!IsLanguageCodeSupported(configLanguageCode))
            {
                APILogger.LogWarning($"Language {configLanguageCode} is not supported by the API");
            }

            // string culture = configLanguage.culture;
            // if (!IsCultureCodeSupported(culture))
            // {
            //     APILogger.LogWarning($"Culture {culture} is not supported by the API");
            // }
        }

        foreach (LanguageData languageData in s_supportedlanguages)
        {
            if (supportedLanguages.All(x => x != languageData.Code))
            {
                APILogger.LogWarning($"Language with culture code {languageData.CultureCode} is not present in the game with its code");
            }

            // if (!__instance.Config.languages.Any(x => x.culture == languageData.CultureCode))
            // {
            //     APILogger.LogWarning($"Language with CultureCode {languageData.CultureCode} is not present in the game with its culture code");
            // }
        }
    }
}