using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TOTS_ModdingTools;
using TOTS_ModdingTools.Helpers;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using TotS;
using TotS.Lorebook;
using TotS.UI;
using TotS.UI.Rebinding;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using PluginInfo = BepInEx.PluginInfo;

[HarmonyPatch]
public static class MainMenuPatches
{
    [HarmonyPatch(typeof(LorebookSettingsUI), nameof(LorebookSettingsUI.OpenSection))]
    [HarmonyPrefix]
    private static bool LorebookSettingsUI_Start(LorebookSettingsUI __instance)
    {
        APILogger.LogInfo($"LorebookSettingsUI_Start");
        // Setup Header Toggle
        ModeSelectorUI container = __instance.gameObject.GetComponentInChildren<ModeSelectorUI>(true);
        
        ModeSelectorButton templateButton = container.m_ModeButtons[0];
        if (templateButton == null)
        {
            APILogger.LogError("ModeSelectorButton not found in LorebookSettingsUI");
            return true;
        }

        Transform lastChild = templateButton.transform.parent.GetChild(templateButton.transform.parent.childCount - 1);

        ModeSelectorButton newHeaderButton = GameObject.Instantiate(templateButton, templateButton.transform.parent);
        newHeaderButton.gameObject.name = "Mods ModeSelectorButton";
        LocalizedText[] localizedTexts = newHeaderButton.gameObject.SafeGetComponentsInChildren<LocalizedText>(true);
        foreach (LocalizedText text in localizedTexts)
        {
            text.enabled = false;
        }
        // APILogger.LogInfo($"localizedText = {localizedText.FullName()}");
        // localizedText.m_Key = "Mods"; // Set the key to "Mods" for localization
        // localizedText.enabled = false;
        // GameObject.Destroy(localizedText);
        TMP_Text tmpText = newHeaderButton.gameObject.SafeGetComponentInChildren<TMP_Text>(true);
        APILogger.LogInfo($"tmpText = {tmpText.FullName()}");
        tmpText.SetText("Mods");
        
        
        newHeaderButton.m_Button.onClick.RemoveAllListeners();

        int index = container.m_ModeButtons.Length;
        newHeaderButton.m_Button.onClick.AddListener(() =>
        {
            __instance.SelectSettingSection(index);
        });
        
        lastChild.SetAsLastSibling();
        container.m_ModeButtons = container.m_ModeButtons.ForceAdd(newHeaderButton);
        
        
        // Setup section
        GameObject keyboardSection = __instance.m_AllOptionSections[1];
        GameObject modSection = GameObject.Instantiate(keyboardSection, keyboardSection.transform.parent);
        modSection.name = "Mods Settings Section";
        GameObject.Destroy(modSection.GetComponent<GamepadControlsUI>());
        GameObject.Destroy(modSection.GetComponent<RebindingControlsUI>());
        ModsSettingsSection section = modSection.AddComponent<ModsSettingsSection>();
        
        __instance.m_AllOptionSections = __instance.m_AllOptionSections.ForceAdd(modSection);
        return true;
    }
}

public class ModsSettingsSection : MonoBehaviour, ISettingsSection
{
    public List<GameObject> ScrollableObjects => m_Buttons;
    public bool IsDefaultToggleButtonEnabled => true;
    
    private TotSScrollRect m_ScrollRect;
    private List<GameObject> m_Buttons;
    private LorebookSettingsUI m_SettingsUI;
    
    private GameObject m_HeaderText;
    private GameObject m_Toggle;
    private GameObject m_Slider;
    private GameObject m_Dropdown;
    private GameObject m_InputField;

