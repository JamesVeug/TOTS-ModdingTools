using System.Collections.Generic;
using Articy.Tots;
using Articy.Unity;
using Articy.Unity.Interfaces;
using HarmonyLib;
using TOTS_ModdingTools;
using TotS;
using UnityEngine;
using NPCDialogue = Articy.Tots.NPCDialogue;
using Object = UnityEngine.Object;

[HarmonyPatch]
public static partial class TaleManager
{
    public static ulong NPC_00 = 72057598332895560UL; 
    public static ulong NPC_01 = 72057598332895564UL; 
    public static ulong NPC_02 = 72057611217814007UL; 
    public static ulong NPC_03 = 72057611217814011UL; 
    public static ulong VisitingHealer = 72057602627868074UL; 
    public static ulong Gandalf = 72057649872529637UL; 
    public static ulong LadyButtercup = 72057649872544853UL; 
    public static ulong OrloProudfoot = 72057598332895542UL; 
    public static ulong DelphiniumBrandybuck = 72057602627862829UL; 
    public static ulong NoraBurrows = 72057602627862833UL; 
    public static ulong FoscoBurrows = 72057602627862837UL; 
    public static ulong MarigoldPotts = 72057602627862841UL; 
    public static ulong OldNoakes = 72057602627862845UL; 
    public static ulong Nefi = 72057602627862849UL; 
    public static ulong WillowTook = 72057602627862853UL; 
    public static ulong HobsonHornblower = 72057602627862857UL; 
    public static ulong FarmerCotton = 72057602627862861UL; 
    public static ulong DaisyTook = 72057598333687769UL; 
    public static ulong LilyCotton = 72057606922838061UL; 
    public static ulong YoungTomCotton = 72057611217805640UL; 
    public static ulong RosieCotton = 72057611217805901UL; 
    public static ulong Sandyman = 72057619807758535UL; 
    public static ulong Assessor = 72057619807765924UL; 
    public static ulong Ladle = 72057611217815137UL; 
    public static ulong UNKNOWN = 72057598332895538UL; 
    public static ulong Player = 72057598332895538UL; 
    public static Dictionary<string, ulong> NPCNameToID = new Dictionary<string, ulong>();
    
    public static ulong CameraShot_R_Mid_Shot = 72057598333694058UL; 
    public static ulong CameraShot_L_Reverse_Mid = 72057598333694062UL; 
    public static ulong CameraShot_L_Two_Shot = 72057598333694066UL; 
    public static ulong CameraShot_L_Wide_Shot = 72057619807736996UL; 
    public static ulong CameraShot_R_High_Wide = 72057619807737000UL; 
    public static ulong CameraShot_R_Low_Mid = 72057619807737004UL; 
    public static ulong CameraShot_L_Mid_Shot = 72057619807737330UL; 
    public static ulong CameraShot_R_Reverse_Mid = 72057619807737334UL; 
    public static ulong CameraShot_R_Two_Shot = 72057619807737338UL; 
    public static ulong CameraShot_R_Wide_Shot = 72057619807737342UL; 
    public static ulong CameraShot_L_High_Wide = 72057619807737346UL; 
    public static ulong CameraShot_L_Low_Mid = 72057619807737350UL; 
    public static ulong CameraShot_R_Close_Shot = 72057645577538436UL; 
    public static ulong CameraShot_R_Overhead = 72057658462445844UL; 
    public static Dictionary<string, ulong> CameraShotNameToID = new Dictionary<string, ulong>();
    
