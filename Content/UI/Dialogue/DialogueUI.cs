using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using static DialogueHelper.Content.UI.Dialogue.DialogueHolder;
using Terraria.UI.Chat;
using DialogueHelper.Content.UI.Dialogue.DialogueStyles;

namespace DialogueHelper.Content.UI.Dialogue
{

    public class DialogueUIState : UIState
    {
        public class DialogueText : UIElement
        {
            public string Text = "";
            public bool crawling = true;
            internal float boxWidth = 0f;
            internal int textDelay = 10;
            internal Vector2 textScale = new(1.5f, 1.5f);
            internal int textIndex = 0;
            private int counter = -30;
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                float xResolutionScale = Main.screenWidth / 2560f;
                float yResolutionScale = Main.screenHeight / 1440f;
                Vector2 ResolutionScale = new(Main.screenWidth / 2560f, Main.screenHeight / 1440f);

                CalculatedStyle innerDimensions = GetInnerDimensions();
                float xPageTop = innerDimensions.X;
                float yPageTop = innerDimensions.Y;

                int textWidth = (int)Parent.Width.Pixels - 6;
                textWidth = (int)(textWidth * xResolutionScale);

                if (++counter % textDelay == 0 && counter >= 0)
                    textIndex++;
                List<TextSnippet> fullSnippets = ChatManager.ParseMessage(Text, Color.White);
                List<TextSnippet> printedSnippets = [];
                if (textIndex < Text.Length)
                {
                    crawling = true;
                    int textLength = 0;
                    for (int i = 0; i < fullSnippets.Count; i++)
                    {
                        string text = fullSnippets[i].TextOriginal;
                        if (text.Contains('['))
                            text = fullSnippets[i].Text;
                        textLength += text.Length;
                    }
                    if (textIndex < textLength)
                    {
                        int CurrentLength = 0;
                        for (int i = 0; i < fullSnippets.Count; i++)
                        {
                            string text = fullSnippets[i].TextOriginal;
                            if (text.Contains('['))
                                text = fullSnippets[i].Text;

                            if (CurrentLength + text.Length < textIndex)
                            {
                                CurrentLength += text.Length;
                                printedSnippets.Add(fullSnippets[i]);
                            }
                            else
                            {
                                string newText = text.Remove(textIndex - CurrentLength);
                                if (newText.Length != 0 && fullSnippets[i].Color != Color.White)
                                {
                                    List<TextSnippet> snippets = ChatManager.ParseMessage($"[C/{fullSnippets[i].Color.Hex3()}:{newText}]", Color.White);
                                    TextSnippet snippet = snippets[0];
                                    printedSnippets.Add(snippet);
                                }
                                else
                                    printedSnippets.Add(new TextSnippet(newText, Color.White));
                                break;
                            }
                        }
                    }
                    else
                    {
                        crawling = false;
                        printedSnippets = fullSnippets;
                    }

                }
                else
                {
                    crawling = false;
                    printedSnippets = fullSnippets;
                }

                string TextToPrint = "";
                for (int i = 0; i < printedSnippets.Count; i++)
                {
                    TextToPrint += printedSnippets[i].TextOriginal;
                }
                List<string> textLines = [.. Utils.WordwrapString(TextToPrint, FontAssets.MouseText.Value, (int)((Parent.Width.Pixels + Width.Pixels) / 1.5f), 250, out _)];
                textLines.RemoveAll(text => string.IsNullOrEmpty(text));

                string PreviousLineColorHex = "";
                for (int i = 0; i < textLines.Count; i++)
                {
                    int LeftBracketCounter = 0;
                    int RightBracketCounter = 0;
                    bool gettingHex = false;
                    for (int j = 0; j < textLines[i].Length; j++)
                    {
                        if (textLines[i][j] == '[')
                            LeftBracketCounter++;
                        if (textLines[i][j] == ']')
                            RightBracketCounter++;
                    }
                    if (LeftBracketCounter != RightBracketCounter)
                    {
                        //Main.NewText("Line Imbalance at line: " + i);
                        for (int j = 0; j < textLines[i].Length; j++)
                        {
                            if (LeftBracketCounter > RightBracketCounter)
                            {
                                if (gettingHex && textLines[i][j] != ':')
                                    PreviousLineColorHex += textLines[i][j];
                                if (textLines[i][j] == '/')
                                    gettingHex = true;
                                else if (textLines[i][j] == ':')
                                    gettingHex = false;
                            }
                        }
                        if (LeftBracketCounter > RightBracketCounter)
                        {
                            //Main.NewText("Closing Bracket needed!");
                            textLines[i] += ']';
                        }
                        else
                        {
                            //Main.NewText("Color Tag needed!");
                            if (PreviousLineColorHex.Length > 6)
                                PreviousLineColorHex = PreviousLineColorHex.Remove(6);
                            string newText = $"[C/{PreviousLineColorHex}:" + textLines[i];
                            //Main.NewText("Added Tag: " + PreviousLineColorHex);
                            textLines[i] = newText;
                        }
                        //Main.NewText("Updated line: " + textLines[i]);
                    }
                }

