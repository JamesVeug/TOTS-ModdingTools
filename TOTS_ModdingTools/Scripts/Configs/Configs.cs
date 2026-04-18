using System;
using TOTS_ModdingTools;
using BepInEx.Configuration;

namespace TOTS_ModdingTools;

internal static class Configs
{
    public static APILogger.APILogLevel APILogLevel
    {
        get => m_LogLevel.Value;
        set
        {
            m_LogLevel.Value = value;
            ModdingToolsPlugin.Instance.Config.Save();
        }
    }

    internal static bool ExportGameToJSON
    {
        get
        {
            return m_ExportGameToJSON.Value;
        }
        set
        {
            m_ExportGameToJSON.Value = value;
            ModdingToolsPlugin.Instance.Config.Save();
        }
    }


    private static ConfigEntry<bool> m_ExportGameToJSON;
    private static ConfigEntry<APILogger.APILogLevel> m_LogLevel;
    
    private static ConfigFile m_ConfigFile;
    
    public static void InitializeConfigs(ConfigFile config)
    {
        m_ConfigFile = config;
        
        m_LogLevel = Bind("General", "Log Level", APILogger.APILogLevel.Info, 
            "Set how much logging you want to see in the console. The higher the level the more you will see.\n" +
            "Errors - Only show errors\n" +
            "Warnings - Show errors and warnings\n" +
            "Info - Show errors, warnings and info\n" +
            "Debug - Show all logs including debug logs");
            
        m_ExportGameToJSON = Bind("Exporting", "Export Game to JSON", false, 
            $"When set to true JSONLoader will export as much data as it can to '{ModdingToolsPlugin.ExportPath}'.");
    }

    private static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
	{
		return m_ConfigFile.Bind(section, key, defaultValue, new ConfigDescription(description, null, Array.Empty<object>()));
	}
}