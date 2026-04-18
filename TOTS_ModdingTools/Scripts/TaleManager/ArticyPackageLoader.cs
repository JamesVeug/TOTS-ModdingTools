using System;
using System.CodeDom;
using System.Collections.Generic;
using Articy.Tots;
using Articy.Unity;
using Articy.Unity.Interfaces;
using TOTS_ModdingTools;
using UnityEngine;

public static class ArticyPackageLoader
{
    private static Dictionary<string, Type> articyTypeMapping = new Dictionary<string, Type>
    {
        { "Dialogue", typeof(NPCDialogue) },
        { "Root", typeof(QuestRoot) },
        { "MainSubQuest", typeof(MainSubQuest) },
        { "Prompt", typeof(QuestDialoguePrompt) },
        { "Exit", typeof(NPCDialogueGoodbye) }
    };
    
    private static ulong NextID = 69_000_000UL;

    private static Type GetTypeFromPassage(TweeFile.Passage passage)
    {
        string id = passage.ID; // Dialogue Orlo_something_something

        string typeName = "Dialogue"; // Default type
        if(id.IndexOf(' ', StringComparison.OrdinalIgnoreCase) >= 0)
        {
            typeName = id.Substring(0, id.IndexOf(' ', StringComparison.OrdinalIgnoreCase));
        }

        if(articyTypeMapping.TryGetValue(typeName, out Type type))
        {
            return type;
        }
        
        string types = string.Join(", ", articyTypeMapping.Keys);
        
        throw new NotImplementedException("Articy type not implemented: " + typeName + ". Implement one of the following types: " + types);
    }

    private static ArticyObject GetContainer()
    {
        return TaleManager.GetArticyObject<ArticyObject>(72057602627862745UL); // Parent of Global_StartTales
    }

    public struct TransitionData
    {
        public ulong NodeTransitioningFrom;
        public TweeFile.Passage.MenuOption MenuOption;
    }

    public class QueuedPassage
    {
        public TweeFile.Passage Passage;
        public ArticyObject Parent;
        public TransitionData TransitionData;
    }
    