                List<List<TextSnippet>> dialogLines = [];
                for (int i = 0; i < textLines.Count; i++)
                {
                    dialogLines.Add(ChatManager.ParseMessage(textLines[i], Color.White));
                }

                float yOffsetPerLine = 32f * textScale.Y;
                int yScale = (int)(42 * yResolutionScale);
                int yScale2 = (int)(yOffsetPerLine * yResolutionScale);
                for (int i = 0; i < dialogLines.Count; i++)
                    if (dialogLines[i] != null)
                    {
                        int textDrawPositionY = yScale + i * yScale2 + (int)yPageTop;
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, [.. dialogLines[i]], new Vector2(xPageTop, textDrawPositionY), 0f, Vector2.Zero, textScale, out int hoveredSnippet);
                        //Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, dialogLines[i], xPageTop, textDrawPositionY, Color.White, Color.Black, Vector2.Zero, 1.5f);
                    }
            }
        }

        public MouseBlockingUIPanel Textbox;
        public UIImage Speaker;
        public UIImage SubSpeaker;

        public string TreeKey;
        public int DialogueIndex = 0;
        private int counter = 0;
        private int frameCounter = 1;
        public override void OnInitialize()
        {
            if (DialogueTrees.Count == 0)
                return;
            if (DialogueTrees.Count != 0)
            {
                counter = 0;

                DialogueTree CurrentTree = DialogueTrees[TreeKey];
                Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
                Character CurrentSpeaker;
                Character CurrentSubSpeaker;

                int subSpeakerIndex = -1;
                bool justOpened = true;
                bool newSpeaker = false;
                bool newSubSpeaker = false;
                bool returningSpeaker = false;
                bool speakerRight = true;

                BaseDialogueStyle style;
                if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                    style = (BaseDialogueStyle)Activator.CreateInstance(Characters[CurrentTree.Characters[ModContent.GetInstance<DialogueUISystem>().subSpeakerIndex]].Style);
                else
                    style = (BaseDialogueStyle)Activator.CreateInstance(Characters[CurrentTree.Characters[CurrentDialogue.CharacterID]].Style);

                if (ModContent.GetInstance<DialogueUISystem>() != null)
                {
                    if (ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker != null)
                        CurrentSpeaker = (Character)ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
                    if (ModContent.GetInstance<DialogueUISystem>().SubSpeaker != null)
                        CurrentSubSpeaker = (Character)ModContent.GetInstance<DialogueUISystem>().SubSpeaker;
                    subSpeakerIndex = ModContent.GetInstance<DialogueUISystem>().subSpeakerIndex;
                    justOpened = ModContent.GetInstance<DialogueUISystem>().justOpened;
                    newSpeaker = ModContent.GetInstance<DialogueUISystem>().newSpeaker;
                    newSubSpeaker = ModContent.GetInstance<DialogueUISystem>().newSubSpeaker;
                    returningSpeaker = ModContent.GetInstance<DialogueUISystem>().returningSpeaker;
                    speakerRight = ModContent.GetInstance<DialogueUISystem>().speakerRight;
                }
                else
                {
                    CurrentSpeaker = Characters[CurrentTree.Characters[CurrentDialogue.CharacterID]];
                    CurrentSubSpeaker = new Character([new()], "Bill");
                }
                style.PreUICreate(TreeKey, DialogueIndex);
                if (CurrentDialogue.CharacterID != -1)
                {
                    //Main.NewText("Create Speaker: " + CurrentDialogue.CharacterIndex);
                    CurrentSpeaker = Characters[CurrentTree.Characters[CurrentDialogue.CharacterID]];
                    string expressionID = CurrentSpeaker.Expressions[CurrentDialogue.ExpressionIndex].Title;

                    string AssetPath = CharacterAssetPathes[Characters.First(c => c.Value == CurrentSpeaker).Key.Split("/")[0].Replace("/", "")];
                    string SpeakerID = Characters.First(c => c.Value == CurrentSpeaker).Key.Split("/")[1];
                    Texture2D speakerTexture = ModContent.Request<Texture2D>($"{AssetPath}/{SpeakerID}/{SpeakerID}_{expressionID}", AssetRequestMode.ImmediateLoad).Value;
                    Rectangle speakerFrame = new(0, 0, speakerTexture.Bounds.Width, speakerTexture.Bounds.Height / CurrentSpeaker.Expressions[CurrentDialogue.ExpressionIndex].FrameCount);

                    Texture2D speakerFrameTexture = new(Main.graphics.GraphicsDevice, speakerFrame.Width, speakerFrame.Height);
                    Color[] data = new Color[speakerFrame.Width * speakerFrame.Height];
                    speakerTexture.GetData(0, speakerFrame, data, 0, data.Length);
                    speakerFrameTexture.SetData(data);
                    if (!speakerRight)
                        speakerFrameTexture = FlipTexture2D(speakerFrameTexture, false, true);
                    Speaker = new(speakerFrameTexture)
                    {
                        ImageScale = CurrentSpeaker.Scale
                    };
                    Main.NewText(returningSpeaker);                   

                    if (justOpened || newSpeaker)
                        SetRectangle(Speaker, left: 0, top: Main.screenHeight, width: speakerFrameTexture.Width, height: speakerFrameTexture.Height);
                    else
                        SetRectangle(Speaker, left: 0, top: Main.screenHeight / 2.5f, width: speakerFrameTexture.Width, height: speakerFrameTexture.Height);

                    if (speakerRight)
                        Speaker.Left.Pixels = returningSpeaker ? Main.screenWidth / 1.2f : Main.screenWidth / 1.25f;
                    else
                        Speaker.Left.Pixels = returningSpeaker ? Main.screenWidth * 0f : Main.screenWidth * 0.05f;

                    //Main.NewText(Main.screenWidth);
                    style.PreSpeakerCreate(TreeKey, DialogueIndex, Speaker);
                    Append(Speaker);
                    style.PostSpeakerCreate(TreeKey, DialogueIndex, Speaker);
                }
                if (subSpeakerIndex != -1)
                {
                    //Main.NewText("Create Sub-Speaker: " + subSpeakerIndex);
                    CurrentSubSpeaker = Characters[CurrentTree.Characters[subSpeakerIndex]];
                    string AssetPath = CharacterAssetPathes[Characters.First(c => c.Value == CurrentSubSpeaker).Key.Split("/")[0].Replace("/", "")];
                    string subSpeakerID = Characters.First(c => c.Value == CurrentSubSpeaker).Key.Split("/")[1];
                    string expressionID = CurrentSubSpeaker.Expressions[0].Title;
                    Texture2D subSpeakerTexture = ModContent.Request<Texture2D>($"{AssetPath}/{subSpeakerID}/{subSpeakerID}_{expressionID}", AssetRequestMode.ImmediateLoad).Value;
                    Rectangle subSpeakerFrame = new(0, 0, subSpeakerTexture.Bounds.Width, subSpeakerTexture.Bounds.Height / CurrentSubSpeaker.Expressions[0].FrameCount);

                    Texture2D subSpeakerFrameTexture = new(Main.graphics.GraphicsDevice, subSpeakerFrame.Width, subSpeakerFrame.Height);
                    Color[] data = new Color[subSpeakerFrame.Width * subSpeakerFrame.Height];
                    subSpeakerTexture.GetData(0, subSpeakerFrame, data, 0, data.Length);
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i].A != 0)
                        {
                            byte alpha = data[i].A;
                            data[i] *= 0.5f;
                            data[i].A = alpha;
                        }
                    }
                    subSpeakerFrameTexture.SetData(data);
                    if (speakerRight)
                        subSpeakerFrameTexture = FlipTexture2D(subSpeakerTexture, false, true);
                    SubSpeaker = new(subSpeakerFrameTexture)
                    {
                        ImageScale = CurrentSubSpeaker.Scale
                    };
                    
                    SetRectangle(SubSpeaker, left: 0, top: Main.screenHeight / 2.5f, width: subSpeakerFrameTexture.Width, height: subSpeakerFrameTexture.Height);

                    if (speakerRight)
                        SubSpeaker.Left.Pixels = newSpeaker || returningSpeaker ? Main.screenWidth * 0.15f : 0f;
                    else
                        SubSpeaker.Left.Pixels = newSpeaker || returningSpeaker ? Main.screenWidth / 1.25f : Main.screenWidth / 1.35f;

                    style.PreSubSpeakerCreate(TreeKey, DialogueIndex, Speaker, SubSpeaker);
                    Append(SubSpeaker);
                    style.PostSubSpeakerCreate(TreeKey, DialogueIndex, Speaker, SubSpeaker);
                }

                SpawnTextBox();               

                justOpened = false;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //Main.NewText(Main.screenWidth);

            DialogueTree CurrentTree = DialogueTrees[TreeKey];
            Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];

            BaseDialogueStyle style;
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                style = (BaseDialogueStyle)Activator.CreateInstance(Characters[CurrentTree.Characters[ModContent.GetInstance<DialogueUISystem>().subSpeakerIndex]].Style);
            else
                style = (BaseDialogueStyle)Activator.CreateInstance(Characters[CurrentTree.Characters[CurrentDialogue.CharacterID]].Style);

            if (ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
            {
                style.PostUpdateActive(Textbox, Speaker, SubSpeaker);
                //Main.NewText(Speaker.Left.Pixels);
                if (Speaker != null)
                {
                    float goalHeight = Main.screenHeight / 2.5f;
                    if (Speaker.Top.Pixels > goalHeight)
                    {
                        Speaker.Top.Pixels -= (Speaker.Top.Pixels - goalHeight) / 15;
                        if (Speaker.Top.Pixels - goalHeight < 1)
                            Speaker.Top.Pixels = goalHeight;
                    }
                    float goalLeft = Main.screenWidth * 0.05f;
                    float goalRight = Main.screenWidth / 1.25f;
                    //Main.NewText(ModContent.GetInstance<DialogueUISystem>().speakerRight);
                    if (ModContent.GetInstance<DialogueUISystem>().speakerRight && Speaker.Left.Pixels > goalRight)
                    {
                        Speaker.Left.Pixels -= (Speaker.Left.Pixels - goalRight) / 20;
                        if (Speaker.Left.Pixels - goalRight < 1)
                            Speaker.Left.Pixels = goalRight;
                    }
                    else if (!ModContent.GetInstance<DialogueUISystem>().speakerRight && Speaker.Left.Pixels < goalLeft)
                    {
                        Speaker.Left.Pixels += (goalLeft - Speaker.Left.Pixels) / 20;
                        if (goalLeft - Speaker.Left.Pixels < 1)
                            Speaker.Left.Pixels = goalLeft;
                    }
                    Character speakerCharacter = Characters[CurrentTree.Characters[CurrentDialogue.CharacterID]];
                    Expression currentExpression = speakerCharacter.Expressions[CurrentDialogue.ExpressionIndex];
                    if (currentExpression.FrameRate != 0 && counter % currentExpression.FrameRate == 0)
                    {
                        frameCounter++;
                        if (frameCounter > currentExpression.FrameCount)
                        {
                            if (currentExpression.Loop)
                                frameCounter = 1;
                            else
                                frameCounter = currentExpression.FrameCount;
                        }
                        string AssetPath = CharacterAssetPathes[Characters.First(c => c.Value == speakerCharacter).Key.Split("/")[0].Replace("/", "")];
                        string speakerID = Characters.First(c => c.Value == speakerCharacter).Key.Split("/")[1];

                        Texture2D speakerTexture = ModContent.Request<Texture2D>($"{AssetPath}/{speakerID}/{speakerID}_{currentExpression.Title}", AssetRequestMode.ImmediateLoad).Value;
                        Rectangle speakerFrame = speakerTexture.Frame(1, speakerCharacter.Expressions[CurrentDialogue.ExpressionIndex].FrameCount, 0, frameCounter - 1);

                        Texture2D speakerFrameTexture = new(Main.graphics.GraphicsDevice, speakerFrame.Width, speakerFrame.Height);
                        Color[] data = new Color[speakerFrame.Width * speakerFrame.Height];
                        speakerTexture.GetData(0, speakerFrame, data, 0, data.Length);
                        speakerFrameTexture.SetData(data);

                        Speaker.SetImage(speakerFrameTexture);
                    }
                }
                if (SubSpeaker != null)
                {
                    if (ModContent.GetInstance<DialogueUISystem>().dismissSubSpeaker)
                    {
                        float goalRight = Main.screenWidth + SubSpeaker.Width.Pixels;
                        float goalLeft = -SubSpeaker.Width.Pixels;

                        if (!ModContent.GetInstance<DialogueUISystem>().speakerRight && SubSpeaker.Left.Pixels < goalRight)
                        {
                            SubSpeaker.Left.Pixels += (goalRight - SubSpeaker.Left.Pixels) / 20;
                            if (goalRight - SubSpeaker.Left.Pixels < 10)
                                SubSpeaker.Left.Pixels = goalRight;
                        }
                        else if (ModContent.GetInstance<DialogueUISystem>().speakerRight && SubSpeaker.Left.Pixels > goalLeft)
                        {
                            SubSpeaker.Left.Pixels -= (goalLeft - SubSpeaker.Left.Pixels) / 20;
                            if (goalLeft - SubSpeaker.Left.Pixels < 10)
                                SubSpeaker.Left.Pixels = goalLeft;
                        }
                        if (SubSpeaker.Left.Pixels <= goalLeft || SubSpeaker.Left.Pixels >= goalRight)
                        {
                            ModContent.GetInstance<DialogueUISystem>().dismissSubSpeaker = false;
                            SubSpeaker.Remove();
                            SubSpeaker = null;
                            ModContent.GetInstance<DialogueUISystem>().SubSpeaker = null;
                        }
                    }
                    else
                    {
                        float goalLeft = 0f;
                        float goalRight = Main.screenWidth / 1.2f;

                        if (ModContent.GetInstance<DialogueUISystem>().speakerRight && SubSpeaker.Left.Pixels > 0f - Main.screenWidth * 0.1f)
                        {
                            SubSpeaker.Left.Pixels -= (SubSpeaker.Left.Pixels - goalLeft) / 20;
                            if (SubSpeaker.Left.Pixels - goalLeft < 1)
                                SubSpeaker.Left.Pixels = goalLeft;
                        }
                        else if (!ModContent.GetInstance<DialogueUISystem>().speakerRight && SubSpeaker.Left.Pixels < Main.screenWidth * 1.1f)
                        {
                            SubSpeaker.Left.Pixels += (goalRight - SubSpeaker.Left.Pixels) / 20;
                            if (goalRight - SubSpeaker.Left.Pixels < 1)
                                SubSpeaker.Left.Pixels = goalRight;
                        }
                    }
                }
            }
            else
            {
                style.PostUpdateClosing(Textbox, Speaker, SubSpeaker);

                float goalRight = Main.screenWidth + Speaker.Width.Pixels;
                float goalLeft = -Speaker.Width.Pixels * 2;

                if (Speaker != null)
                {
                    if (ModContent.GetInstance<DialogueUISystem>().speakerRight && Speaker.Left.Pixels < goalRight)
                    {
                        Speaker.Left.Pixels += (goalRight - Speaker.Left.Pixels) / 20;
                        if (goalRight - Speaker.Left.Pixels < 10)
                            Speaker.Left.Pixels = goalRight;
                    }
                    else if (!ModContent.GetInstance<DialogueUISystem>().speakerRight && Speaker.Left.Pixels > goalLeft)
                    {
                        Speaker.Left.Pixels -= (Speaker.Left.Pixels - goalLeft) / 20;
                        if (Speaker.Left.Pixels - goalLeft < 10)
                            Speaker.Left.Pixels = goalLeft;
                    }
                }
                if (SubSpeaker != null)
                {
                    if (!ModContent.GetInstance<DialogueUISystem>().speakerRight && SubSpeaker.Left.Pixels < goalRight)
                    {
                        SubSpeaker.Left.Pixels += (goalRight - SubSpeaker.Left.Pixels) / 20;
                        if (goalRight - SubSpeaker.Left.Pixels < 10)
                            SubSpeaker.Left.Pixels = goalRight;
                    }
                    else if (ModContent.GetInstance<DialogueUISystem>().speakerRight && SubSpeaker.Left.Pixels > goalLeft)
                    {
                        SubSpeaker.Left.Pixels -= (goalLeft - SubSpeaker.Left.Pixels) / 20;
                        if (goalLeft - SubSpeaker.Left.Pixels < 10)
                            SubSpeaker.Left.Pixels = goalLeft;
                    }
                }
                if
                (
                    (Speaker == null || Speaker.Left.Pixels >= goalRight && ModContent.GetInstance<DialogueUISystem>().speakerRight || Speaker.Left.Pixels <= goalLeft && !ModContent.GetInstance<DialogueUISystem>().speakerRight)
                    &&
                    (SubSpeaker == null || SubSpeaker.Left.Pixels >= goalRight && !ModContent.GetInstance<DialogueUISystem>().speakerRight || SubSpeaker.Left.Pixels <= goalLeft && ModContent.GetInstance<DialogueUISystem>().speakerRight)
                    &&
                    style.TextboxOffScreen(Textbox)
                )
                    ModContent.GetInstance<DialogueUISystem>().HideDialogueUI();
            }
            counter++;
        }
        public static void SetRectangle(UIElement uiElement, float left, float top, float width, float height)
        {
            uiElement.Left.Set(left, 0f);
            uiElement.Top.Set(top, 0f);
            uiElement.Width.Set(width, 0f);
            uiElement.Height.Set(height, 0f);
        }
        internal void OnBoxClick(UIMouseEvent evt, UIElement listeningElement)
        {
            DialogueText dialogue = (DialogueText)Textbox.Children.Where(c => c.GetType() == typeof(DialogueText)).First();
            DialogueTree CurrentTree = DialogueTrees[TreeKey];
            if (CurrentTree.Dialogues[DialogueIndex].Responses == null && !dialogue.crawling)
            {
                ModContent.GetInstance<DialogueUISystem>().ButtonClick?.Invoke(TreeKey, DialogueIndex, 0);

                if (DialogueIndex + 1 >= CurrentTree.Dialogues.Length)
                {
                    ModContent.GetInstance<DialogueUISystem>().isDialogueOpen = false;
                    ModContent.GetInstance<DialogueUISystem>().DialogueClose?.Invoke(TreeKey, DialogueIndex, 0);
                }
                else
                {
                    ModContent.GetInstance<DialogueUISystem>().UpdateDialogueUI(TreeKey, DialogueIndex + 1);
                }
            }
            else if (dialogue.crawling)
            {
                string key = TreeKey.Split("/")[1];
                dialogue.textIndex = Language.GetTextValue(CurrentTree.LocalizationPath + key + ".Messages." + DialogueIndex).Length;
            }
        }
        internal void OnButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            DialogueTree CurrentTree = DialogueTrees[TreeKey];
            int responseCount = CurrentTree.Dialogues[DialogueIndex].Responses.Length;
            UIText text = (UIText)listeningElement.Children.ToArray().First();
            int buttonID = 0;
            for (int i = 0; i < responseCount; i++)
            {
                if (CurrentTree.Dialogues[DialogueIndex].Responses[i].Localize)
                {
                    string key = TreeKey.Split("/")[1];
                    if (text.Text == Language.GetTextValue(CurrentTree.LocalizationPath + key + ".Responses." + CurrentTree.Dialogues[DialogueIndex].Responses[i].Title))
                        buttonID = i;
                }
                else
                {
                    if (text.Text == CurrentTree.Dialogues[DialogueIndex].Responses[i].Title)
                        buttonID = i;
                }
            }
            Response response = CurrentTree.Dialogues[DialogueIndex].Responses[buttonID];
            if (response.Cost == null || CanAffordCost(Main.LocalPlayer, response.Cost.Value))
            {
                ModContent.GetInstance<DialogueUISystem>().ButtonClick?.Invoke(TreeKey, DialogueIndex, buttonID);

                if (response.DismissSubSpeaker)
                    ModContent.GetInstance<DialogueUISystem>().dismissSubSpeaker = true;

                int heading = response.DialogueIndex;
                if (heading == -1 || heading == -2 && !(CurrentTree.Dialogues.Length > DialogueIndex + 1))
                {
                    ModContent.GetInstance<DialogueUISystem>().isDialogueOpen = false;
                    ModContent.GetInstance<DialogueUISystem>().DialogueClose?.Invoke(TreeKey, DialogueIndex, buttonID);
                }
                else if (heading == -2 && CurrentTree.Dialogues.Length > DialogueIndex + 1)
                    ModContent.GetInstance<DialogueUISystem>().UpdateDialogueUI(TreeKey, DialogueIndex + 1);
                else
                    ModContent.GetInstance<DialogueUISystem>().UpdateDialogueUI(TreeKey, heading);
            }
        }
        public void SpawnTextBox()
        {
            BaseDialogueStyle style;
            float xResolutionScale = Main.screenWidth / 2560f;
            float yResolutionScale = Main.screenHeight / 1440f;

            DialogueTree CurrentTree = DialogueTrees[TreeKey];
            Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
            Character CurrentCharacter = Characters[CurrentTree.Characters[CurrentDialogue.CharacterID]];

            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                style = (BaseDialogueStyle)Activator.CreateInstance(Characters[CurrentTree.Characters[ModContent.GetInstance<DialogueUISystem>().subSpeakerIndex]].Style);
            else
                style = (BaseDialogueStyle)Activator.CreateInstance(Characters[CurrentTree.Characters[CurrentDialogue.CharacterID]].Style);
            Textbox = new MouseBlockingUIPanel();
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            {
                Character FormerCharacter = Characters[CurrentTree.Characters[ModContent.GetInstance<DialogueUISystem>().subSpeakerIndex]];
                if (style.BackgroundColor.HasValue)
                    Textbox.BackgroundColor = style.BackgroundColor.Value;
                else if (FormerCharacter.PrimaryColor.HasValue)
                    Textbox.BackgroundColor = FormerCharacter.PrimaryColor.Value;
                else
                    Textbox.BackgroundColor = new Color(73, 94, 171);

                if (style.BackgroundBorderColor.HasValue)
                    Textbox.BorderColor = style.BackgroundBorderColor.Value;
                else if (FormerCharacter.SecondaryColor.HasValue)
                    Textbox.BorderColor = FormerCharacter.SecondaryColor.Value;
                else
                    Textbox.BorderColor = Color.Black;
            }
            else
            {
                if (style.BackgroundColor.HasValue)
                    Textbox.BackgroundColor = style.BackgroundColor.Value;
                else if (CurrentCharacter.PrimaryColor.HasValue)
                    Textbox.BackgroundColor = CurrentCharacter.PrimaryColor.Value;
                else
                    Textbox.BackgroundColor = new Color(73, 94, 171);

                if (style.BackgroundBorderColor.HasValue)
                    Textbox.BorderColor = style.BackgroundBorderColor.Value;
                else if (CurrentCharacter.SecondaryColor.HasValue)
                    Textbox.BorderColor = CurrentCharacter.SecondaryColor.Value;
                else
                    Textbox.BorderColor = Color.Black;
            }
            style.OnTextboxCreate(Textbox, Speaker, SubSpeaker);
            Textbox.OnLeftClick += OnBoxClick;
            Append(Textbox);
            if (!ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            {
                string key = TreeKey.Split("/")[1];
                DialogueText DialogueText = new()
                {
                    boxWidth = Textbox.Width.Pixels,
                    Text = Language.GetTextValue(CurrentTree.LocalizationPath + key + ".Messages." + DialogueIndex)
                };
                if (CurrentDialogue.TextDelay > 0)
                    DialogueText.textDelay = CurrentDialogue.TextDelay;
                else if (CurrentCharacter.TextDelay > 0)
                    DialogueText.textDelay = CurrentCharacter.TextDelay;
                else
                    DialogueText.textDelay = 3;
                style.OnDialogueTextCreate(DialogueText);
                Textbox.Append(DialogueText);

                if (CurrentDialogue.Responses != null)
                {
                    Response[] availableResponses = CurrentDialogue.Responses.Where(r => r.Requirement).ToArray();
                    int responseCount = availableResponses.Length;

                    for (int i = 0; i < responseCount; i++)
                    {
                        UIPanel button = new();
                        Color color;

                        if (style.ButtonColor.HasValue)
                            color = style.ButtonColor.Value;
                        else if (CurrentCharacter.PrimaryColor.HasValue)
                            color = CurrentCharacter.PrimaryColor.Value;
                        else
                            color = new Color(73, 94, 171);
                        color.A = 125;
                        button.BackgroundColor = color;

                        if (style.ButtonBorderColor.HasValue)
                            button.BorderColor = style.ButtonBorderColor.Value;
                        else if (CurrentCharacter.SecondaryColor.HasValue)
                            button.BorderColor = CurrentCharacter.SecondaryColor.Value;
                        else
                            button.BorderColor = Color.Black;

                        style.OnResponseButtonCreate(button, Textbox, responseCount, i);
                        button.OnLeftClick += OnButtonClick;
                        Append(button);

                        UIText text;

                        key = TreeKey.Split("/")[1];
                        if (availableResponses[i].Localize)
                            text = new(Language.GetTextValue(CurrentTree.LocalizationPath + key + ".Responses." + availableResponses[i].Title), 0f);
                        else
                            text = new(availableResponses[i].Title, 0f);

                        text.Width.Pixels = style.ButtonSize.X;
                        text.IsWrapped = true;
                        text.WrappedTextBottomPadding = 0.1f;
                        style.OnResponseTextCreate(text);
                        button.Append(text);
                        if (availableResponses[i].Cost != null)
                        {
                            ItemStack cost = (ItemStack)availableResponses[i].Cost;
                            UIPanel costHolder = new()
                            {
                                BorderColor = Color.Transparent,
                                BackgroundColor = Color.Transparent,
                                VAlign = 0.75f
                            };

                            UIText stackText = new($"x{cost.Stack}")
                            {
                                HAlign = 1f,
                                VAlign = 0.5f
                            };

                            Texture2D itemTexture = (Texture2D)ModContent.Request<Texture2D>(ItemLoader.GetItem(cost.Type).Texture);
                            UIImage itemIcon = new(itemTexture);
                            itemIcon.Width.Pixels = itemTexture.Width;
                            itemIcon.Height.Pixels = itemTexture.Height;
                            itemIcon.ImageScale = 18f / itemIcon.Height.Pixels;

                            itemIcon.Top.Pixels -= itemIcon.Height.Pixels / 2;
                            itemIcon.Left.Pixels -= itemIcon.Width.Pixels / 2;

                            costHolder.Height.Pixels = 18f > stackText.Height.Pixels ? 24f : stackText.Height.Pixels;
                            costHolder.Width.Pixels = itemIcon.Width.Pixels * itemIcon.ImageScale + 15 * stackText.Text.Length;

                            costHolder.Append(itemIcon);
                            costHolder.Append(stackText);

                            style.OnResponseCostCreate(text, costHolder);

                            button.Append(costHolder);
                        }
                    }
                }

                style.PostUICreate(TreeKey, DialogueIndex, Textbox, Speaker, SubSpeaker);
                ModContent.GetInstance<DialogueUISystem>().styleSwapped = false;
            }
            else
                style.PostUICreate(TreeKey, DialogueIndex, Textbox, Speaker, SubSpeaker);
        }
        private static bool CanAffordCost(Player player, ItemStack price)
        {
            int amount = price.Stack;
            foreach (Item item in player.inventory.Where(i => i.type == price.Type))
            {
                if (item.stack >= amount)
                {
                    amount = 0;
                    break;
                }
                else
                    amount -= item.stack;
            }
            if (amount == 0)
            {
                foreach (Item item in player.inventory.Where(i => i.type == price.Type))
                {
                    amount = price.Stack;
                    if (item.stack >= amount)
                    {
                        item.stack -= amount;
                        amount = 0;
                        break;
                    }
                    else
                    {
                        amount -= item.stack;
                        item.stack = 0;
                    }
                }
                return true;
            }
            else
                return false;
        }
        private static Texture2D FlipTexture2D(Texture2D input, bool vertical, bool horizontal)
        {
            Texture2D flipped = new(input.GraphicsDevice, input.Width, input.Height);
            Color[] data = new Color[input.Width * input.Height];
            Color[] flipped_data = new Color[data.Length];

            input.GetData<Color>(data);

            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    int index = 0;
                    if (horizontal && vertical)
                        index = input.Width - 1 - x + (input.Height - 1 - y) * input.Width;
                    else if (horizontal && !vertical)
                        index = input.Width - 1 - x + y * input.Width;
                    else if (!horizontal && vertical)
                        index = x + (input.Height - 1 - y) * input.Width;
                    else if (!horizontal && !vertical)
                        index = x + y * input.Width;

                    flipped_data[x + y * input.Width] = data[index];
                }
            }

            flipped.SetData<Color>(flipped_data);

            return flipped;
        }
    }
}