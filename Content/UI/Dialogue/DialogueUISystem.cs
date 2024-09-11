using DialogueHelper.Content.UI.Dialogue.DialogueStyles;
using Terraria.UI;

namespace DialogueHelper.Content.UI.Dialogue
{
    public static class DialogueHolder
    {
        public static Dictionary<string, Character> Characters = [];

        public static Dictionary<string, string> CharacterAssetPathes = [];

        public static Dictionary<string, DialogueTree> DialogueTrees = [];
    }

    public delegate void DialogueNotifier(string treeKey, int dialogueID, int buttonID);

    public class DialogueUISystem : ModSystem
    {
        public DialogueUIState DialogueUIState;

        public UserInterface DialogueUI;

        public Character? CurrentSpeaker = null;

        public Character? SubSpeaker = null;

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

        public int subSpeakerIndex = -1;

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

        public void DisplayDialogueTree(string TreeKey, int DialogueIndex = 0)
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
            DialogueTree currentTree = DialogueHolder.DialogueTrees[TreeKey];
            Dialogue currentDialogue = currentTree.Dialogues[DialogueIndex];

            CurrentSpeaker = DialogueHolder.Characters[currentTree.Characters[currentDialogue.CharacterID]];
            SubSpeaker = null;
            subSpeakerIndex = -1;
            Main.NewText(Main.screenWidth);

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

