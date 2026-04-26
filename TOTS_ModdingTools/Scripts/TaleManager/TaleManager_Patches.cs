using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Articy.Tots;
using Articy.Tots.Features;
using Articy.Unity;
using Articy.Unity.Interfaces;
using HarmonyLib;
using TOTS_ModdingTools;
using TotS;
using TotS.Character.NPCCharacter;
using TotS.Quests;
using TotS.Speech;
using UnityEngine;

public static partial class TaleManager
{
    [HarmonyPatch(typeof(QuestManager.Quest), MethodType.Constructor, new Type[] { typeof(QuestManager), typeof(ArticyObject) })]
    [HarmonyPrefix]
    public static void LogQuestManager(QuestManager owner, ArticyObject articyObject)
    {
        APILogger.LogVerbose(
            "QuestManager.Quest Constructor: " + articyObject.technicalName + " (" + articyObject.Id + ")");
    }

    [HarmonyPatch(typeof(DialoguePlayer), nameof(DialoguePlayer.OnFlowPlayerPaused))]
    [HarmonyPrefix]
    public static void DialoguePlayer_OnFlowPlayerPaused(DialoguePlayer __instance, IFlowObject aObject)
    {
        string text = "null";
        if (aObject != null)
        {
            text = $"[{aObject.GetType().FullName}] {aObject.ToString()}";
        }

        APILogger.LogVerbose("DialoguePlayer.OnFlowPlayerPaused: " + text);
    }

    [HarmonyPatch(typeof(DialogueManager), "TotS.Speech.DialoguePlayer.IInternalDialogueManager.ShowDialogueLine")]
    [HarmonyPrefix]
    public static void DialogueManager_ShowDialogueLine(DialogueManager __instance, 
        ArticyObject speaker,
        string speakerName,
        string localizedText,
        string key,
        List<DialoguePlayer.ResponseBranch> responses,
        DialogueBlocking blocking,
        CameraShot cameraShot,
        DialoguePlayer.DialogueNode.EmoteData startEmoteData,
        DialoguePlayer.DialogueNode.EmoteData endEmoteData,
        ArticyObject pose,
        ArticyObject audioClip)
    {
        APILogger.LogVerbose("DialogueManager_ShowDialogueLine" +
                             "\n- speaker: " + speaker +
                             "\n- speakerName: " + speakerName +
                             "\n- localizedText: " + localizedText +
                             "\n- key: " + key +
                             "\n- cameraShot: " + cameraShot +
                             "\n- startEmoteData: " + startEmoteData +
                             "\n- endEmoteData: " + endEmoteData +
                             "\n- pose: " + pose +
                             "\n- audioClip: " + audioClip);
    }
    