    private void Awake()
    {
        m_SettingsUI = GetComponentInParent<LorebookSettingsUI>();
        m_ScrollRect = m_SettingsUI.GetComponentInChildren<TotSScrollRect>(true);
        
        m_Buttons = new List<GameObject>();

        int childCount = transform.childCount;
        while(childCount > 0)
        {
            Transform child = transform.GetChild(--childCount);
            if (child.gameObject.name.Contains("Header") && m_HeaderText == null)
            {
                APILogger.LogInfo("Found Header: " + child.gameObject.name);
                m_HeaderText = child.gameObject;
                m_HeaderText.gameObject.SetActive(false);
            }
            else if (child.gameObject.name.Contains("Toggle") && m_Toggle == null)
            {
                APILogger.LogInfo("Found Toggle: " + child.gameObject.name);
                m_Toggle = child.gameObject;
                m_Toggle.gameObject.SetActive(false);
            }
            else if (child.gameObject.name.Contains("Slider") && m_Slider == null)
            {
                APILogger.LogInfo("Found Slider: " + child.gameObject.name);
                m_Slider = child.gameObject;
                m_Slider.gameObject.SetActive(false);
            }
            else
            {
                Destroy(child.gameObject);
            }
        }

        GameplayUI gameplayUI = m_SettingsUI.SafeGetComponentInChildren<GameplayUI>(true);
        if (gameplayUI != null)
        {
            foreach (Transform child in gameplayUI.m_ContentRect as Transform)
            {
                if (child.gameObject.name.Contains("Dropdown"))
                {
                    if (m_Dropdown == null)
                    {
                        m_Dropdown = child.gameObject;

                        m_InputField = CreateInputField(null, m_Dropdown);
                        m_InputField.gameObject.SetActive(false);
                    }
                }
            }
        }

        PopulateMods();
    }
    