        public void UpdateDialogueUI(string TreeKey, int DialogueIndex)
        {
            int formerSpeakerIndex = DialogueHolder.DialogueTrees[DialogueUIState.TreeKey].Dialogues[DialogueUIState.DialogueIndex].CharacterID;
            DialogueTree currentTree = DialogueHolder.DialogueTrees[TreeKey];

            Dialogue currentDialogue = currentTree.Dialogues[DialogueIndex];
            //Main.NewText("Correct Speaker ID: " + currentTree.Characters[currentDialogue.CharacterIndex].ID);
            //Main.NewText("Current ID: " + CurrentSpeaker.ID);
            //Main.NewText("Subspeaker ID: " + SubSpeaker.ID);

            if (DialogueHolder.Characters[currentTree.Characters[currentDialogue.CharacterID]].Style != ((Character)CurrentSpeaker).Style)
                swappingStyle = true;

            if (currentTree.Characters[currentDialogue.CharacterID] == DialogueHolder.Characters.First(c => c.Value == CurrentSpeaker).Key)
            {
                Main.NewText("Speaker Unchanged");
                newSpeaker = false;
                newSubSpeaker = false;
                returningSpeaker = false;
            }
            else if (SubSpeaker == null)
            {
                Main.NewText("New speaker! No subspeaker");
                newSpeaker = true;
                newSubSpeaker = true;
                returningSpeaker = false;
                SubSpeaker = CurrentSpeaker;
                subSpeakerIndex = formerSpeakerIndex;
                CurrentSpeaker = DialogueHolder.Characters[currentTree.Characters[currentDialogue.CharacterID]];
                speakerRight = !speakerRight;
            }
            else
            {
                Main.NewText("New speaker! Yes subspeaker.");
                newSpeaker = false;
                newSubSpeaker = true;
                returningSpeaker = true;
                Character temp = (Character)SubSpeaker;
                SubSpeaker = CurrentSpeaker;
                subSpeakerIndex = formerSpeakerIndex;
                CurrentSpeaker = temp;
                speakerRight = !speakerRight;
            }


            justOpened = false;
            DialogueUI?.SetState(null);
            DialogueUI = new();
            DialogueUIState = new()
            {
                TreeKey = TreeKey,
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
    public struct Character(Expression[] expressions, string name, float scale = 2f, Type style = null, int textDelay = 3, Color? primaryColor = null, Color? secondaryColor = null)
    {
        public string Name = name;
        public float Scale = scale;
        public Expression[] Expressions = expressions;
        public Type Style = style ?? typeof(DefaultDialogueStyle);
        public int TextDelay = textDelay;
        public Color? PrimaryColor = primaryColor;
        public Color? SecondaryColor = secondaryColor;

        public static bool operator ==(Character c1, Character c2) => c1.Equals(c2);

        public static bool operator !=(Character c1, Character c2) => !c1.Equals(c2);

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(Character)) return false;
            if (((Character)obj).Name != Name) return false;
            if (((Character)obj).Scale != Scale) return false;
            if (((Character)obj).Expressions != Expressions) return false;
            if (((Character)obj).Style != Style) return false;
            return true;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }

    /// <param name="dialogues">The array of <see cref="Dialogue"/>s the Tree manages.</param>
    /// <param name="characters">The array of <see cref="Character"/>s the Tree is able to use.</param>
    /// <returns>
    /// Represents a Dialogue Tree able to be displayed via the <see cref="DialogueUISystem.DisplayDialogueTree(int, int)"/> function./>s.
    /// </returns>
    public struct DialogueTree(Dialogue[] dialogues, string[] characters, string localizationPath)
    {
        public Dialogue[] Dialogues = dialogues;
        public string[] Characters = characters;
        public string LocalizationPath = localizationPath;
    }

    /// <param name="responses">The array of <see cref="Response"/>s the player can give. If set to null, clicking on the Textbox itself will proceed to the next Dialogue within the <see cref="DialogueTree"/> or close the Dialogue if there are no more dialogues Defaults to <see cref="null"/>. </param>
    /// <param name="characterIndex">The index of a character within the <see cref="DialogueTree"/>'s <see cref="DialogueTree.Characters"/> array. Represents the character who will be speaking. Defaults to <see cref="0"/>, the first character in the <see cref="DialogueTree.Characters"/> array.</param>
    /// <param name="expressionIndex">The index of an expression within a <see cref="Character"/>'s <see cref="Character.Expressions"/> array. Represents the expression, or asset, the character will use while speaking. Defaults to <see cref="0"/>, the first expression in the <see cref="Character.Expressions"/> array.</param>
    /// <param name="textScaleX">Scales the size of the text horizontally. Defaults to <see cref="1.5f"/>.</param>
    /// <param name="textScaleY">Scales the size of the text vertically. Defaults to <see cref="1.5f"/>.</param>
    /// <param name="textDelay">The Text Delay associated with this character. Affects how long between characters appearing in the Textbox. Defaults to <see cref="-1"/>, causing it to use the delay associated with the current <see cref="Character"/> if no Character is speaking, it defaults to <see cref="3"/>.</param>
    /// <param name="musicID">The ID of the music that will play during this dialogue. This shouldn't change too often throughout a tree. Defaults to <see cref="-1"/>, which wont interupt the current background music. </param>
    /// <returns>
    /// Represents a single dialogue state within a <see cref="DialogueTree"/>.
    /// </returns>
    public struct Dialogue(Response[] responses = null, int characterIndex = 0, int expressionIndex = 0, float textScaleX = 1.5f, float textScaleY = 1.5f, int textDelay = -1, int musicID = -1)
    {
        public Response[] Responses = responses;
        public int CharacterID = characterIndex;
        public int ExpressionIndex = expressionIndex;
        public Vector2 TextScale = new(textScaleX, textScaleY);
        public int TextDelay = textDelay;
        public int MusicID = musicID;
    }

    /// <param name="title">The text displayed on the Response Button.</param>
    /// <param name="dialogueIndex">The index within the <see cref="DialogueTree.Dialogues"/> array this response leads to. Defaults to <see cref="-1"/>, which closes the dialogue.</param>
    /// <param name="requirement">A <see cref="Boolean"/> which determines whether or not this response can appear as an option Defaults to <see cref="true"/>.</param>
    /// <param name="cost">A <see cref="ItemStack"/> which applys a cost that is needed in order to select that response Defaults to <see cref="null"/>, meaning there is no cost.</param>
    /// <param name="dismissSubSpeaker">A <see cref="Boolean"/> which will remove the current SubSpeaker, if there is one, from the dialogue when this response is selected.</param>
    /// <returns>
    /// Represents a response the player is able to give to a <see cref="Dialogue"/>./>s.
    /// </returns>
    public struct Response(string title, int dialogueIndex = -1, bool requirement = true, ItemStack? cost = null, bool dismissSubSpeaker = false, bool localize = true)
    {
        public string Title = title;
        public bool Localize = localize;
        public int DialogueIndex = dialogueIndex;
        public bool Requirement = requirement;
        public ItemStack? Cost = cost;
        public bool DismissSubSpeaker = dismissSubSpeaker;
    }

    public struct Expression(string title, int frameCount, int frameRate, bool loop = true)
    {
        public string Title = title;
        public int FrameCount = frameCount;
        public int FrameRate = frameRate;
        public bool Loop = loop;
    }

    public struct ItemStack(int id, int stack)
    {
        public int Type = id;
        public int Stack = stack;
    }
    #endregion
}