    [HarmonyPatch(typeof(DialoguePlayer), nameof(DialoguePlayer.PausedOnDialogueFragment))]
    [HarmonyPrefix]
    public static bool DialoguePlayer_PausedOnDialogueFragment(DialoguePlayer __instance, IFlowObject aObject)
    {
        ArticyObject articyObject = aObject as ArticyObject;
        StringBuilder builder = new StringBuilder();
        ArticyObject o = articyObject;
        while (o != null)
        {
            builder.Insert(0, $"[{o.GetType().Name}] {o.id}, {o.technicalName}\n");
            o = o.Parent;
        }
        APILogger.LogVerbose(builder.ToString());
        
        
        IObjectWithText text = aObject as IObjectWithText;
        IObjectWithLocalizableText key = aObject as IObjectWithLocalizableText;
        ArticyObject speaker = (ArticyObject)null;
        if (aObject is IObjectWithSpeaker objectWithSpeaker)
        {
            speaker = objectWithSpeaker.Speaker;
            if ((long)speaker.Id == (long)__instance.m_Manager.PlayerArticyRef.id)
            {
                Singleton<PlayerManager>.Instance.ActivePlayer.PlayerHub.PlayerLookable.SetDialoguePriority();
            }
            else
            {
                NPCHub npc = Singleton<NPCManager>.Instance.GetNPC(speaker.Id);
                if ((UnityEngine.Object)npc != (UnityEngine.Object)null)
                {
                    npc.NPCLookable.SetDialoguePriority();
                }
            }
        }

        DialogueBlocking blocking = __instance.m_Handle.GetBlocking();
        CameraShot defaultCameraShot = __instance.m_Manager.DefaultCameraShot;
        
        IObjectWithFeatureCameraDirection withFeatureInParent1 =
            __instance.GetObjectWithFeatureInParent<IObjectWithFeatureCameraDirection>(articyObject);
        CameraShot cameraShot = (CameraShot)null;
        if (withFeatureInParent1 != null)
            cameraShot = withFeatureInParent1.GetFeatureCameraDirection().Shot as CameraShot;
        if ((UnityEngine.Object)cameraShot == (UnityEngine.Object)null)
            cameraShot = defaultCameraShot;
        bool autoSkip = aObject is IObjectWithFeatureNPCDialogueQuestPlaceholder;
        ArticyObject emote1 = (ArticyObject)null;
        bool exitEmoteLoop1 = false;
        ArticyObject emote2 = (ArticyObject)null;
        bool exitEmoteLoop2 = false;
        ArticyObject pose = (ArticyObject)null;
        IObjectWithFeatureDialogueEmote withFeatureInParent2 =
            __instance.GetObjectWithFeatureInParent<IObjectWithFeatureDialogueEmote>(articyObject);
        if (withFeatureInParent2 != null)
        {
            DialogueEmoteFeature featureDialogueEmote = withFeatureInParent2.GetFeatureDialogueEmote();
            emote1 = featureDialogueEmote.StartingEmote;
            exitEmoteLoop1 = featureDialogueEmote.ExitStartEmoteLoop;
            emote2 = featureDialogueEmote.EndingEmote;
            exitEmoteLoop2 = featureDialogueEmote.ExitEndEmoteLoop;
            pose = featureDialogueEmote.EmotePose;
        }

        List<DialoguePlayer.DialogueNode.ReactionEmoteData> reactionDataSet1 =
            new List<DialoguePlayer.DialogueNode.ReactionEmoteData>();
        List<DialoguePlayer.DialogueNode.ReactionEmoteData> reactionDataSet2 =
            new List<DialoguePlayer.DialogueNode.ReactionEmoteData>();
        IObjectWithFeatureDialogueReactions withFeatureInParent3 =
            __instance.GetObjectWithFeatureInParent<IObjectWithFeatureDialogueReactions>(articyObject);
        if (withFeatureInParent3 != null)
        {
            DialogueReactionsFeature dialogueReactions = withFeatureInParent3.GetFeatureDialogueReactions();
            if (dialogueReactions.DialogueStartReactionPairs != null &&
                dialogueReactions.DialogueStartReactionPairs.Count > 0)
            {
                for (int index = 0; index <= dialogueReactions.DialogueStartReactionPairs.Count - 2; index += 2)
                {
                    DialoguePlayer.DialogueNode.ReactionEmoteData reactionEmoteData =
                        new DialoguePlayer.DialogueNode.ReactionEmoteData();
                    reactionEmoteData.SetReactionData(dialogueReactions.DialogueStartReactionPairs[index],
                        dialogueReactions.DialogueStartReactionPairs[index + 1]);
                    reactionDataSet1.Add(reactionEmoteData);
                }
            }

            if (dialogueReactions.DialogueEndReactionPairs != null &&
                dialogueReactions.DialogueEndReactionPairs.Count > 0)
            {
                for (int index = 0; index <= dialogueReactions.DialogueEndReactionPairs.Count - 2; index += 2)
                {
                    DialoguePlayer.DialogueNode.ReactionEmoteData reactionEmoteData =
                        new DialoguePlayer.DialogueNode.ReactionEmoteData();
                    reactionEmoteData.SetReactionData(dialogueReactions.DialogueEndReactionPairs[index],
                        dialogueReactions.DialogueEndReactionPairs[index + 1]);
                    reactionDataSet2.Add(reactionEmoteData);
                }
            }
        }

        DialoguePlayer.DialogueNode.EmoteData startEmoteData = new DialoguePlayer.DialogueNode.EmoteData();
        DialoguePlayer.DialogueNode.EmoteData endEmoteData = new DialoguePlayer.DialogueNode.EmoteData();
        IObjectWithFeatureDialogueEmoteSpeakerAiming withFeatureInParent4 =
            __instance.GetObjectWithFeatureInParent<IObjectWithFeatureDialogueEmoteSpeakerAiming>(
                articyObject);
        if (withFeatureInParent4 != null)
        {
            DialogueEmoteSpeakerAimingFeature emoteSpeakerAiming =
                withFeatureInParent4.GetFeatureDialogueEmoteSpeakerAiming();
            startEmoteData.SetData(emote1, exitEmoteLoop1, emoteSpeakerAiming.StartingEyeAndHeadAimer,
                emoteSpeakerAiming.StartingGesturingAimer, emoteSpeakerAiming.StartingEyeAimingActive,
                emoteSpeakerAiming.StartingHeadAimingActive, emoteSpeakerAiming.StartingAwkwardEyeMovementActive,
                reactionDataSet1);
            endEmoteData.SetData(emote2, exitEmoteLoop2, emoteSpeakerAiming.EndingEyeAndHeadAimer,
                emoteSpeakerAiming.EndingGesturingAimer, emoteSpeakerAiming.EndingEyeAimingActive,
                emoteSpeakerAiming.EndingHeadAimingActive, emoteSpeakerAiming.EndingAwkwardEyeMovementActive,
                reactionDataSet2);
        }

        ArticyObject audioClip = (ArticyObject)null;
        IObjectWithFeatureDialogueAudio iDialogueAudioFeature = __instance.GetObjectWithFeatureInParent<IObjectWithFeatureDialogueAudio>(articyObject);
        if (iDialogueAudioFeature != null)
        {
            APILogger.LogVerbose("DialoguePlayer_PausedOnDialogueFragment: Found IObjectWithFeatureDialogueAudio with audio clip: " + articyObject.technicalName);
            audioClip = iDialogueAudioFeature.GetFeatureDialogueAudio().AudioClipEntity;
            APILogger.LogVerbose("DialoguePlayer_PausedOnDialogueFragment: clip: " + audioClip);
            if (audioClip == null)
            {
                SystemLanguage language = TOTS_ModdingTools.Localization.LocalizationManager.CurrentLanguage;
                if (voiceLines.TryGetValue(language, articyObject.technicalName, out var audioClipWrapper)
                    || voiceLines.TryGetValue(SystemLanguage.English, articyObject.technicalName, out audioClipWrapper))
                {
                    APILogger.LogVerbose("DialoguePlayer_PausedOnDialogueFragment: Found voice clip: " + audioClip);
                    audioClip = audioClipWrapper;
                }
                else
                {
                    APILogger.LogVerbose("DialoguePlayer_PausedOnDialogueFragment: No voice clip found for: " + articyObject.technicalName);
                }
            }
        }
        else
        {
            APILogger.LogVerbose("DialoguePlayer_PausedOnDialogueFragment: Not a audio clip: " + articyObject.technicalName);
        }

        __instance.m_CurrentNode = (DialoguePlayer.Node)new DialoguePlayer.DialogueNode(__instance, speaker, text, key,
            startEmoteData, endEmoteData, pose, audioClip, cameraShot, blocking, autoSkip);

        return false;
    }