    public static ArticyPackage FromTweeFile(string filePath)
    {
        // Load all passages from the tween file
        TweeFile tweeFile = new TweeFile();
        tweeFile.Parse(filePath);

        List<ArticyObject> parents = new List<ArticyObject>();
        ArticyObject Global_StartTales = TaleManager.GetArticyObject<ArticyObject>(72057598333689321UL); // Global_StartTales
        ArticyObject container = TaleManager.GetArticyObject<ArticyObject>(72057602627862745UL); // Parent of Global_StartTales
        parents.Add(container); // Push starting node so all tales start from somewhere
        
        List<ArticyObject> objects = new List<ArticyObject>();
        List<string> parsedPassages = new List<string>();

        
        // Queue<QueuedPassage> passagesToParse = new Queue<QueuedPassage>();
        // passagesToParse.Enqueue(new QueuedPassage()
        // {
        //     Passage = tweeFile.StartingPassageRef,
        //     Parent = container, // This node contains this passage
        //     TransitionData = new TransitionData()
        //     {
        //         NodeTransitioningFrom = container.Id
        //     }
        // });

        ParsePassage(tweeFile.StartingPassageRef, Global_StartTales, null);
        ArticyObject ParsePassage(TweeFile.Passage passage, ArticyObject previousPassage, TweeFile.Passage.MenuOption transitionMenuOption)
        {
            ArticyObject node = CreateObjectFromPassage(tweeFile.StoryTitle, passage, transitionMenuOption);
            objects.Add(node);
            parsedPassages.Add(passage.ID);
            
            bool typeContainsChildren = node.GetType() == typeof(QuestRoot)
                                       || node.GetType() == typeof(MainSubQuest)
                                       || node.GetType() == typeof(QuestDialoguePrompt);
            if(typeContainsChildren)
            {
                parents.Add(node);
                (previousPassage as IInputPinsOwner).GoToChild(node as IInputPinsOwner);
            }
            else
            {
                (previousPassage as IOutputPinsOwner).GoToNext(node as IInputPinsOwner);
            }
            
            foreach (TweeFile.Passage.MenuOption menuOption in passage.MenuOptions)
            {
                if (menuOption.NextPassageRef != null)
                {
                    ParsePassage(menuOption.NextPassageRef, node, menuOption);
                }
                else
                {
                    APILogger.LogError("Menu option " + menuOption.Text + " in passage " + passage.ID + " has an id that connects to no other passage?");
                }
            }
            
            // TODO: Connect output of children with no connections to 'node's output pin

            if (typeContainsChildren)
            {
                foreach (ArticyObject child in node.children)
                {
                    if(child is IOutputPinsOwner outputOwner)
                    {
                        
                    }
                }
            }

            parents.Remove(node);
            return node;
        }
        
        ArticyPackage articyPackage = ScriptableObject.CreateInstance<ArticyPackage>();
        articyPackage.name = tweeFile.StoryTitle;
        articyPackage.mObjects = new List<ArticyObject>
        {
            objects[0]
        };
        articyPackage.mObjects.AddRange(objects[0].children);
        return articyPackage;

        ArticyObject CreateObjectFromPassage(string guid, TweeFile.Passage passage, TweeFile.Passage.MenuOption transitionMenuOption)
        {
            Type typeFromString = GetTypeFromPassage(passage);
            ArticyObject parent = GetArticyObjectParent(typeFromString);
            string technicalName = guid + "_" + passage.ID;
            
            if (typeFromString == typeof(QuestRoot))
            {
                string displayName = GetTaskTrackingDisplayNameFromComments(passage.Comments);
                string description = GetTaskTrackingDescriptionFromComments(passage.Comments);
                return ArticyFactory.CreateQuestRoot(NextID++, parent, technicalName, displayName, description, QuestType.PrimaryTale);
            }
            else if (typeFromString == typeof(SubQuest))
            {
                string displayName = GetTaskTrackingDisplayNameFromComments(passage.Comments);
                string description = GetTaskTrackingDescriptionFromComments(passage.Comments);
                return ArticyFactory.CreateMainSubQuest(NextID++, parent, technicalName, displayName, description, QuestType.PrimaryTale);
            }
            else if (typeFromString == typeof(QuestDialoguePrompt))
            {
                // string displayName = GetTaskTrackingDisplayNameFromComments(passage.Comments);
                // string description = GetTaskTrackingDescriptionFromComments(passage.Comments);
                // return ArticyFactory.CreateQuestDialoguePrompt(NextID++, parent, technicalName, displayName, description, QuestType.PrimaryTale);
            }
            else if (typeFromString == typeof(NPCDialogue))
            {
                ulong speakerID = GetSpeakerIDFromMessage(passage.Message);
                ulong emoteID = GetEmoteFromComments(passage.Comments);
                ulong cameraShotID = GetCameraShotComments(passage.Comments);
            
                return ArticyFactory.CreateNpcDialogue(NextID++,
                    parent,
                    technicalName,
                    passage.Message,
                    transitionMenuOption.Text,
                    speakerID,
                    emoteID,
                    cameraShotID);
            }
            
            throw new NotImplementedException("Articy type not implemented: " + typeFromString + " for passage: " + passage.ID);
        }

        ArticyObject GetArticyObjectParent(Type typeFromString)
        {
            ArticyObject parent = container;
            foreach (ArticyObject parentObject in parents)
            {
                int parentObjectRanking = GetNodeTypeRanking(parentObject.GetType());
                int myRanking = GetNodeTypeRanking(typeFromString);
                
                if (myRanking < parentObjectRanking)
                {
                    parent = parentObject;
                    break;
                }
            }

            return parent;
        }
    }
    
    private static int GetNodeTypeRanking(Type type)
    {
        if (type == typeof(NPCDialogue))
            return 0; // Base Node
        if (type == typeof(QuestDialoguePrompt))
            return 1; // Leaf Node
        if (type == typeof(MainSubQuest))
            return 2; // Mid Node
        if (type == typeof(QuestRoot))
            return 3; // Top Node

        return 99;
    }

    public static string GetTaskTrackingDisplayNameFromComments(List<string> comments)
    {
        foreach (string comment in comments)
        {
            if (comment.Trim().StartsWith("TaskTrackingDisplayName:", StringComparison.OrdinalIgnoreCase))
            {
                return comment.Trim().Substring(24).Trim();
            }
        }

        return "";
    }

