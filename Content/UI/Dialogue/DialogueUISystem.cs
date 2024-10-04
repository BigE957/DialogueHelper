using DialogueHelper.Content.UI.Dialogue.DialogueStyles;
using DialogueHelper.Content.UI.Dialogue;
using System.Text.Json;
using Terraria.UI;
using System.IO;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using Steamworks;

namespace DialogueHelper.Content.UI.Dialogue
{
    public delegate void DialogueNotifier(string treeKey, int dialogueID, int buttonID);

    public class DialogueUISystem : ModSystem
    {
        public DialogueUIState DialogueUIState;

        public UserInterface DialogueUI;

        public Mod DialogueSource = null;

        public DialogueTree CurrentTree = null;

        public Character CurrentSpeaker = null;

        public Character SubSpeaker = null;

        public DialogueNotifier ButtonClick;

        public DialogueNotifier DialogueOpen;

        public DialogueNotifier DialogueClose;

        public bool justOpened = true;

        public bool isDialogueOpen = false;

        public bool newSpeaker = false;

        public bool newSubSpeaker = false;

        public bool returningSpeaker = false;

        public bool dismissSubSpeaker = false;

        public bool speakerRight = true;

        public bool swappingStyle = false;

        public bool styleSwapped = false;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                DialogueUI = new();
                DialogueUIState = new();
                DialogueUIState.Activate();
            }
        }

        public override void ClearWorld()
        {
            isDialogueOpen = false;
            DialogueUI?.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (DialogueUI?.CurrentState != null)
            {
                DialogueUI?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int SettingsIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Settings Button"));
            if (SettingsIndex != -1)
            {
                layers.Insert(SettingsIndex, new LegacyGameInterfaceLayer(
                    "Windfall: Displays the Dialogue UI",
                    delegate
                    {
                        DialogueUI.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public void DisplayDialogueTree(Mod mod, string TreeKey, int DialogueIndex = 0)
        {
            isDialogueOpen = true;
            justOpened = true;
            speakerRight = true;
            newSpeaker = false;
            newSubSpeaker = false;
            returningSpeaker = false;
            dismissSubSpeaker = false;
            swappingStyle = false;
            styleSwapped = false;

            //Update the DialogueTree array with any new changes (use Hot Reload to apply changes to the function). Use this while testing out your dialogue so you dont have to restart the program every time you add something!
            DialogueSource = mod;
            string activeExtension = LanguageManager.Instance.ActiveCulture.Name;
            string path = Path.Combine("Localization/DialogueTrees", activeExtension, TreeKey + ".json");

            // Fall back to english if not found
            if (!DialogueSource.FileExists(path))
                path = Path.Combine("Localization/DialogueTrees", "en-US", TreeKey + ".json");

            // Throw if we cant find english either
            if (!DialogueSource.FileExists(path))
                throw new FileNotFoundException($"Could not find the dialog file {path}.");

            Stream stream = DialogueSource.GetFileStream(path);

            CurrentTree = JsonSerializer.Deserialize<DialogueTree>(stream);

            stream.Close();

            Dialogue currentDialogue = CurrentTree.Dialogues[DialogueIndex];

            activeExtension = LanguageManager.Instance.ActiveCulture.Name;
            path = Path.Combine("Localization/Characters", activeExtension, CurrentTree.Characters[currentDialogue.CharacterIndex] + ".json");

            // Fall back to english if not found
            if (!DialogueSource.FileExists(path))
                path = Path.Combine("Localization/Characters", "en-US", CurrentTree.Characters[currentDialogue.CharacterIndex] + ".json");

            // Throw if we cant find english either
            if (!DialogueSource.FileExists(path))
                throw new FileNotFoundException($"Could not find the dialog file {path}.");

            stream = DialogueSource.GetFileStream(path);

            CurrentSpeaker = JsonSerializer.Deserialize<Character>(stream);

            stream.Close();

            SubSpeaker = null;

            DialogueOpen?.Invoke(TreeKey, DialogueIndex, 0);

            DialogueUI = new();
            DialogueUIState = new()
            {
                TreeKey = TreeKey,
                DialogueIndex = DialogueIndex
            };
            DialogueUIState.Activate();

            DialogueUI?.SetState(DialogueUIState);
        }

        public void UpdateDialogueUI(string treeKey, int DialogueIndex)
        {
            string activeExtension;
            string path;
            Stream stream;
            #region Update Tree if new tree
            if (treeKey != DialogueUIState.TreeKey)
            {
                activeExtension = LanguageManager.Instance.ActiveCulture.Name;
                path = Path.Combine("Localization/DialogueTrees", activeExtension, treeKey + ".json");

                // Fall back to english if not found
                if (!DialogueSource.FileExists(path))
                    path = Path.Combine("Localization/DialogueTrees", "en-US", "rizz.json");

                // Throw if we cant find english either
                if (!DialogueSource.FileExists(path))
                    throw new FileNotFoundException($"Could not find the dialog file {path}.");

                stream = DialogueSource.GetFileStream(path);

                CurrentTree = JsonSerializer.Deserialize<DialogueTree>(stream);

                stream.Close();
            }
            #endregion
            Dialogue currentDialogue = CurrentTree.Dialogues[DialogueIndex];

            #region UpcomingSpeaker Assignment
            activeExtension = LanguageManager.Instance.ActiveCulture.Name;
            path = Path.Combine("Localization/Characters", activeExtension, CurrentTree.Characters[currentDialogue.CharacterIndex] + ".json");

            // Fall back to english if not found
            if (!DialogueSource.FileExists(path))
                path = Path.Combine("Localization/Characters", "en-US", CurrentTree.Characters[currentDialogue.CharacterIndex] + ".json");

            // Throw if we cant find english either
            if (!DialogueSource.FileExists(path))
                throw new FileNotFoundException($"Could not find the dialog file {path}.");
            stream = DialogueSource.GetFileStream(path);

            Character upcomingSpeaker = JsonSerializer.Deserialize<Character>(stream);

            stream.Close();
            #endregion

            if (upcomingSpeaker.Style != CurrentSpeaker.Style)
                swappingStyle = true;

            if (upcomingSpeaker.Name == CurrentSpeaker.Name)
            {
                //Main.NewText("Speaker Unchanged");
                newSpeaker = false;
                newSubSpeaker = false;
                returningSpeaker = false;
            }
            else if (SubSpeaker == null)
            {
                //Main.NewText("New speaker! No subspeaker");
                newSpeaker = true;
                newSubSpeaker = true;
                returningSpeaker = false;
                SubSpeaker = CurrentSpeaker;
                CurrentSpeaker = upcomingSpeaker;
                speakerRight = !speakerRight;
            }
            else
            {
                //Main.NewText("New speaker! Yes subspeaker.");
                newSpeaker = false;
                newSubSpeaker = true;
                returningSpeaker = true;
                Character temp = (Character)SubSpeaker;
                SubSpeaker = CurrentSpeaker;
                CurrentSpeaker = temp;
                speakerRight = !speakerRight;
            }


            justOpened = false;
            DialogueUI?.SetState(null);
            DialogueUI = new();
            DialogueUIState = new()
            {
                TreeKey = treeKey,
                DialogueIndex = DialogueIndex
            };

            DialogueUI?.SetState(DialogueUIState);
        }

        public void HideDialogueUI()
        {
            isDialogueOpen = false;
            DialogueUI?.SetState(null);
        }

    }

    #region Structures
    /// <param name="ID">The identifier for this Character, commonly their name. This is used primarilly to locate this character's Expression Assets within the Character Assets folder.</param>
    /// <param name="expressions">An array of identifiers, smililar to a Character's <see cref="ID"/>, used to find individual Expression Assets within the Character Assets Folder.</param>
    /// <param name="name">The actual name of the character, used by the Textbox. Can include spaces and other formatting, unlike the <see cref="ID"/>. Will default to the Character's <see cref="ID"/> if not set.</param>
    /// <param name="scale">Determines the scale the character portrait will be drawn at by the Dialogue System. Defaults to <see cref="2f"/>.</param>
    /// <param name="style">The Dialogue Style, in the form of a <see cref="Type"/>, associated with this Character. Defaults to <see cref="null"/>, meaning it will use the <see cref="DefaultDialogueStyle"/>.</param>
    /// <param name="textDelay">The Text Delay associated with this character. Affects how long between characters appearing in the Textbox. Defaults to <see cref="3"/>.</param>
    /// <returns>
    /// Represents a character able to be used within a <see cref="DialogueTree"/>.
    /// </returns>
    public class Character
    {
        public string Name { get; set; }
        public Expression[] Expressions { get; set; }
        public float Scale { get; set; } = 1f;
        public string Style { get; set; } = "DialogueHelper.Content.UI.Dialogue.DialogueStyles.DefaultDialogueStyle";
        public int TextDelay { get; set; } = 3;
        public string PrimaryColor { get; set; } = null;
        public string SecondaryColor { get; set; } = null;

        public Color getPrimaryColor()
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(PrimaryColor);
            int r = Convert.ToInt16(color.R);
            int g = Convert.ToInt16(color.G);
            int b = Convert.ToInt16(color.B);
            return new Color(r, g, b);
        }
        public Color getSecondaryColor()
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(SecondaryColor);
            int r = Convert.ToInt16(color.R);
            int g = Convert.ToInt16(color.G);
            int b = Convert.ToInt16(color.B);
            return new Color(r, g, b);
        }
    }

    public class Expression
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public int FrameCount { get; set; } = 1;
        public int FrameRate { get; set; } = 0;
        public bool Loop { get; set; } = false;
        public bool HasAnimateCondition { get; set; } = false;
    }

    public class DialogueTree
    {
        public Dialogue[] Dialogues { get; set; }
        public string[] Characters { get; set; }
        public bool Important { get; set; } = true;
    }

    public class Dialogue
    {
        public string Text { get; set; }
        public Response[] Responses { get; set; } = [];
        public int CharacterIndex { get; set; } = 0;
        public int ExpressionIndex { get; set; } = 0;
        public float TextScale { get; set; } = 1.5f;
        public int TextDelay { get; set; } = -1;
        public Music Music { get; set; } = null;
    }

    public class Response
    {
        public string Title { get; set; }
        public int Heading { get; set; } = -1;
        public string TextColor { get; set; } = "#FFFFFF";
        public string TextBorderColor { get; set; } = "#000000";
        public string SwapToTreeKey { get; set; } = null;
        public bool HasRequirement { get; set; } = false;
        public ItemStack Cost { get; set; } = null;
        public bool DismissSubSpeaker { get; set; } = false;

        public Color getTextColor()
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(TextColor);
            int r = Convert.ToInt16(color.R);
            int g = Convert.ToInt16(color.G);
            int b = Convert.ToInt16(color.B);
            return new Color(r, g, b);
        }
        public Color getTextBorderColor()
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(TextBorderColor);
            int r = Convert.ToInt16(color.R);
            int g = Convert.ToInt16(color.G);
            int b = Convert.ToInt16(color.B);
            return new Color(r, g, b);
        }
    }
    public class Music
    {
        public string ModName { get; set; }
        public string FilePath { get; set; }
    }
    public class ItemStack
    {
        public int ItemID { get; set; } = -1;
        public string SourceMod { get; set; } = null;
        public string ItemName { get; set; } = null;
        public int Stack { get; set; } = 1;

        public int FetchItemID()
        {
            if (ItemID != -1)
                return ItemID;
            Mod mod = ModLoader.GetMod(SourceMod);
            if (!mod.TryFind<ModItem>(ItemName, out ModItem item))
                return -1;
            return item.Type;
        }
    }
    #endregion
}