    [HarmonyPatch(typeof(DialoguePlayer.DialogueNode), nameof(DialoguePlayer.DialogueNode.RebuildResponses))]
    [HarmonyPrefix]
    public static bool DialoguePlayer_RebuildResponses(DialoguePlayer.DialogueNode __instance, IList<Branch> branches)
    {
        APILogger.LogVerbose("DialoguePlayer.RebuildResponses with " + branches.Count + " branches:");
        foreach (Branch branch in branches)
        {
            string text = "null";
            if (branch.Target != null)
            {
                text = $"[{branch.Target.GetType().FullName}] {branch.Target.ToString()}";
            }
        
            APILogger.LogVerbose(" - Branch: " + text);
        }
        
        return true;
    }

    [HarmonyPatch(typeof(QuestManager.Quest), nameof(QuestManager.Quest.CompleteInternal))]
    [HarmonyPrefix]
    public static bool Quest_CompleteInternal(QuestManager.Quest __instance, List<IOutputPin> outputPins)
    {
        APILogger.LogVerbose("Quest.CompleteInternal: " + __instance.Name + " (" + __instance.ID + ")");
        foreach (IOutputPin outputPin in outputPins)
        {
            APILogger.LogVerbose(" - OutputPin " + outputPin.Id + " with " + outputPin.GetOutgoingConnections().Count +
                              " connections:");
            foreach (var outgoingConnection in outputPin.GetOutgoingConnections())
            {
                var connection = (OutgoingConnection)outgoingConnection;
                if (connection != null)
                {
                    if (connection.mTarget != null)
                    {
                        APILogger.LogVerbose("     - To " + connection.mTarget.GetValue().GetDisplayText());
                    }
                    else
                    {
                        APILogger.LogVerbose("     - To null");
                    }
                }
            }
        }
        return true;
    }