    public static string GetTaskTrackingDescriptionFromComments(List<string> comments)
    {
        foreach (string comment in comments)
        {
            if (comment.Trim().StartsWith("TaskTrackingDescription:", StringComparison.OrdinalIgnoreCase))
            {
                return comment.Trim().Substring(24).Trim();
            }
        }

        return "";
    }

    public static ulong GetSpeakerIDFromMessage(string message)
    {
        int indexOf = message.IndexOf(":", StringComparison.OrdinalIgnoreCase);

        string text = message;
        if (indexOf >= 0)
        {
            text = message.Substring(0, indexOf).Trim();
        }
        else
        {
            text = "$You";
        }
        text = text.Replace("$","").Replace(" ","").ToLower();

        switch (text)
        {
            case "player":
            case "me":
            case "you":
                return TaleManager.Player;
            case "orlo":
            case "orloproudfoot":
                return TaleManager.OrloProudfoot;
            case "gandalf":
                return TaleManager.Gandalf;
            case "buttercup":
            case "ladybuttercup":
                return TaleManager.LadyButtercup;
            case "delphi":
            case "delphy":
            case "delphinium":
            case "delphiniumbrandybuck":
                return TaleManager.DelphiniumBrandybuck;
            case "nora":
            case "noraburrows":
                return TaleManager.NoraBurrows;
            case "fosco":
            case "foscoburrows":
                return TaleManager.FoscoBurrows;
            case "marigold":
            case "marigoldpotts":
                return TaleManager.MarigoldPotts;
            case "noakes":
            case "oldnoakes":
                return TaleManager.OldNoakes;
            case "nefi":
                return TaleManager.Nefi;
            case "willow":
            case "willowtook":
                return TaleManager.WillowTook;
            case "hobson":
            case "hobsonhornblower":
                return TaleManager.HobsonHornblower;
            case "farmer":
            case "farmercotton":
                return TaleManager.FarmerCotton;
            case "daisy":
            case "daisytook":
                return TaleManager.DaisyTook;
            case "lily":
            case "lilycotton":
                return TaleManager.LilyCotton;
            case "tom":
            case "tomcotton":
            case "youngtomcotton":
                return TaleManager.YoungTomCotton;
            case "rosie":
            case "rosiecotton":
                return TaleManager.RosieCotton;
            case "sandyman":
                return TaleManager.Sandyman;
            case "assessor":
                return TaleManager.Assessor;
            case "ladle":
                return TaleManager.Ladle;
            
        }

        APILogger.LogWarning("Could not find speaker: '" + text + "'. Defaulting to Player.");
        return TaleManager.Player;
    }

    public static ulong GetEmoteFromComments(List<string> comments)
    {
        foreach (string comment in comments)
        {
            if (comment.Trim().StartsWith("Emote:", StringComparison.OrdinalIgnoreCase))
            {
                string emoteString = comment.Trim().Substring(6).Trim();
                if (!TaleManager.EmoteNameToID.TryGetValue(emoteString, out ulong id))
                {
                    string allEmotes = string.Join(", ", TaleManager.EmoteNameToID.Keys);
                    Debug.LogError($"Could not find emote: '{emoteString}' (available emotes: {allEmotes})");
                    return 0UL;
                }
                
                return id;
            }
        }

        return 0L;
    }
    
    public static ulong GetCameraShotComments(List<string> comments)
    {
        foreach (string comment in comments)
        {
            if (comment.Trim().StartsWith("CameraShot:", StringComparison.OrdinalIgnoreCase))
            {
                string cameraShotID = comment.Trim().Substring(11).Trim();
                if (!TaleManager.CameraShotNameToID.TryGetValue(cameraShotID, out ulong id))
                {
                    string allCameraShots = string.Join(", ", TaleManager.CameraShotNameToID.Keys);
                    Debug.LogError($"Could not find camera shot: '{cameraShotID}' (available camera shots: {allCameraShots})");
                    return 0UL;
                }
                
                return id;
            }
        }

        return 0L;
    }
}