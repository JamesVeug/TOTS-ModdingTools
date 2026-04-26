using System;
using System.Collections.Generic;
using Articy.Tots;
using Articy.Unity;
using HarmonyLib;
using TOTS_ModdingTools;
using TOTS_ModdingTools.Helpers;
using TotS;
using TotS.Audio;
using UnityEngine;
using AudioClip = UnityEngine.AudioClip;
using Path = System.IO.Path;

public static partial class TaleManager
{
    private static AudioInstance dialogueAudioinstance;
    private static void LoadAllVoiceLines()
    {
        // Create audio source to play the sounds
        GameObject dialogueAudioInstance = new GameObject("TaleManager_Audio");
        // dialogueAudioInstance.hideFlags = HideFlags.HideAndDontSave;
        
        AudioSource source = dialogueAudioInstance.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.enabled = true;
        
        dialogueAudioinstance = dialogueAudioInstance.AddComponent<AudioInstance>();
        dialogueAudioinstance.Initialize(source);
        dialogueAudioinstance.SetVolume(1f);
        
        GameObject.DontDestroyOnLoad(dialogueAudioInstance);
        
        
        // Load all audio files from plugins folder
        List<string> sounds = ModdingToolsPlugin.GetFilesInPluginsFolder("*.wav");
        APILogger.LogInfo("Loading " + sounds.Count + " audio files");

        ArticyDatabase database = ArticyDatabase.Instance;
        foreach (string filePath in sounds) // DFr_3E02601A.Text_en.wav
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath); // DFr_3E02601A.Text_en
            int lastIndexOf = fileName.LastIndexOf("_", StringComparison.CurrentCultureIgnoreCase);
            if (lastIndexOf < 0)
            {
                continue;
            }

            string languageCode = fileName.Substring(lastIndexOf + 1); // en
            SystemLanguage language = TOTS_ModdingTools.Localization.LocalizationManager.CodeToLanguage(languageCode);


            int lastFullStop = fileName.LastIndexOf('.');
            if (lastFullStop >= 0)
            {
                fileName = fileName.Substring(0, lastFullStop); // DFr_3E02601A
            }
                
            APILogger.LogInfo("Looking for technical name: " + fileName);
            ArticyObject o = database.InternalGetObject(fileName);
            if (o != null)
            {
                APILogger.LogInfo("Found audio file for object: " + o.name);
                if (o is IObjectWithFeatureDialogueAudio dialogueAudio)
                {
                    APILogger.LogInfo("object is IObjectWithFeatureDialogueAudio");
                    AudioClip clip = AudioHelpers.LoadAudioClip(filePath);
                        
                    ArticyObjectVoiceLine voiceLine = ScriptableObject.CreateInstance<ArticyObjectVoiceLine>();
                    voiceLine.name = filePath;
                    voiceLine.AudioClip = clip;
                    voiceLines.Add(language, fileName, voiceLine);
                    APILogger.LogInfo("assigned voice clip " + voiceLine + " to " + o.technicalName);
                }
            }
            else
            {
                APILogger.LogError("Could not find object for audio file: " + fileName);
            }
        }
            
        APILogger.LogInfo("Done loading audio files");
    }
    
    [HarmonyPatch(typeof(DialogueAudioController), nameof(DialogueAudioController.PlayAudio))]
    [HarmonyPrefix]
    public static bool DialogueAudioController_PlayAudio(ArticyObject audioClip)
    {
        APILogger.LogVerbose("DialogueAudioController_PlayAudio: " + audioClip);
        return true;
    }
    
    [HarmonyPatch(typeof(DialogueAudioController), nameof(DialogueAudioController.ApplyAudio))]
    [HarmonyPrefix]
    public static bool DialogueAudioController_ApplyAudio(DialogueAudioController __instance)
    {
        APILogger.LogVerbose("DialogueAudioController_ApplyAudio");
        ArticyObject audioClip = __instance.m_AudioClip;
        if (audioClip is ArticyObjectVoiceLine voiceLine)
        {
            APILogger.LogVerbose("DialogueAudioController_ApplyAudio is now playing " + voiceLine.name);
            Clip clip = ScriptableObject.CreateInstance<Clip>();
            clip.AudioClip = voiceLine.AudioClip;
          
            dialogueAudioinstance.SetEnabled(true);
            dialogueAudioinstance.Clip = voiceLine.AudioClip;
            dialogueAudioinstance.Play();
        }

        return false;
    }

    [HarmonyPatch(typeof(DialogueAudioController), nameof(DialogueAudioController.ClearAudio))]
    [HarmonyPrefix]
    public static bool DialogueAudioController_ClearAudio(DialogueAudioController __instance)
    {
        APILogger.LogVerbose("DialogueAudioController_ClearAudio");
        if (dialogueAudioinstance != null)
        {
            dialogueAudioinstance.m_AudioSource.Stop();
        }
        return true;
    }
}