using System.Collections.Generic;
using System.Linq;
using Articy.Tots;
using Articy.Tots.Features;
using Articy.Tots.Templates;
using Articy.Unity;
using Articy.Unity.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ArticyFactory
{
    public static QuestRoot CreateQuestRoot(ulong id, ArticyObject parentNode, string technicalName, string trackingName, string trackingDescription, QuestType questType)
    {
        QuestRoot rootNode = ScriptableObject.CreateInstance<QuestRoot>();
        rootNode.id = id;
        rootNode.parentId = parentNode.id;
        rootNode.technicalName = technicalName;
        rootNode.mAttachments = new ArticyValueArticyModelList();
        rootNode.mAttachments.listIds = [];
        rootNode.mAttachments.list = new List<ArticyObject>();
        rootNode.childrenIds = new List<ulong>();
        rootNode.children = new List<ArticyObject>();
        rootNode.mInputPins = new ArticyValueListInputPin();
        rootNode.mInputPins.SetValue(new List<InputPin>()
        {
            new InputPin()
            {
                id = 1,
                mOwner = rootNode.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptCondition()
                {
                    value = new ArticyScriptCondition()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        rootNode.mOutputPins = new ArticyValueListOutputPin();
        rootNode.mOutputPins.SetValue(new List<OutputPin>()
        {
            new OutputPin()
            {
                id = 1,
                mOwner = rootNode.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptInstruction()
                {
                    value = new ArticyScriptInstruction()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        // rootNode.mLocaKey_DisplayName = "A Test Tale";

        ArticyValueQuestRootTemplate template = new ArticyValueQuestRootTemplate();
        template.value = new QuestRootTemplate();
        rootNode.mTemplate = template;
        
        template.value.mQuestElement = new ArticyValueQuestElementFeature();
        template.value.QuestElement = new QuestElementFeature();
        template.value.QuestElement.mOwnerId = rootNode.id;
        // template.value.QuestElement.OwnerInstanceId = 1;

        template.value.mTaskTrackerRoot = new ArticyValueTaskTrackerRootFeature();
        template.value.TaskTrackerRoot = new TaskTrackerRootFeature();
        template.value.TaskTrackerRoot.mOwnerId = rootNode.id;
        template.value.TaskTrackerRoot.mLocaKey_TaskTrackerDisplayName = trackingName;
        template.value.TaskTrackerRoot.mLocaKey_TaskTrackerDescription = trackingDescription;
        template.value.TaskTrackerRoot.mQuestType = questType;
        // template.value.TaskTrackerRoot.OwnerInstanceId = 1;
        
        
        parentNode.childrenIds.Add(rootNode.id);
        parentNode.children.Add(rootNode);
        return rootNode;
    }

    public static MainSubQuest CreateMainSubQuest(ulong id, ArticyObject parentNode, string technicalName, string trackingName, string trackingDescription, QuestType questType)
    {
        MainSubQuest rootNode = ScriptableObject.CreateInstance<MainSubQuest>();
        rootNode.id = id;
        rootNode.parentId = parentNode.id;
        rootNode.technicalName = technicalName;
        rootNode.mAttachments = new ArticyValueArticyModelList();
        rootNode.mAttachments.listIds = [];
        rootNode.mAttachments.list = new List<ArticyObject>();
        rootNode.childrenIds = new List<ulong>();
        rootNode.children = new List<ArticyObject>();
        rootNode.mInputPins = new ArticyValueListInputPin();
        rootNode.mInputPins.SetValue(new List<InputPin>()
        {
            new InputPin()
            {
                id = 1,
                mOwner = rootNode.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptCondition()
                {
                    value = new ArticyScriptCondition()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        rootNode.mOutputPins = new ArticyValueListOutputPin();
        rootNode.mOutputPins.SetValue(new List<OutputPin>()
        {
            new OutputPin()
            {
                id = 1,
                mOwner = rootNode.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptInstruction()
                {
                    value = new ArticyScriptInstruction()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        // rootNode.mLocaKey_DisplayName = "A Test Tale";

        ArticyValueMainSubQuestTemplate template = new ArticyValueMainSubQuestTemplate();
        template.value = new MainSubQuestTemplate();
        rootNode.mTemplate = template;
        
        template.value.mQuestElement = new ArticyValueQuestElementFeature();
        template.value.QuestElement = new QuestElementFeature();
        template.value.QuestElement.mOwnerId = rootNode.id;

        template.value.mSubQuest = new ArticyValueSubQuestFeature();
        template.value.SubQuest = new SubQuestFeature();
        template.value.SubQuest.mOwnerId = rootNode.id;

        template.value.mTaskTrackerRoot = new ArticyValueTaskTrackerRootFeature();
        template.value.TaskTrackerRoot = new TaskTrackerRootFeature();
        template.value.TaskTrackerRoot.mOwnerId = rootNode.id;
        template.value.TaskTrackerRoot.mLocaKey_TaskTrackerDisplayName = trackingName;
        template.value.TaskTrackerRoot.mLocaKey_TaskTrackerDescription = trackingDescription;
        template.value.TaskTrackerRoot.mQuestType = questType;

        template.value.mQuestStartBarks = new ArticyValueQuestStartBarksFeature();
        template.value.QuestStartBarks = new QuestStartBarksFeature();
        template.value.QuestStartBarks.mOwnerId = rootNode.id;
        template.value.QuestStartBarks.mQuestStepBarks = new ArticyValueArticyObject(){ objectRef = 0UL };
        template.value.QuestStartBarks.mNumberOfBarksToPlay = -1;

        template.value.mQuestSuccessBarks = new ArticyValueQuestSuccessBarksFeature();
        template.value.QuestSuccessBarks = new  QuestSuccessBarksFeature();
        template.value.QuestSuccessBarks.mOwnerId = rootNode.id;
        template.value.QuestSuccessBarks.mQuestSuccessBarks = new ArticyValueArticyObject(){ objectRef = 0UL };

        template.value.mNodeTransition = new ArticyValueNodeTransitionFeature();
        template.value.NodeTransition = new NodeTransitionFeature();
        template.value.NodeTransition.mOwnerId = rootNode.id;
        template.value.NodeTransition.OwnerInstanceId = 5;
        
        parentNode.childrenIds.Add(rootNode.id);
        parentNode.children.Add(rootNode);
        return rootNode;
    }

    public static NPCDialogue CreateNpcDialogue(ulong id, ArticyObject parentNode, string technicalName, string text, string choiceText, ulong speaker, ulong emote=0L, ulong cameraShot=0L)
    {
        NPCDialogue orlo = ScriptableObject.CreateInstance<NPCDialogue>();
        orlo.id = id;
        orlo.parentId = parentNode.id;
        orlo.technicalName = technicalName;
        orlo.childrenIds = new List<ulong>();
        orlo.children = new List<ArticyObject>();
        orlo.mSpeaker = new ArticyValueArticyObject()
        {
            objectRef = speaker 
        };
        orlo.mInputPins = new ArticyValueListInputPin();
        orlo.mInputPins.SetValue(new List<InputPin>()
        {
            new InputPin()
            {
                id = 1,
                mOwner = orlo.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptCondition()
                {
                    value = new ArticyScriptCondition()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        orlo.mOutputPins = new ArticyValueListOutputPin();
        orlo.mOutputPins.SetValue(new List<OutputPin>()
        {
            new OutputPin()
            {
                id = 1,
                mOwner = orlo.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptInstruction()
                {
                    value = new ArticyScriptInstruction()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        orlo.mLocaKey_Text = text;
        // orlo.mLocaKey_StageDirections = "Question03";
        orlo.mLocaKey_MenuText = choiceText;
        
        orlo.mTemplate = new ArticyValueNPCDialogueTemplate();
        orlo.mTemplate.value = new NPCDialogueTemplate();
        
        orlo.mTemplate.value.mCameraDirection.value = new CameraDirectionFeature();
        orlo.mTemplate.value.CameraDirection.mOwnerId = orlo.id;
        // orlo.mTemplate.value.CameraDirection.OwnerInstanceId = 1;
        orlo.mTemplate.value.CameraDirection.mShot.objectRef = cameraShot;
        
        orlo.mTemplate.value.mDialogueEmote = new ArticyValueDialogueEmoteFeature();
        orlo.mTemplate.value.DialogueEmote = new DialogueEmoteFeature();
        orlo.mTemplate.value.DialogueEmote.mOwnerId = orlo.id;
        // orlo.mTemplate.value.DialogueEmote.OwnerInstanceId = 2;
        orlo.mTemplate.value.DialogueEmote.mStartingEmote = new ArticyValueArticyObject();
        orlo.mTemplate.value.DialogueEmote.mStartingEmote.objectRef = emote;
        orlo.mTemplate.value.DialogueEmote.mExitStartEmoteLoop = true;
        orlo.mTemplate.value.DialogueEmote.mEndingEmote = new ArticyValueArticyObject();
        orlo.mTemplate.value.DialogueEmote.mEndingEmote.objectRef = 0UL;
        orlo.mTemplate.value.DialogueEmote.mExitEndEmoteLoop = false;
        orlo.mTemplate.value.DialogueEmote.mEmotePose = new ArticyValueArticyObject();
        orlo.mTemplate.value.DialogueEmote.mEmotePose.objectRef = 0UL;
        
        orlo.mTemplate.value.mDialogueReactions = new ArticyValueDialogueReactionsFeature();
        orlo.mTemplate.value.DialogueReactions = new DialogueReactionsFeature();
        orlo.mTemplate.value.DialogueReactions.mOwnerId = orlo.id;
        // orlo.mTemplate.value.DialogueReactions.OwnerInstanceId = 3;
        orlo.mTemplate.value.DialogueReactions.mDialogueStartReactionPairs = new ArticyValueArticyModelList()
        {
            list = new List<ArticyObject>(),
            listIds = []
        };
        orlo.mTemplate.value.DialogueReactions.mDialogueEndReactionPairs = new ArticyValueArticyModelList()
        {
            list = new List<ArticyObject>(),
            listIds = []
        };

        orlo.mTemplate.value.mDialogueEmoteSpeakerAiming = new ArticyValueDialogueEmoteSpeakerAimingFeature();
        orlo.mTemplate.value.DialogueEmoteSpeakerAiming = new DialogueEmoteSpeakerAimingFeature();
        orlo.mTemplate.value.DialogueEmoteSpeakerAiming.mOwnerId = orlo.id;
        // orlo.mTemplate.value.DialogueEmoteSpeakerAiming.OwnerInstanceId = 4;

        orlo.mTemplate.value.mDialogueAudio = new ArticyValueDialogueAudioFeature();
        orlo.mTemplate.value.DialogueAudio = new DialogueAudioFeature();
        orlo.mTemplate.value.DialogueAudio.mOwnerId = orlo.id;
        // orlo.mTemplate.value.DialogueAudio.OwnerInstanceId = 5;
        orlo.mTemplate.value.DialogueAudio.mAudioClipEntity = new ArticyValueArticyObject();
        orlo.mTemplate.value.DialogueAudio.mAudioClipEntity.objectRef = 0UL;
        
        
        parentNode.childrenIds.Add(orlo.id);
        parentNode.children.Add(orlo);
        return orlo;
    }
    
    public static QuestDialoguePrompt CreateQuestDialoguePrompt(ulong id, ArticyObject parentNode, string technicalName, string menuText, ulong triggerNPC, List<ulong> characters)
    {
        QuestDialoguePrompt rootNode = ScriptableObject.CreateInstance<QuestDialoguePrompt>();
        rootNode.id = id;
        rootNode.parentId = parentNode.id;
        rootNode.technicalName = technicalName;
        rootNode.mAttachments = new ArticyValueArticyModelList();
        rootNode.mAttachments.listIds = [];
        rootNode.mAttachments.list = new List<ArticyObject>();
        rootNode.childrenIds = new List<ulong>();
        rootNode.children = new List<ArticyObject>();
        rootNode.mInputPins = new ArticyValueListInputPin();
        rootNode.mInputPins.SetValue(new List<InputPin>()
        {
            new InputPin()
            {
                id = 1,
                mOwner = rootNode.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptCondition()
                {
                    value = new ArticyScriptCondition()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        rootNode.mOutputPins = new ArticyValueListOutputPin();
        rootNode.mOutputPins.SetValue(new List<OutputPin>()
        {
            new OutputPin()
            {
                id = 1,
                mOwner = rootNode.id,
                mOwnerInstanceId = 0,
                mText = new ArticyValueArticyScriptInstruction()
                {
                    value = new ArticyScriptInstruction()
                    {
                        // ownerId = testObject.id,
                        // handlerId = 1,
                        mRawScript = ""
                    }
                },
                mConnections = new ArticyValueListOutgoingConnection()
                {
                    value = new List<OutgoingConnection>()
                }
            }
        });
        // rootNode.mLocaKey_DisplayName = "A Test Tale";

        ArticyValueQuestDialoguePromptTemplate template = new ArticyValueQuestDialoguePromptTemplate();
        template.value = new QuestDialoguePromptTemplate();
        rootNode.mTemplate = template;
        
        template.value.mQuestElement = new ArticyValueQuestElementFeature();
        template.value.QuestElement = new QuestElementFeature();
        template.value.QuestElement.mOwnerId = rootNode.id;

        template.value.mDialogueSpeakers = new ArticyValueDialogueSpeakersFeature();
        template.value.DialogueSpeakers = new DialogueSpeakersFeature();
        template.value.DialogueSpeakers.mOwnerId = rootNode.id;
        template.value.DialogueSpeakers.mLocationOrderOverride = new ArticyValueArticyModelList();
        template.value.DialogueSpeakers.mLocationOrderOverride.list = new List<ArticyObject>();
        template.value.DialogueSpeakers.mLocationOrderOverride.listIds = [];
        template.value.DialogueSpeakers.mSpeakers = new ArticyValueArticyModelList()
        {
            listIds = characters.ToArray(),
        };

        template.value.mQuestDialoguePrompt = new ArticyValueQuestDialoguePromptFeature();
        template.value.QuestDialoguePrompt = new QuestDialoguePromptFeature();
        template.value.QuestDialoguePrompt.mOwnerId = rootNode.id;
        template.value.QuestDialoguePrompt.mTriggerNPC = new ArticyValueArticyObject();
        template.value.QuestDialoguePrompt.mTriggerNPC.objectRef = triggerNPC;
        template.value.QuestDialoguePrompt.mLocaKey_MenuText = menuText;
        template.value.QuestDialoguePrompt.mIsNag = false;
        template.value.QuestDialoguePrompt.mCondition = new ArticyValueArticyScriptCondition()
        {
            value = new ArticyScriptCondition()
            {
                RawScript = "Player.InQuestLockdown == false"
            }
        };
        
        template.value.mTaskTrackerStep = new ArticyValueTaskTrackerStepFeature();
        template.value.TaskTrackerStep = new TaskTrackerStepFeature();
        template.value.TaskTrackerStep.mOwnerId = rootNode.id;
        template.value.TaskTrackerStep.mLocaKey_TaskTrackerDescription = "No task tracker description set.";
        template.value.TaskTrackerStep.mTaskTrackerCharacters = new ArticyValueArticyModelList()
        {
            listIds = characters.Where(a=>a != TaleManager.Player).ToArray()
        };
        template.value.TaskTrackerStep.mTaskTrackerLocations = new ArticyValueArticyModelList()
        {
            list = new List<ArticyObject>()
            {
                
            }
        };

        template.value.mNodeTransition = new ArticyValueNodeTransitionFeature();
        template.value.NodeTransition = new NodeTransitionFeature();
        template.value.NodeTransition.mOwnerId = rootNode.id;
        // template.value.NodeTransition.OwnerInstanceId = 6;
        return rootNode;
    }

    public static void GoToNext(this IOutputPinsOwner dialogue, IInputPinsOwner other)
    {
        (dialogue.GetOutputPins()[0] as OutputPin).Connections.Add(new OutgoingConnection()
        {
            mTargetPin = (other.GetInputPins()[0] as InputPin).id,
            mTarget = new ArticyValueArticyObject()
            {
                objectRef = other.GetInputPins()[0].Owner,
            }
        });
    }
    
    public static void GoToChild(this IInputPinsOwner dialogue, IInputPinsOwner other)
    {
        (dialogue.GetInputPins()[0] as InputPin).Connections.Add(new OutgoingConnection()
        {
            mTargetPin = (dialogue.GetInputPins()[0] as InputPin).id,
            mTarget = new ArticyValueArticyObject()
            {
                objectRef = other.GetInputPins()[0].Owner,
            }
        });
    }
}