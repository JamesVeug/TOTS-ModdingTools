using System;
using System.Collections.Generic;
using UnityEngine;

public class TweeFile
{
    public string StoryTitle;

    public string StartingPassageID;
    public Passage StartingPassageRef;

    public List<Passage> Passages = new List<Passage>();
    public Dictionary<string, Passage> PassageLookup = new Dictionary<string, Passage>();

    public class Passage
    {
        public class MenuOption
        {
            public string Text;
            public string NextPassageID;
            public Passage NextPassageRef;
        }

        public string ID;
        public string SpeakerID = "";
        public string Message = "";
        public List<MenuOption> MenuOptions = new List<MenuOption>();
        public List<string> Comments = new List<string>();
    }

    public void Parse(string filePath)
    {
        Passages.Clear();
        PassageLookup.Clear();

        string allText = System.IO.File.ReadAllText(filePath);
        string[] passages = allText.Split(new string[] { "::" }, System.StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < passages.Length; i++)
        {
            var passage = passages[i];
            string[] lines = passage.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                if (i == 0)
                {
                    StoryTitle = string.Join("\n", lines, 1, lines.Length - 1).Trim();
                    continue;
                }
                else if (i == 1)
                {
                    // :: StoryData
                    // {
                    //     "ifid": "CEDE849B-B7FC-4128-A660-AB31A5A55627",
                    //     "format": "Harlowe",
                    //     "format-version": "3.3.9",
                    //     "start": "Root MyTale",
                    //     "zoom": 1
                    // }

                    foreach (string line in lines)
                    {
                        if (line.AsSpan().Trim().StartsWith("\"start\"", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length == 2)
                            {
                                StartingPassageID = parts[1].Replace("\"", "").Replace(",", "").Trim();
                                // Console.WriteLine("Starting passage ID: " + StartingPassageID);
                                break;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(StartingPassageID))
                    {
                        Debug.LogError("Could not get starting passage ID!");
                    }

                    continue;
                }

                //:: Dialogue Orlo1_1 {"position":"525,675","size":"100,100"}
                //Orlo: Why hullo $PlayerName!
                //
                //[[Hello!->Dialogue Orlo1_2]]
                //
                //
                //<!--Emote:Greet-->
                //<!--CameraShot:R_Close_Shot-->

                string title = lines[0].Trim();
                if (title.Contains('{', StringComparison.OrdinalIgnoreCase))
                {
                    title = title.Substring(0, title.IndexOf('{', StringComparison.CurrentCultureIgnoreCase)).Trim();
                }


                Passage newPassage = new Passage();
                newPassage.ID = title;
                Passages.Add(newPassage);
                PassageLookup[title] = newPassage;


                for (int j = 1; j < lines.Length; j++)
                {
                    string line = lines[j].Trim();
                    if (line.StartsWith("[[", StringComparison.OrdinalIgnoreCase) &&
                        line.EndsWith("]]", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string optionText = line.Substring(2, line.Length - 4).Trim();
                        string[] optionParts =
                            optionText.Split(new string[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                        if (optionParts.Length == 2)
                        {
                            Passage.MenuOption menuOption = new Passage.MenuOption();
                            menuOption.Text = optionParts[0].Trim();
                            menuOption.NextPassageID = optionParts[1].Trim();
                            newPassage.MenuOptions.Add(menuOption);
                        }
                        else
                        {
                            Passage.MenuOption menuOption = new Passage.MenuOption();
                            menuOption.Text = optionText;
                            menuOption.NextPassageID = optionText;
                            newPassage.MenuOptions.Add(menuOption);
                        }
                    }
                    else if (line.StartsWith("<!--", StringComparison.OrdinalIgnoreCase) &&
                             line.EndsWith("-->", StringComparison.CurrentCultureIgnoreCase))
                    {
                        //<!--Emote:Greet-->
                        //<!--CameraShot:R_Close_Shot-->
                        string optionText = line.Substring(4, line.Length - 7).Trim();
                        newPassage.Comments.Add(optionText);
                    }
                    else if (!string.IsNullOrEmpty(line))
                    {
                        //Orlo: Why hullo $PlayerName!
                        //Oh, it also goes to new lines

                        if (string.IsNullOrEmpty(newPassage.Message))
                        {
                            int colonIndex = line.IndexOf(':');
                            if (colonIndex > 0)
                            {
                                newPassage.SpeakerID = line.Substring(0, colonIndex).Trim();
                                newPassage.Message = line.Substring(colonIndex + 1).Trim();
                            }
                            else
                            {
                                newPassage.Message = line;
                            }
                        }
                        else
                        {
                            newPassage.Message += "\n" + line;
                        }

                    }
                }

                if (string.IsNullOrEmpty(newPassage.ID))
                {
                    Debug.LogError("Passage with empty ID!");
                }
            }
        }

        if (string.IsNullOrEmpty(StartingPassageID))
        {
            Debug.LogError("Starting passage ID is empty!");
        }
        else if (!PassageLookup.TryGetValue(StartingPassageID, out StartingPassageRef))
        {
            Debug.LogError("Could not find starting passage: " + StartingPassageID);
        }

        // Link menu options to passages
        foreach (var passage in Passages)
        {
            for (var i = 0; i < passage.MenuOptions.Count; i++)
            {
                var option = passage.MenuOptions[i];
                if (string.IsNullOrEmpty(StartingPassageID))
                {
                    Debug.LogError("Next passage id is empty for passage " + passage.ID + " and option: " + i + " text: " + option.Text);
                }
                else if (PassageLookup.TryGetValue(option.NextPassageID, out Passage nextPassage))
                {
                    option.NextPassageRef = nextPassage;
                }
                else
                {
                    Debug.LogError(
                        $"Could not find passage for menu option: {option.NextPassageID} in passage: {passage.ID}");
                }
            }
        }
    }
}