    public static ulong Emote_Agree = 72057598333684365UL; 
    public static ulong Emote_Disagree = 72057598333684369UL; 
    public static ulong Emote_Amazed = 72057598333686145UL; 
    public static ulong Emote_Apologetic = 72057598333686149UL; 
    public static ulong Emote_Confident = 72057598333686153UL; 
    public static ulong Emote_Curiosity = 72057598333686157UL; 
    public static ulong Emote_Delight = 72057598333686161UL; 
    public static ulong Emote_GiftGive = 72057598333686165UL; 
    public static ulong Emote_GiftReceive = 72057598333686169UL; 
    public static ulong Emote_Happiness = 72057598333686173UL; 
    public static ulong Emote_Inspiration = 72057598333686177UL; 
    public static ulong Emote_Laughter = 72057598333686181UL; 
    public static ulong Emote_Mistaken = 72057598333686185UL; 
    public static ulong Emote_Sadness = 72057598333686189UL; 
    public static ulong Emote_Talk = 72057598333686193UL; 
    public static ulong Emote_Greet = 72057598333686584UL; 
    public static ulong Emote_Hunger = 72057598333686588UL; 
    public static ulong Emote_Mischief = 72057598333686592UL; 
    public static ulong Emote_Sleepy = 72057598333686596UL; 
    public static ulong Emote_Smirking = 72057598333686600UL; 
    public static ulong Emote_Thought = 72057598333686604UL; 
    public static ulong Emote_Worry = 72057598333686608UL; 
    public static ulong Emote_Point = 72057619807735199UL; 
    public static ulong Emote_Shrug = 72057619807735209UL; 
    public static ulong Emote_Command = 72057619807735214UL; 
    public static ulong Emote_Refuse = 72057619807735220UL; 
    public static ulong Emote_Dismiss = 72057619807735225UL; 
    public static ulong Emote_Ponder = 72057619807735233UL; 
    public static ulong Emote_Welcome = 72057619807735239UL; 
    public static ulong Emote_Sweat = 72057619807735252UL; 
    public static ulong Emote_Joy = 72057619807735265UL; 
    public static ulong Emote_Look = 72057619807735294UL; 
    public static ulong Emote_Lean = 72057619807735312UL; 
    public static ulong Emote_Question = 72057619807736182UL; 
    public static ulong Emote_Shy = 72057619807736190UL; 
    public static ulong Emote_Wow = 72057619807736194UL; 
    public static ulong Emote_Bashful = 72057619807736198UL; 
    public static ulong Emote_Orlo = 72057619807736244UL; 
    public static ulong Emote_Deadpan = 72057619807736287UL; 
    public static ulong Emote_Approval = 72057619807738247UL; 
    public static ulong Emote_Pleased = 72057619807742436UL; 
    public static ulong Emote_CountOff = 72057619807742440UL; 
    public static ulong Emote_Sit = 72057619807746000UL; 
    public static ulong Emote_Suspicious = 72057619807755221UL; 
    public static ulong Emote_Bewilderment = 72057619807755225UL; 
    public static ulong Emote_Flourish = 72057619807755229UL; 
    public static ulong Emote_Love = 72057619807755233UL; 
    public static ulong Emote_Sighing = 72057619807755237UL; 
    public static ulong Emote_Daydreaming = 72057619807755241UL; 
    public static ulong Emote_Surprise = 72057619807755245UL; 
    public static ulong Emote_Pride = 72057619807755253UL; 
    public static ulong Emote_Aggravation = 72057658462437422UL; 
    public static ulong Emote_Caution = 72057658462437430UL; 
    public static ulong Emote_Embarrassed = 72057658462437434UL; 
    public static ulong Emote_Question01 = 72057658462437438UL; 
    public static ulong Emote_Question02 = 72057658462437442UL; 
    public static ulong Emote_Question03 = 72057658462437446UL; 
    public static ulong Emote_Fearful = 72057658462437450UL; 
    public static ulong Emote_Distressed = 72057658462437454UL; 
    public static ulong Emote_Chilly = 72057658462437458UL; 
    public static ulong Emote_Appraise = 72057658462437462UL; 
    public static ulong Emote_Talk02 = 72057658462438065UL; 
    public static ulong Emote_Talk03 = 72057658462438069UL; 
    public static ulong Emote_Annoyed = 72057658462438073UL; 
    public static ulong Emote_Wistful = 72057658462438077UL; 
    public static ulong Emote_Shrug02 = 72057658462438081UL; 
    public static ulong Emote_Refuse02 = 72057658462438085UL; 
    public static ulong Emote_Concern = 72057658462438089UL; 
    public static ulong Emote_Realization = 72057658462438093UL; 
    public static ulong Emote_Emphatic = 72057658462438097UL; 
    public static ulong Emote_Idle = 72057658462440883UL; 
    public static ulong Emote_Nefi_Hammerflip = 72057658462443315UL; 
    public static ulong Emote_Nefi_Hammerspin = 72057658462443319UL; 
    public static ulong Emote_Nefi_Ponder = 72057658462443323UL; 
    public static ulong Emote_Nefi_Talk01 = 72057658462443327UL; 
    public static ulong Emote_Nefi_Talk02 = 72057658462443331UL; 
    public static ulong Emote_Nefi_Talk03 = 72057658462443335UL; 
    public static ulong Emote_Nefi_Agree = 72057658462443704UL; 
    public static ulong Emote_Nefi_Disagree = 72057658462443708UL; 
    public static ulong Emote_Nefi_Approval = 72057658462443712UL; 
    public static ulong Emote_Nefi_Sadness = 72057658462443716UL; 
    public static ulong Emote_Gandalf_Agree = 72057658462443720UL; 
    public static ulong Emote_Gandalf_Disagree = 72057658462443724UL; 
    public static ulong Emote_Gandalf_Approval = 72057658462443728UL; 
    public static ulong Emote_Gandalf_Lookaround = 72057658462443732UL; 
    public static ulong Emote_Gandalf_Pleased = 72057658462443736UL; 
    public static ulong Emote_Gandalf_Sadness = 72057658462443740UL; 
    public static ulong Emote_Gandalf_Talk01 = 72057658462443744UL; 
    public static ulong Emote_Gandalf_Talk02 = 72057658462443748UL; 
    public static ulong Emote_Gandalf_Talk03 = 72057658462443752UL; 
    public static ulong Emote_ONW_Agree = 72057658462445017UL; 
    public static ulong Emote_ONW_Disagree = 72057658462445021UL; 
    public static ulong Emote_ONW_Shrug = 72057658462445025UL; 
    public static ulong Emote_ONW_Deadpan = 72057658462445029UL; 
    public static ulong Emote_Gandalf_Ponder = 72057658462447938UL; 
    public static ulong Emote_Repair = 72057649872544601UL; 
    public static ulong Emote_Harvest_Low = 72057649872544605UL;
    public static Dictionary<string, ulong> EmoteNameToID = new Dictionary<string, ulong>();
    