    [HarmonyPatch(typeof(Dialogue), nameof(Dialogue.GetInputPins))]
    [HarmonyPrefix]
    public static bool Dialogue_GetInputPins(Dialogue __instance)
    {
        APILogger.LogVerbose("Dialogue.GetInputPins: " + __instance.GetDisplayText());
        foreach (InputPin pin in __instance.mInputPins.value)
        {
            if (pin != null)
            {
                APILogger.LogVerbose(" - InputPin " + pin.Id + " with " + pin.GetOutgoingConnections().Count +
                                  " connections:");
                foreach (var outgoingConnection in pin.GetOutgoingConnections())
                {
                    var connection = (OutgoingConnection)outgoingConnection;
                    if (connection != null)
                    {
                        if (connection.mTarget != null)
                        {
                            APILogger.LogVerbose("     - To " + connection.mTarget.GetValue().GetDisplayText());
                        }
                        else
                        {
                            APILogger.LogVerbose("     - To null");
                        }
                    }
                }
            }
            else
            {
                APILogger.LogVerbose(" - InputPin is null");
            }
        }

        return true;
    }

    [HarmonyPatch(typeof(QuestManager), nameof(QuestManager.Pump))]
    [HarmonyPrefix]
    public static bool QuestManager_Pump(QuestManager __instance)
    {
        string queue = "Pumping queue:";
        Queue<QuestManager.PumpStep> steps = __instance.m_PumpQueue;
        if (steps.Count > 0)
        {
            foreach (QuestManager.PumpStep step in steps)
            {
                // Get field if it's a Quest or AricyObject
                FieldInfo[] fields = step.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                ArticyObject articyObject = null;
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(QuestManager.Quest) &&
                        field.GetValue(step) is QuestManager.Quest quest)
                    {
                        articyObject = quest.m_ArticyObject;
                        queue += $"\n- [{step.GetType().FullName}] " + quest.Name + "(" + quest.ID + ");";
                    }
                    else if (field.FieldType == typeof(ArticyObject) &&
                             field.GetValue(step) is ArticyObject articyObjectCasted)
                    {
                        articyObject = articyObjectCasted;
                        queue += $"\n- [{step.GetType().FullName}] " + articyObjectCasted.technicalName + "(" +
                                 articyObject.Id + ");";
                    }
                    else
                    {
                        queue += $"\n- [{step.GetType().FullName}] {step}";
                    }
                }

                if (articyObject != null)
                {
                    if (articyObject.id == 72057598333689321UL)
                    {
                        QuestManager.PumpCompleteQuest quest = step as QuestManager.PumpCompleteQuest;
                        if (quest != null)
                        {
                            foreach (IOutputPin outputPin in quest.m_OutputPins)
                            {
                                queue +=
                                    $"\n    - OutputPin {outputPin.Id} with {outputPin.GetOutgoingConnections().Count} connections:";
                                foreach (var outgoingConnection in outputPin.GetOutgoingConnections())
                                {
                                    var connection = (OutgoingConnection)outgoingConnection;
                                    if (connection != null)
                                    {
                                        if (connection.mTarget != null)
                                        {
                                            queue += $"\n        - To {connection.mTarget.GetValue().GetDisplayText()}";
                                        }
                                        else
                                        {
                                            queue += $"\n        - To null";
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            queue += $"\n    - Is actually of type {step.GetType().FullName}";
                        }
                    }
                }
            }

            APILogger.LogVerbose(queue);
        }

        return true;
    }
}