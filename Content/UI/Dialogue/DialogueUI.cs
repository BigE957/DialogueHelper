using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;
using DialogueHelper.Content.UI.Dialogue.DialogueStyles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

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
                List<string> textLines = [.. Utils.WordwrapString(TextToPrint, FontAssets.MouseText.Value, (int)((Parent.Width.Pixels + Width.Pixels) / 1.55f), 250, out _)];
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

        public delegate bool DialogueNotifier(string treeKey, int dialogueID, int buttonID);
        public DialogueNotifier ResponseRequirementCheck;

        public delegate bool CharacterNotifier(string characterName, string expressionName);
        public CharacterNotifier AnimationConditionCheck;

        public string TreeKey;
        public int DialogueIndex = 0;
        private int counter = 0;
        private int frameCounter = 1;
        public override void OnInitialize()
        {
            if (ModContent.GetInstance<DialogueUISystem>() == null)
                return;
            else
            {
                counter = 0;

                DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
                Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
                Character CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
                Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;

                bool justOpened = true;
                bool newSpeaker = false;
                bool newSubSpeaker = false;
                bool returningSpeaker = false;
                bool speakerRight = true;

                if (ModContent.GetInstance<DialogueUISystem>() != null)
                {
                    if (ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker != null)
                        CurrentCharacter = (Character)ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
                    if (ModContent.GetInstance<DialogueUISystem>().SubSpeaker != null)
                        FormerCharacter = (Character)ModContent.GetInstance<DialogueUISystem>().SubSpeaker;
                    justOpened = ModContent.GetInstance<DialogueUISystem>().justOpened;
                    newSpeaker = ModContent.GetInstance<DialogueUISystem>().newSpeaker;
                    newSubSpeaker = ModContent.GetInstance<DialogueUISystem>().newSubSpeaker;
                    returningSpeaker = ModContent.GetInstance<DialogueUISystem>().returningSpeaker;
                    speakerRight = ModContent.GetInstance<DialogueUISystem>().speakerRight;
                }
                else
                {
                    CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
                    FormerCharacter = null;
                }

                BaseDialogueStyle style;
                if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                    style = (BaseDialogueStyle)Activator.CreateInstance(Type.GetType(FormerCharacter.Style));
                else
                    style = (BaseDialogueStyle)Activator.CreateInstance(Type.GetType(CurrentCharacter.Style) ?? typeof(DefaultDialogueStyle));

                style.PreUICreate(DialogueIndex);
                if (CurrentDialogue.CharacterIndex != -1)
                {
                    //Main.NewText("Create Speaker: " + CurrentDialogue.CharacterIndex);
                    CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;

                    Texture2D speakerTexture = ModContent.Request<Texture2D>(CurrentCharacter.Expressions[CurrentDialogue.ExpressionIndex].Path, AssetRequestMode.ImmediateLoad).Value;
                    Rectangle speakerFrame = new(0, 0, speakerTexture.Bounds.Width, speakerTexture.Bounds.Height / CurrentCharacter.Expressions[CurrentDialogue.ExpressionIndex].FrameCount);

                    Texture2D speakerFrameTexture = new(Main.graphics.GraphicsDevice, speakerFrame.Width, speakerFrame.Height);
                    Color[] data = new Color[speakerFrame.Width * speakerFrame.Height];
                    speakerTexture.GetData(0, speakerFrame, data, 0, data.Length);
                    speakerFrameTexture.SetData(data);
                    if (!speakerRight)
                        speakerFrameTexture = FlipTexture2D(speakerFrameTexture, false, true);
                    Speaker = new(speakerFrameTexture)
                    {
                        ImageScale = CurrentCharacter.Scale
                    };

                    if (justOpened || newSpeaker)
                        SetRectangle(Speaker, left: 0, top: Main.screenHeight, width: speakerFrameTexture.Width, height: speakerFrameTexture.Height);
                    else
                        SetRectangle(Speaker, left: 0, top: Main.screenHeight - Speaker.Height.Pixels + 16, width: speakerFrameTexture.Width, height: speakerFrameTexture.Height);

                    if (speakerRight)
                        Speaker.Left.Pixels = returningSpeaker ? Main.screenWidth / 1.2f - (Speaker.Width.Pixels / 2f) : Main.screenWidth / 1.25f - (Speaker.Width.Pixels / 2f);
                    else
                        Speaker.Left.Pixels = returningSpeaker ? 0f + (Speaker.Width.Pixels / 2f) : Main.screenWidth * 0.05f + (Speaker.Width.Pixels / 2f);

                    //Main.NewText(Main.screenWidth);
                    style.PreSpeakerCreate(DialogueIndex, Speaker);
                    Append(Speaker);
                    style.PostSpeakerCreate(DialogueIndex, Speaker);
                }
                if (FormerCharacter != null)
                {
                    //Main.NewText("Create Sub-Speaker: " + subSpeakerIndex);
                    Texture2D subSpeakerTexture = ModContent.Request<Texture2D>(FormerCharacter.Expressions[0].Path, AssetRequestMode.ImmediateLoad).Value;
                    Rectangle subSpeakerFrame = new(0, 0, subSpeakerTexture.Bounds.Width, subSpeakerTexture.Bounds.Height / FormerCharacter.Expressions[0].FrameCount);

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
                        ImageScale = FormerCharacter.Scale
                    };

                    SetRectangle(SubSpeaker, left: 0, top: Main.screenHeight - SubSpeaker.Height.Pixels + 16, width: subSpeakerFrameTexture.Width, height: subSpeakerFrameTexture.Height);

                    if (speakerRight)
                        SubSpeaker.Left.Pixels = newSpeaker || returningSpeaker ? Main.screenWidth * 0.05f + (SubSpeaker.Width.Pixels / 2f) : 0f + (SubSpeaker.Width.Pixels / 2f);
                    else
                        SubSpeaker.Left.Pixels = newSpeaker || returningSpeaker ? Main.screenWidth / 1.25f - (SubSpeaker.Width.Pixels / 2f) : Main.screenWidth / 1.35f - (SubSpeaker.Width.Pixels / 2f);

                    style.PreSubSpeakerCreate(DialogueIndex, Speaker, SubSpeaker);
                    Append(SubSpeaker);
                    style.PostSubSpeakerCreate(DialogueIndex, Speaker, SubSpeaker);
                }

                SpawnTextBox();

                justOpened = false;
            }
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //Main.NewText(Main.screenWidth);

            DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
            Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
            Character CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
            Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;

            BaseDialogueStyle style;
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                style = (BaseDialogueStyle)Activator.CreateInstance(Type.GetType(FormerCharacter.Style));
            else
                style = (BaseDialogueStyle)Activator.CreateInstance(Type.GetType(CurrentCharacter.Style) ?? typeof(DefaultDialogueStyle));

            if (ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
            {
                style.PostUpdateActive(Textbox, Speaker, SubSpeaker);
                //Main.NewText(Speaker.Left.Pixels);
                if (Speaker != null)
                {
                    float goalHeight = Main.screenHeight - Speaker.Height.Pixels + 16;
                    if (Speaker.Top.Pixels > goalHeight)
                    {
                        Speaker.Top.Pixels -= (Speaker.Top.Pixels - goalHeight) / 15;
                        if (Speaker.Top.Pixels - goalHeight < 1)
                            Speaker.Top.Pixels = goalHeight;
                    }
                    float goalLeft = Main.screenWidth * 0.05f + (Speaker.Width.Pixels / 2f);
                    float goalRight = Main.screenWidth / 1.25f - (Speaker.Width.Pixels / 2f);
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
                    Expression currentExpression = CurrentCharacter.Expressions[CurrentDialogue.ExpressionIndex];
                    if (currentExpression.FrameCount != 1 && currentExpression.FrameRate != 0 && (!currentExpression.HasAnimateCondition || AnimationConditionCheck.Invoke(CurrentCharacter.Name, currentExpression.Title)) && counter % currentExpression.FrameRate == 0)
                    {
                        frameCounter++;
                        if (frameCounter > currentExpression.FrameCount)
                        {
                            if (currentExpression.Loop)
                                frameCounter = 1;
                            else
                                frameCounter = currentExpression.FrameCount;
                        }

                        Texture2D speakerTexture = ModContent.Request<Texture2D>(currentExpression.Path, AssetRequestMode.ImmediateLoad).Value;
                        Rectangle speakerFrame = speakerTexture.Frame(1, CurrentCharacter.Expressions[CurrentDialogue.ExpressionIndex].FrameCount, 0, frameCounter - 1);

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
                        float goalLeft = -SubSpeaker.Width.Pixels * 2;

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
                        float goalLeft = 0f + SubSpeaker.Width.Pixels / 2f;
                        float goalRight = Main.screenWidth / 1.2f - SubSpeaker.Width.Pixels / 2f;

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
            DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
            Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
            if (CurrentDialogue.Responses == null && !dialogue.crawling)
            {
                ModContent.GetInstance<DialogueUISystem>().ButtonClick?.Invoke(TreeKey, DialogueIndex, 0);

                if (DialogueIndex + 1 >= CurrentTree.Dialogues.Length)
                {
                    ModContent.GetInstance<DialogueUISystem>().isDialogueOpen = false;
                    ModContent.GetInstance<DialogueUISystem>().DialogueClose?.Invoke(TreeKey, DialogueIndex, 0);
                }
                else
                    ModContent.GetInstance<DialogueUISystem>().UpdateDialogueUI(TreeKey, DialogueIndex + 1);
            }
            else if (dialogue.crawling)
                dialogue.textIndex = CurrentDialogue.Text.Length;
        }
        internal void OnButtonClick(UIMouseEvent evt, UIElement listeningElement)
        {
            DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
            int responseCount = CurrentTree.Dialogues[DialogueIndex].Responses.Length;
            UIText text = (UIText)listeningElement.Children.ToArray().First();
            int buttonID = 0;
            for (int i = 0; i < responseCount; i++)
            {
                if (text.Text == CurrentTree.Dialogues[DialogueIndex].Responses[i].Title)
                    buttonID = i;
            }
            Response response = CurrentTree.Dialogues[DialogueIndex].Responses[buttonID];
            if (response.Cost == null || CanAffordCost(Main.LocalPlayer, response.Cost))
            {
                ModContent.GetInstance<DialogueUISystem>().ButtonClick?.Invoke(TreeKey, DialogueIndex, buttonID);

                if (response.DismissSubSpeaker)
                    ModContent.GetInstance<DialogueUISystem>().dismissSubSpeaker = true;

                int heading = response.Heading;
                if (heading == -1 || heading == -2 && !(CurrentTree.Dialogues.Length > DialogueIndex + 1))
                {
                    ModContent.GetInstance<DialogueUISystem>().isDialogueOpen = false;
                    ModContent.GetInstance<DialogueUISystem>().DialogueClose?.Invoke(TreeKey, DialogueIndex, buttonID);
                }
                else if (heading == -2 && CurrentTree.Dialogues.Length > DialogueIndex + 1)
                    ModContent.GetInstance<DialogueUISystem>().UpdateDialogueUI(response.SwapToTreeKey ?? TreeKey, DialogueIndex + 1);
                else
                    ModContent.GetInstance<DialogueUISystem>().UpdateDialogueUI(response.SwapToTreeKey ?? TreeKey, heading);
            }
        }
        public void SpawnTextBox()
        {
            BaseDialogueStyle style;
            float xResolutionScale = Main.screenWidth / 2560f;
            float yResolutionScale = Main.screenHeight / 1440f;

            DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
            Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
            Character CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
            Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;

            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                style = (BaseDialogueStyle)Activator.CreateInstance(Type.GetType(FormerCharacter.Style));
            else
                style = (BaseDialogueStyle)Activator.CreateInstance(Type.GetType(CurrentCharacter.Style) ?? typeof(DefaultDialogueStyle));
            Textbox = new MouseBlockingUIPanel();
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            {
                if (style.BackgroundColor.HasValue)
                    Textbox.BackgroundColor = style.BackgroundColor.Value;
                else if (FormerCharacter.PrimaryColor != null)
                    Textbox.BackgroundColor = FormerCharacter.getPrimaryColor();
                else
                    Textbox.BackgroundColor = new Color(73, 94, 171);

                if (style.BackgroundBorderColor.HasValue)
                    Textbox.BorderColor = style.BackgroundBorderColor.Value;
                else if (FormerCharacter.SecondaryColor != null)
                    Textbox.BorderColor = FormerCharacter.getSecondaryColor();
                else
                    Textbox.BorderColor = Color.Black;
            }
            else
            {
                if (style.BackgroundColor.HasValue)
                    Textbox.BackgroundColor = style.BackgroundColor.Value;
                else if (CurrentCharacter.PrimaryColor != null)
                    Textbox.BackgroundColor = CurrentCharacter.getPrimaryColor();
                else
                    Textbox.BackgroundColor = new Color(73, 94, 171);

                if (style.BackgroundBorderColor.HasValue)
                    Textbox.BorderColor = style.BackgroundBorderColor.Value;
                else if (CurrentCharacter.SecondaryColor != null)
                    Textbox.BorderColor = CurrentCharacter.getSecondaryColor();
                else
                    Textbox.BorderColor = Color.Black;
            }
            style.OnTextboxCreate(Textbox, Speaker, SubSpeaker);
            Textbox.OnLeftClick += OnBoxClick;
            Append(Textbox);
            if (!ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            {
                DialogueText DialogueText = new()
                {
                    boxWidth = Textbox.Width.Pixels,
                    Text = CurrentDialogue.Text
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
                    Response[] availableResponses = [];
                    for(int i = 0; i < CurrentDialogue.Responses.Length; i++)
                    {
                        if (!CurrentDialogue.Responses[i].HasRequirement || ResponseRequirementCheck.Invoke(TreeKey, DialogueIndex, i))
                            availableResponses.Append(CurrentDialogue.Responses[i]);
                    }
                    int responseCount = availableResponses.Length;

                    for (int i = 0; i < responseCount; i++)
                    {
                        UIPanel button = new();
                        Color color;

                        if (style.ButtonColor.HasValue)
                            color = style.ButtonColor.Value;
                        else if (CurrentCharacter.PrimaryColor != null)
                            color = CurrentCharacter.getPrimaryColor();
                        else
                            color = new Color(73, 94, 171);
                        color.A = 125;
                        button.BackgroundColor = color;

                        if (style.ButtonBorderColor.HasValue)
                            button.BorderColor = style.ButtonBorderColor.Value;
                        else if (CurrentCharacter.SecondaryColor != null)
                            button.BorderColor = CurrentCharacter.getSecondaryColor();
                        else
                            button.BorderColor = Color.Black;

                        style.OnResponseButtonCreate(button, Textbox, responseCount, i);
                        button.OnLeftClick += OnButtonClick;
                        Append(button);

                        UIText text;
                        text = new(availableResponses[i].Title, 0f)
                        {
                            TextColor = availableResponses[i].getTextColor(),
                            ShadowColor = availableResponses[i].getTextBorderColor()
                        };

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

                            Texture2D itemTexture = (Texture2D)ModContent.Request<Texture2D>(ItemLoader.GetItem(cost.ItemID).Texture);
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

                style.PostUICreate(DialogueIndex, Textbox, Speaker, SubSpeaker);
                ModContent.GetInstance<DialogueUISystem>().styleSwapped = false;
            }
            else
                style.PostUICreate(DialogueIndex, Textbox, Speaker, SubSpeaker);
        }
        private static bool CanAffordCost(Player player, ItemStack price)
        {
            int amount = price.Stack;
            int itemID = price.ItemID;
            if (itemID == -1)
                itemID = price.TypeToID();
            if (itemID == -1) //If the ItemType is unable to be found, then returns false; as the player can't pay with something that doesn't exist :P
                return false;
            foreach (Item item in player.inventory.Where(i => i.type == itemID))
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
                foreach (Item item in player.inventory.Where(i => i.type == itemID))
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