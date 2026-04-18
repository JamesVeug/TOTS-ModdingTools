using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TOTS_ModdingTools;
using TinyJson;
using TotS;
using TotS.Customisation;
using UMA;

public class CustomisationColorLoader
{
    public const string fileExtension = "_{0}Color.json";
    
    public static void LoadAll(List<string> files)
    {
        Dictionary<string, NewColorConfig> configLookup = new Dictionary<string, NewColorConfig>();
        configLookup[string.Format(fileExtension, CustomisationManager.Hair.ColorType)] = CustomisationManager.Hair;
        configLookup[string.Format(fileExtension, CustomisationManager.Eyes.ColorType)] = CustomisationManager.Eyes;
        // configLookup[string.Format(fileExtension, CustomisationManager.Skin.ColorType)] = CustomisationManager.Skin;
        
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];

            NewColorConfig config = null;
            foreach (KeyValuePair<string, NewColorConfig> pair in configLookup)
            {
                if (file.EndsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    config = pair.Value;
                    break;
                }
            }

            if (config == null)
            {
                continue;
            }

            ImportExportUtils.SetDebugPath(file);
            files.RemoveAt(i--);

            try
            {
                APILogger.LogVerbose($"Loading JSON (CustomisationColor) {file}");
                CustomisationColorData data = JSONParser.FromFilePath<CustomisationColorData>(file);
                if (data == null)
                {
                    APILogger.LogError($"Failed to load JSON (CustomisationColor) {file}");
                    continue;
                }

                APILogger.LogVerbose($"Parsed JSON {file}");

                string fullName = string.IsNullOrEmpty(data.Guid) ? data.Name : data.Guid + "_" + data.Name;

                CustomisationColor model = null;
                if (config.TryGetColor(fullName, out model))
                {
                    APILogger.LogVerbose($"Found existing CustomisationColor {fullName}");
                }
                else
                {
                    APILogger.LogVerbose($"Creating new CustomisationColor {fullName}");
                    model = config.NewColor(data.Guid, data.Name).color;
                }

                APILogger.LogVerbose($"Applying JSON (CustomisationColor) {file} to CustomisationColor {fullName}"); 
                Apply(model, data, true, fullName, config);

                APILogger.LogVerbose($"Loaded JSON CustomisationColor {fullName}");
            }
            catch (Exception e)
            {
                APILogger.LogError($"Error loading JSON (CustomisationColor) {file}\n{e}");
            }
        }
    }

    public static void Apply(CustomisationColor model, CustomisationColorData data, bool toModel, string modelName, NewColorConfig config)
    {
        ImportExportUtils.SetID(modelName);

        if (toModel)
        {
            model.m_Color = new CharacterCustomisationRebuilder.CustomisationUMAColor([]);

            UMATextRecipe defaultRecipe = config.AllColors[0].m_AssociatedRecipe;
            if (!string.IsNullOrEmpty(data.textRecipeName))
            {
                CustomisationColor matchingRecipe = config.AllColors.FirstOrDefault(c =>
                    c.m_AssociatedRecipe.name.Equals(data.textRecipeName, StringComparison.OrdinalIgnoreCase));
                if (matchingRecipe != null)
                {
                    model.m_AssociatedRecipe = matchingRecipe.m_AssociatedRecipe;
                }
                else
                {
                    APILogger.LogError($"Could not find recipe with name '{data.textRecipeName}' so defaulting to {defaultRecipe.name}");
                    model.m_AssociatedRecipe = defaultRecipe;
                }
            }
            else if(defaultRecipe != null)
            {
                APILogger.LogError($"Recipe not specified so defaulting to {defaultRecipe.name}");
                model.m_AssociatedRecipe = defaultRecipe;
            }
        }
        else
        {
            data.textRecipeName = model.m_AssociatedRecipe != null  ? model.m_AssociatedRecipe.name : null;
        }
        
        ImportExportUtils.ApplyValue(ref model.m_Color.m_Colors, ref data.Colors, toModel, modelName, "Colors");
        
    }

    public static void ExportAll()
    {
        void Export(NewColorConfig config)
        {
            APILogger.LogVerbose($"Exporting CustomisationColor {config.ColorType} ({config.AllColors.Length})");
            foreach (CustomisationColor color in config.AllColors)
            {
                APILogger.LogVerbose("\t" + color.name);
                if (color.AssociatedRecipe != null)
                {
                    APILogger.LogVerbose("\t\thas Recipe! " + color.AssociatedRecipe.name);
                }
                
                CustomisationColorData data = new CustomisationColorData();
                data.Initialize();

                if (config.NewColorsLookup.TryGetValue(color.name, out var newModel))
                {
                    data.Guid = newModel.modGUID;
                    data.Name = newModel.color.name;
                }
                else
                {
                    data.Name = color.name;
                }

                Apply(color, data, false, color.name, config); 

                string extension = string.Format(fileExtension, config.ColorType);
                string file = Path.Combine(ModdingToolsPlugin.ExportPath, "CustomisationColor", config.ColorType, data.Name + extension);
                if (Directory.Exists(Path.GetDirectoryName(file)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                }

                string json = JSONParser.ToJSON(data);
                File.WriteAllText(file, json);
            }
        }
        
        Export(CustomisationManager.Hair);
        Export(CustomisationManager.Eyes);
        // Export(CustomisationManager.Skin);
    }
}

[GenerateSchema("CustomisationColor", "A defined color to be used for hair", CustomisationColorLoader.fileExtension)]
public class CustomisationColorData : IInitializable
{
    [SchemaGuid] 
    public string Guid;

    [SchemaName] 
    public string Name;
    
    public string[] Colors;

    public string textRecipeName;
    
    public void Initialize()
    {
        
    }
}