    public static ulong Tales_T_P_MTV_MeetTheVillagers = 72057645577547465UL;

    static TaleManager()
    {
        // Use reflection to find all Emote_ fields and add them to the Emotes dictionary
        var fields = typeof(TaleManager).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        foreach (var field in fields)
        {
            if (field.Name.StartsWith("Emote_"))
            {
                EmoteNameToID[field.Name.Substring("Emote_".Length)] = (ulong)field.GetValue(null);
            }
            else if (field.Name.StartsWith("CameraShot_"))
            {
                CameraShotNameToID[field.Name.Substring("CameraShot_".Length)] = (ulong)field.GetValue(null);
            }
            else if (field.Name.StartsWith("NPC_"))
            {
                NPCNameToID[field.Name.Substring("NPC_".Length)] = (ulong)field.GetValue(null);
            }
        }
    }
    
    private static LocalizedDictionary<string, ArticyObjectVoiceLine> voiceLines = new LocalizedDictionary<string, ArticyObjectVoiceLine>();
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.Initialized), MethodType.Setter)]
    [HarmonyPostfix]
    public static void PostLoadPackages(LocalizationManager __instance)
    {
        if (__instance.Initialized)
        {
            // NOTE: DISABLED UNTIL I COMPLETE PARSING OF TWEE FILE
            // NOTE: Not completed because the tots modding community is non-existent and I want to release everything
            // AddTestDialogue();

            LoadAllVoiceLines();
        }
    }

    public static void AddTestDialogue()
    {
        APILogger.LogInfo("Making custom tale package");
        
        ArticyDatabase database = ArticyDatabase.Instance;
        

        ArticyPackage articyPackage = ScriptableObject.CreateInstance<ArticyPackage>();
        articyPackage.name = "TestDialoguePackage";
        
        QuestDialoguePrompt testObject = CreateTestTale();
        articyPackage.mObjects = new List<ArticyObject>
        {
            testObject
        };
        articyPackage.mObjects.AddRange(testObject.children);

        QuestFragment Global_StartTales = GetArticyObject<QuestFragment>(72057598333689321UL);
        List<OutputPin> outputPins = Global_StartTales.OutputPins;
        outputPins[0].Connections.Add(new OutgoingConnection()
        {
            mTargetPin = testObject.InputPins[0].id,
            mTarget = new ArticyValueArticyObject()
            {
                objectRef = testObject.id,
            }
        });

        if (!database.mLoadedPackages.Contains(articyPackage))
        {
            database.mLoadedPackages.Add(articyPackage);
            if ((bool) (Object) database.mLocalization)
                database.mLocalization.PackageLoaded(articyPackage.name);
        }

        if (articyPackage.objectIds == null)
        {
            articyPackage.objectIds = new HashSet<ulong>();
        }

        Dictionary<ArticyObject, ArticyObject> dictionary = new Dictionary<ArticyObject, ArticyObject>();
        foreach (ArticyObject packageObject in articyPackage.mObjects)
        {
            if (!database.loadedObjects.Contains(packageObject.Id))
            {
                // ArticyObject bareObjectClone = packageObject.CreateBareObjectClone();
                dictionary[packageObject] = packageObject;
                database.mLiveObjects.Add(packageObject);
                database.loadedObjects.Add(packageObject.Id);
            }
            articyPackage.objectIds.Add(packageObject.Id);
        }
        // foreach (KeyValuePair<ArticyObject, ArticyObject> keyValuePair in dictionary)
        //     keyValuePair.Value.CloneObjectProperties(keyValuePair.Key);
        database.forceUpdateDictionaries = true;
        
        
        APILogger.LogInfo("Finished adding custom package");
    }

    private static InputPin CreateInputPin(ulong id, ulong ownerID, ulong targetID, ulong targetPinID)
    {
        return new InputPin()
        {
            id = id,
            mOwner = ownerID,
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
                {
                    new OutgoingConnection()
                    {
                        mTargetPin = targetPinID,
                        mTarget = new ArticyValueArticyObject()
                        {
                            objectRef = targetID
                        }
                    }
                }
            }
        };
    }

    private static QuestDialoguePrompt CreateTestTale()
    {
        ulong id = 69000000UL;
        
        ArticyObject parentNode = GetArticyObject<ArticyObject>(72057602627862745UL); // Parent of Global_StartTales
        
        // Start of tale logic
        QuestRoot root = ArticyFactory.CreateQuestRoot(++id, parentNode, "ModdingTools_Starting_TaleRoot", "Custom Tale", "Test tale!", QuestType.PrimaryTale);
        
        MainSubQuest questRoot = ArticyFactory.CreateMainSubQuest(++id, root, "ModdingTools_StartingTale_SubQuest", "", "", QuestType.PrimaryTale);
        
        QuestDialoguePrompt prompt = ArticyFactory.CreateQuestDialoguePrompt(++id, questRoot, "ModdingTools_StartingTale", "Greeting for Nick T", OrloProudfoot, [OrloProudfoot, Player]);
        
        // Inner dialogue
        NPCDialogue orlo1 = ArticyFactory.CreateNpcDialogue(++id, prompt, "ModdingTools_OrloTalk", "Why hullo [Player.Name]!", "", OrloProudfoot, Emote_Greet, CameraShot_R_Close_Shot);
        NPCDialogue orlo1b = ArticyFactory.CreateNpcDialogue(++id, prompt, "ModdingTools_OrloTalk3", "Greeting, Orlo!", "Hello!", Player, Emote_Greet, CameraShot_L_Mid_Shot);
        
        NPCDialogue orlo2 = ArticyFactory.CreateNpcDialogue(++id, prompt, "ModdingTools_OrloTalk2", "Which engine do you prefer?", "", OrloProudfoot, Emote_Question03, CameraShot_R_High_Wide);
        NPCDialogue orlo2Unity = ArticyFactory.CreateNpcDialogue(++id, prompt, "ModdingTools_OrloTalk2a", "Unity of course", "Unity", Player, Emote_Confident, CameraShot_R_Mid_Shot);
        NPCDialogue orlo2Unreal = ArticyFactory.CreateNpcDialogue(++id, prompt, "ModdingTools_OrloTalk2b", "Unreal of course", "Unreal", Player, Emote_Confident, CameraShot_R_Mid_Shot);
        
        // Connect all the nodes together
        root.GoToChild(questRoot);
        questRoot.GoToChild(prompt);
        prompt.GoToChild(orlo1);
        orlo1.GoToNext(orlo1b);
        orlo1b.GoToNext(orlo2);
        orlo2.GoToNext(orlo2Unity);
        orlo2.GoToNext(orlo2Unreal);
        
        return prompt;
    }

    

    private static void CreateOutputPin(OutputPin outputPin, Dialogue owner, InputPin inputPin)
    {
        outputPin.Connections.Add(new OutgoingConnection()
        {
            mTargetPin = inputPin.id,
            mTarget = new ArticyValueArticyObject()
            {
                objectRef = inputPin.mOwner
            },
        });
    }

    public static T GetArticyObject<T>(ulong id) where T : ArticyObject
    {
        ArticyObject o = ArticyDatabase.GetObject(id);
        if (o != null)
        {
            if (o is T t)
            {
                return t;
            }
            APILogger.LogError("GetArticyObject: Found " + id + " but is not " + typeof(T).FullName + " it is " + o.GetType().FullName);
            return null;
        }

        APILogger.LogError("GetArticyObject: Not found " + id);
        return null;
    }

    public static string GetDisplayText(this ArticyObject articyObject)
    {
        if (articyObject == null)
        {
            return "[null]";
        }
        string text = $"[{articyObject.GetType().Name}][{articyObject.Id}][{articyObject.technicalName}]";
        
        if (articyObject is IObjectWithDisplayName displayName)
        {
            text += $" Name='{displayName.DisplayName}'";
        }
        
        if (articyObject is IObjectWithText textObject)
        {
            text += $" Text='{textObject.Text}'";
        }
        
        return text;
    }
}