    private GameObject CreateInputField(Sprite background, GameObject dropdownTemplate)
    {
        GameObject clone = Instantiate(dropdownTemplate, transform);
        clone.name = "InputField Template";
        clone.SetActive(false);
        
        GameObject findChild = clone.FindChild("DetailsPanel");
        if (findChild != null)
        {
            GameObject.Destroy(findChild);
        }
        
        RectTransform DropdownContainer = clone.FindChild("Content/Dropdown/Dropdown/Content/Template").transform.GetRectTransform();
        foreach (Transform transform in DropdownContainer.parent)
        {
            transform.gameObject.SetActive(false);
        }

        Image image = DropdownContainer.gameObject.GetOrAdd<Image>();
        image.color = Color.red;

        GameObject inputField = new GameObject("InputField (Created)");
        inputField.transform.SetParent(DropdownContainer.parent);
        inputField.layer = LayerMask.NameToLayer("UI");
        RectTransform rectTransform = inputField.AddComponent<RectTransform>();
        // rectTransform.anchoredPosition = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localPosition = Vector3.zero;
        // DropdownContainer.gameObject.SetActive(false);
        
        inputField.AddComponent<CanvasRenderer>();
        Image inputFieldImage = inputField.AddComponent<Image>();
        inputFieldImage.sprite = background;
        inputFieldImage.color = new Color(1, 1, 1, 0);
        TMP_InputField field = inputField.AddComponent<TMP_InputField>();
        field.targetGraphic = inputFieldImage;
        
        
        
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputField.transform);
        textArea.layer = LayerMask.NameToLayer("UI");
        RectTransform textAreaRectTransform = textArea.AddComponent<RectTransform>();
        textAreaRectTransform.anchorMin = new Vector2(0, 0);
        textAreaRectTransform.anchorMax = new Vector2(1, 1);
        textAreaRectTransform.anchoredPosition = new Vector2(0, 0);
        textAreaRectTransform.offsetMin = new Vector2(10, 6);
        textAreaRectTransform.offsetMax = new Vector2(-10, -7);
        textAreaRectTransform.localPosition = Vector3.zero;
        textAreaRectTransform.localRotation = Quaternion.identity;
        textAreaRectTransform.localScale = Vector3.one;
        textArea.AddComponent<RectMask2D>();
        
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform);
        placeholder.layer = LayerMask.NameToLayer("UI");
        RectTransform placeholderRectTransform = placeholder.AddComponent<RectTransform>();
        placeholderRectTransform.anchorMin = new Vector2(0, 0);
        placeholderRectTransform.anchorMax = new Vector2(0.9f, 1);
        placeholderRectTransform.anchoredPosition = new Vector2(0, 0);
        placeholderRectTransform.offsetMin = new Vector2(0, 0);
        placeholderRectTransform.offsetMax = new Vector2(0, 0);
        placeholderRectTransform.localPosition = Vector3.zero;
        placeholderRectTransform.localRotation = Quaternion.identity;
        placeholderRectTransform.localScale = Vector3.one;
        placeholder.AddComponent<CanvasRenderer>();
        TextMeshProUGUI placeHolderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeHolderText.color = new Color(1,1,1, 0.5f);
        placeHolderText.fontSize = 24;
        placeHolderText.text = "Enter text...";
        placeHolderText.fontStyle = FontStyles.Italic;
        placeHolderText.alignment = TextAlignmentOptions.Center;
        LayoutElement placeHolderElement = placeholder.AddComponent<LayoutElement>();
        placeHolderElement.ignoreLayout = true;

        GameObject text = new GameObject("Text");
        text.transform.SetParent(textArea.transform);
        text.layer = LayerMask.NameToLayer("UI");
        RectTransform textRectTransform = text.AddComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0, 0);
        textRectTransform.anchorMax = new Vector2(1, 1);
        textRectTransform.anchoredPosition = new Vector2(0, 0);
        textRectTransform.offsetMin = new Vector2(0, 0);
        textRectTransform.offsetMax = new Vector2(0, 0);
        textRectTransform.localPosition = Vector3.zero;
        textRectTransform.localRotation = Quaternion.identity;
        textRectTransform.localScale = Vector3.one;
        text.AddComponent<CanvasRenderer>();
        TextMeshProUGUI typedText = text.AddComponent<TextMeshProUGUI>();
        typedText.fontSize = 24;
        typedText.extraPadding = true;
        typedText.alignment = TextAlignmentOptions.Center;
        
        
        field.textViewport = textAreaRectTransform;
        field.textComponent = text.GetComponent<TMP_Text>();
        field.placeholder = placeholder.GetComponent<TMP_Text>();
        field.textViewport = textAreaRectTransform;
        
        var label = dropdownTemplate.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null) {
            typedText.color = label.color;
            placeHolderText.color = new Color(label.color.r, label.color.g, label.color.b, 0.5f);
        }
        
        return clone;
    }

    public void OpenSettingsSection()
    {
        ToggleDefaultButton(true);
        m_ScrollRect.content = transform as RectTransform;
        
    }

    public bool CloseSettingsSection()
    {
        return true;
    }

    public void RestoreDefaultSettings()
    {
        
    }

    public void UpdateSelectables(List<GameObject> selectables)
    {
        m_Buttons.AddRange(selectables);
        m_SettingsUI.UpdateSelectables(m_Buttons);
    }

    public void ToggleDefaultButton(bool enable)
    {
        
    }

    public class ModEntry
    {
        public string ModName;
        public PluginInfo Plugin;
        public ConfigFile ConfigFile;
    }
    
    private void PopulateMods()
    {
        APILogger.LogInfo("Populating Mods Settings Section");
        List<ModEntry> mods = new List<ModEntry>();
        
        // Gets the file but doesn't load any entries for some reason.
        // if (!Chainloader.PluginInfos.Keys.Contains("BepInEx"))
        // {
        //     string bepInExPath = Path.GetDirectoryName(typeof(Chainloader).Assembly.Location);
        //     string fullPath = Path.GetFullPath(bepInExPath + "/../config/BepInEx.cfg");
        //     APILogger.LogInfo("fullPath: " + fullPath);
        //     
        //     ModEntry bepinexMod = new ModEntry()
        //     {
        //         ModName = "BepInEx",
        //         Plugin = null,
        //         ConfigFile = new ConfigFile(fullPath, false)
        //     };
        //     clone.Add(bepinexMod);
        //
        //     APILogger.LogError("configEnties: " + bepinexMod.ConfigFile.GetConfigEntries().Length);
        //     foreach (ConfigEntryBase configEntry in bepinexMod.ConfigFile.GetConfigEntries())
        //     {
        //         APILogger.LogError("configEntry: " + configEntry.Definition.Key);
        //     }
        // }

        foreach (KeyValuePair<string, PluginInfo> pair in Chainloader.PluginInfos)
        {
            if (pair.Key.Contains("bepinex", StringComparison.OrdinalIgnoreCase))
            {
                APILogger.LogInfo($"Ignoring plugin: {pair.Key}");
                continue;
            }
            
            mods.Add(new ModEntry()
            {
                ModName = GetModName(pair.Value),
                Plugin = pair.Value,
                ConfigFile = pair.Value.Instance.Config
            });
        }
        mods = mods.OrderBy(a=> a.ModName).ToList();
        
        foreach (ModEntry mod in mods)
        {
            APILogger.LogInfo($"Plugin Name: {mod.ModName}");
            string modName = mod.ModName;
            Version modVersion = null;
            if (mod.Plugin != null)
            {
                PluginInfoExtensions.PluginManifest manifest = mod.Plugin.Manifest();

                modVersion = manifest != null && manifest.ManifestVersion() != null
                    ? manifest.ManifestVersion()
                    : mod.Plugin.Metadata.Version;
            }
            else
            {
                
            }

            GameObject go = Instantiate(m_HeaderText, transform);
            go.SetActive(true);
            go.SafeGetComponentInChildren<LocalizedText>().enabled = false;

            if (modVersion != null)
            {
                go.SafeGetComponentInChildren<TMP_Text>().text = $"{modName} v{modVersion}";
            }
            else
            {
                go.SafeGetComponentInChildren<TMP_Text>().text = "";
            }

            ConfigFile file = mod.ConfigFile;
            if (file == null)
            {
                return;
            }
        
            // Go through all keys in the config file
            APILogger.LogInfo($"Config file for {modName} has {file.Keys.Count} entries.");
            foreach (ConfigEntryBase entry in file.GetConfigEntries())
            {
                APILogger.LogInfo($"Config entry: {entry.Definition.Key} ({entry.SettingType.FullName})");
                Type entrySettingType = entry.SettingType;
                AcceptableValueBase acceptableValues = entry.Description.AcceptableValues;
                if(acceptableValues != null)
                {
                    APILogger.LogInfo($"Acceptable values for {entry.Definition.Key}: {acceptableValues.GetType().FullName}");
                    if(acceptableValues.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueList<>))
                    {
                        var genericType = acceptableValues.GetType().GetGenericArguments()[0];
                        var arrayOfObjects = acceptableValues.GetType().GetProperty("AcceptableValues").GetGetMethod().Invoke(acceptableValues, null);
                            
                        // Dropdown of all possible choices
                        if (genericType == typeof(int))
                        {
                            Dictionary<int, int> values = new();
                            foreach (int value in (int[])arrayOfObjects)
                            {
                                values[value] = value;
                            }
                            Dropdown(entrySettingType, values, entry);
                            continue;
                        }
                        else if (genericType == typeof(float))
                        {
                            Dictionary<float, float> values = new();
                            foreach (float value in (float[])arrayOfObjects)
                            {
                                values[value] = value;
                            }
                            Dropdown(entrySettingType, values, entry);
                            continue;
                        }
                        else if (genericType == typeof(string))
                        {
                            Dictionary<string, string> values = new();
                            foreach (string value in (string[])arrayOfObjects)
                            {
                                values[value] = value;
                            }
                            Dropdown(entrySettingType, values, entry);
                            continue;
                        }
                        else if (genericType.IsEnum)
                        {
                            Dictionary<string, string> values = new();
                            foreach (string value in (string[])arrayOfObjects)
                            {
                                values[value] = value;
                            }

                            Dropdown(entrySettingType, values, entry);
                            continue;
                        }
                    }
                    else if (acceptableValues.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueRange<>))
                    {
                        // Slider
                        if (entrySettingType == typeof(int))
                        {
                            int min = (int)acceptableValues.GetType().GetProperty("MinValue").GetValue(acceptableValues);
                            int max = (int)acceptableValues.GetType().GetProperty("MaxValue").GetValue(acceptableValues);
                            AddIntSlider(entry, min, max);
                            continue;
                        }
                        else if (entrySettingType == typeof(float))
                        {
                            float min = (float)acceptableValues.GetType().GetProperty("MinValue").GetGetMethod().Invoke(acceptableValues, null);
                            float max = (float)acceptableValues.GetType().GetProperty("MaxValue").GetGetMethod().Invoke(acceptableValues, null);
                            AddFloatSlider(entry, min, max);
                            continue;
                        }
                    }
                    else
                    {
                        APILogger.LogError($"Unsupported acceptableValues type {acceptableValues.GetType().FullName} for {entry.Definition.Key}");
                    }
                }
                    
                if (entrySettingType == typeof(bool))
                {
                    // Toggle
                    AddToggle(entry);
                }
                else if (entrySettingType == typeof(int))
                {
                    // Slider
                    AddInputField<int>(entry);
                }
                else if (entrySettingType == typeof(float))
                {
                    // InputField
                    AddInputField<float>(entry);
                }
                else if (entrySettingType == typeof(string))
                {
                    // InputField
                    AddInputField<string>(entry);
                }
                else if (entrySettingType.IsEnum)
                {
                    // InputField
                    EnumDropdown(entrySettingType, entry);
                }
                else
                {
                    // Label saying what the type is
                    AddUnknownType(entry);
                }
            }
            
            APILogger.LogInfo("Finished populating config entries for " + modName);
        }
    }

    private void AddUnknownType(ConfigEntryBase entry)
    {
        APILogger.LogError($"Unsupported type {entry.SettingType.FullName} for {entry.Definition.Key}");
        
        GameObject go = GameObject.Instantiate(m_HeaderText, transform);
        go.name = entry.Definition.Key;
        go.SetActive(true);
        go.SafeGetComponentInChildren<LocalizedText>().enabled = false;
        go.SafeGetComponentInChildren<TMP_Text>().text = $"{entry.Definition.Key} ({entry.SettingType.Name})";
    }

    private void AddInputField<T>(ConfigEntryBase entry)
    {
        TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard;
        if(typeof(T) == typeof(int))
        {
            contentType = TMP_InputField.ContentType.IntegerNumber;
        }
        else if(typeof(T) == typeof(float))
        {
            contentType = TMP_InputField.ContentType.DecimalNumber;
        }
        else if(typeof(T) == typeof(string))
        {
            contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            APILogger.LogError($"Unsupported type {typeof(T).FullName} for input field!");
        }
        
        GameObject clone = GameObject.Instantiate(m_InputField, transform);
        clone.gameObject.SetActive(true);
        clone.name = entry.Definition.Key;
        // PopulateToolTip(clone, entry);
        
        Transform label = clone.transform.FindChild("Content/Text (TMP)");
        LocalizedText locale = label.GetComponent<LocalizedText>();
        locale.enabled = false;

        label.SafeGetComponent<TMP_Text>().text = entry.Definition.Key;
        
                    
        TMP_InputField input = clone.SafeGetComponentInChildren<TMP_InputField>(true);
        // input.GetRectTransform().sizeDelta = new Vector2(290, input.GetRectTransform().sizeDelta.y);
        // input.FindChild("Text Area").AddComponent<RectMask2D>();
        input.contentType = contentType;
        if (typeof(T) == typeof(string))
        {
            TMP_Text tmpText = input.FindChild<TMP_Text>("Text Area/Text");
            tmpText.alignment = TextAlignmentOptions.Left;
        }
        input.onValueChanged.AddListener((value) =>
        {
            APILogger.LogInfo($"Setting {entry.Definition.Key} to {value}");
            entry.BoxedValue = (T)Convert.ChangeType(value, typeof(T));
        });
        
        // OptionsMenuEnabled += () =>
        {
            APILogger.LogInfo($"Setting {entry.Definition.Key} to {entry.BoxedValue}");
            input.SetTextWithoutNotify(entry.BoxedValue?.ToString());
        };
    }

    private void AddFloatSlider(ConfigEntryBase entry, float min, float max)
    {
        GameObject slider = GameObject.Instantiate(m_Slider, transform);
        slider.name = entry.Definition.Key;
        // PopulateToolTip(slider, entry);
        
        GameObject.Destroy(slider.GetComponentInChildren<LocalizedText>());
        TMP_Text label = slider.FindChild<TMP_Text>("Label");
        label.text = entry.Definition.Key;
        Slider sliderComponent = slider.SafeGetComponentInChildren<Slider>();
        sliderComponent.minValue = min;
        sliderComponent.maxValue = max;
        sliderComponent.onValueChanged.AddListener((value) =>
        {
            entry.BoxedValue = value;
            label.text = entry.Definition.Key + " (" + entry.BoxedValue + ")";
        });
        
        // OptionsMenuEnabled += () =>
        // {
        sliderComponent.value = (float)entry.BoxedValue;
        label.text = entry.Definition.Key + " (" + entry.BoxedValue + ")";
        // };
    }

    private void AddIntSlider(ConfigEntryBase entry, int min, int max)
    {
        GameObject slider = GameObject.Instantiate(m_Slider, transform);
        slider.name = entry.Definition.Key;
        // PopulateToolTip(slider, entry);
        
        GameObject.Destroy(slider.SafeGetComponentInChildren<LocalizedText>());
        TMP_Text label = slider.SafeGetComponentInChildren<TMP_Text>();
        label.text = entry.Definition.Key;
        Slider sliderComponent = slider.SafeGetComponentInChildren<Slider>();
        sliderComponent.minValue = min;
        sliderComponent.maxValue = max;
        sliderComponent.onValueChanged.AddListener((value) =>
        {
            entry.BoxedValue = (int)value;
            label.text = entry.Definition.Key + " (" + entry.BoxedValue + ")";
        });
        
        // OptionsMenuEnabled += () =>
        // {
        sliderComponent.value = (int)entry.BoxedValue;
        label.text = entry.Definition.Key + " (" + entry.BoxedValue + ")";
        // };
    }

    private void AddToggle(ConfigEntryBase entry)
    {
        GameObject toggle = GameObject.Instantiate(m_Toggle, transform);
        toggle.gameObject.SetActive(true);
        toggle.name = entry.Definition.Key;
        // PopulateToolTip(toggle, entry);
        
        // GameObject.Destroy(toggle.FindChild<TextFontFeaturesHelper>("Label "));
        GameObject.Destroy(toggle.SafeGetComponentInChildren<LocalizedText>());
        toggle.SafeGetComponentInChildren<TMP_Text>().text = entry.Definition.Key;
        
        Toggle toggleComponent = toggle.SafeGetComponentInChildren<Toggle>();
        toggleComponent.onValueChanged.AddListener((value) =>
        {
            entry.BoxedValue = value;
        });
        
        // OptionsMenuEnabled += () =>
        // {
        toggleComponent.isOn = entry.BoxedValue is bool b && b;
        // };
    }

    private void Dropdown<K,V>(Type type, Dictionary<K,V> values, ConfigEntryBase entry)
    {
        GameObject dropdown = Instantiate(m_Dropdown, transform);
        dropdown.name = entry.Definition.Key;
        // PopulateToolTip(dropdown, entry);

        GameObject findChild = dropdown.FindChild("DetailsPanel");
        if (findChild != null)
        {
            GameObject.Destroy(findChild);
        }
        
        Transform label = dropdown.transform.FindChild("Content/Text (TMP)");
        LocalizedText locale = label.GetComponent<LocalizedText>();
        locale.enabled = false;

        label.SafeGetComponent<TMP_Text>().text = entry.Definition.Key;
        
        List<string> keys = values.Keys.Select(a=>a.ToString()).ToList();
        List<V> valuesList = values.Keys.Select(a=>values[a]).ToList();
        
        TMP_Dropdown dropdownComponent = dropdown.FindChild("Content").SafeGetComponentInChildren<TMP_Dropdown>(true);
        dropdownComponent.ClearOptions();
        dropdownComponent.AddOptions(keys);
        dropdownComponent.onValueChanged.AddListener((value) =>
        {
            int i = dropdownComponent.value;
            entry.BoxedValue = valuesList[i];
        });
        
        // OptionsMenuEnabled += () =>
        // {
        int i = valuesList.IndexOf((V)entry.BoxedValue);
        APILogger.LogInfo("Setting dropdown value for " + entry.Definition.Key + " to " + i);
        // dropdownComponent.SetValueWithoutNotify(i);
        // };

        LocalizedDropdown localizedDropdown = dropdown.GetComponentInChildren<LocalizedDropdown>();
        localizedDropdown.Value = i;
        localizedDropdown.SetCaptionLocalizationKey(keys[i]);
    }

    private void EnumDropdown(Type type, ConfigEntryBase entry)
    {
        Dictionary<string, Enum> values = new();
        foreach (Enum value in Enum.GetValues(type))
        {
            values[value.ToString()] = value;
        }
        Dropdown(type, values, entry);
    }
    
    private static string GetModName(PluginInfo plugin)
    {
        PluginInfoExtensions.PluginManifest manifest = plugin.Manifest();
        string modName = manifest != null ? manifest.name : plugin.Metadata.Name;
        return modName;
    }
}