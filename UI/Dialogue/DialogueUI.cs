﻿using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using DialogueHelper.UI.Dialogue.DialogueStyles;
using DialogueHelper.UI.Dialogue.Emoticons;
using Terraria.GameContent;
using static ReLogic.Graphics.DynamicSpriteFont;
using ReLogic.Graphics;
using DialogueHelper.UI.Dialogue.TextEffects;
using Terraria.Audio;

namespace DialogueHelper.UI.Dialogue;

public class DialogueUIState : UIState
{
    public class DialogueText(DialogueString[] dialogue) : UIElement
    {
        public static readonly char[] Punctuation =
        [
            '.',
            '?',
            '!',
            ';',
            ':'
        ];

        public DialogueString[] Dialogue = dialogue;
        public bool Crawling = true;

        internal int textIndex = 0;

        private Vector2[] CharPositions;
        private float[] CharUpTimes;
        private int counter = -30;
        private int storedDelay = 0;

        public override void OnActivate()
        {
            string Text = "";
            for (int i = 0; i < Dialogue.Length; i++)
                Text += Dialogue[i].Text;

            CharPositions = new Vector2[Text.Length];
            CharUpTimes = new float[Text.Length];

            int DialogueStringIndex = 0;
            int subIndex = 0;
            Vector2 zero = Vector2.Zero;
            bool newLine = true;

            DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
            DialogueStyle style;
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                style = (DialogueStyle)Activator.CreateInstance(Type.GetType(uiSystem.SubSpeaker.Style));
            else if (uiSystem.CurrentSpeaker != null)
                style = (DialogueStyle)Activator.CreateInstance(Type.GetType(uiSystem.CurrentSpeaker.Style) ?? typeof(DefaultDialogueStyle));
            else
                style = (DialogueStyle)Activator.CreateInstance(typeof(DefaultDialogueStyle));

            int LineCount = 0;

            for (int i = 0; i < Text.Length; i++)
            {
                DialogueString myDialogue = Dialogue[DialogueStringIndex];
                string myText = myDialogue.Text;

                #region Dialogue Parsing
                if (subIndex >= myText.Length)
                {
                    subIndex = 0;
                    DialogueStringIndex++;

                    myDialogue = Dialogue[DialogueStringIndex];
                    myText = myDialogue.Text;
                }
                #endregion

                char c = myText[subIndex];

                DynamicSpriteFont font = FontAssets.ItemStack.Value;

                #region Line Breaking
                switch (c)
                {
                    case '\n':
                        zero.X = 0;
                        zero.Y += font.LineSpacing * myDialogue.Scale.Y;
                        newLine = true;
                        continue;
                    case '\r':
                        continue;
                }
                if (subIndex != 0 && c != ' ' && myText[subIndex - 1] == ' ')
                {
                    int wordIndex = 0;
                    float carry = 0;
                    do
                    {
                        char myChar = myText[wordIndex + subIndex];

                        SpriteCharacterData charDara = font.SpriteCharacters[myChar];
                        Vector3 charKerning = charDara.Kerning;
                        Rectangle charPadding = charDara.Padding;

                        carry += font.CharacterSpacing * myDialogue.Scale.X;
                        carry += charKerning.X * myDialogue.Scale.X;
                        if (CharPositions[i - 1].X + (carry + (charPadding.X * myDialogue.Scale.X)) + style.LineBreakOffsets[LineCount].X > Parent.Width.Pixels - 48 - style.LineBreakOffsets[LineCount].Y)
                        {
                            newLine = true;
                            break;
                        }
                        carry += (charKerning.Y + charKerning.Z) * myDialogue.Scale.X;

                        wordIndex++;
                    }
                    while (wordIndex + subIndex < myText.Length && (myText[wordIndex + subIndex] != ' ' || Punctuation.Contains(myText[wordIndex + subIndex])));

                    if (newLine)
                    {
                        zero.X = 0;
                        zero.Y += font.LineSpacing * myDialogue.Scale.Y;
                    }
                }
                #endregion

                if (newLine)
                    LineCount++;

                #region Positioning
                SpriteCharacterData characterData = font.SpriteCharacters[c];
                Vector3 kerning = characterData.Kerning;
                Rectangle padding = characterData.Padding;

                if (newLine)
                    kerning.X = Math.Max(kerning.X, 0f);
                else
                    zero.X += font.CharacterSpacing * myDialogue.Scale.X;

                zero.X += kerning.X * myDialogue.Scale.X;
                Vector2 position = zero;
                position.X += padding.X * myDialogue.Scale.X;
                position.Y += padding.Y * myDialogue.Scale.Y;

                CharPositions[i] = position + (Vector2.UnitX * style.LineBreakOffsets[LineCount]);

                zero.X += (kerning.Y + kerning.Z) * myDialogue.Scale.X;
                newLine = false;
                #endregion

                subIndex++;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            int dialogueIndex = 0;
            int count = textIndex;
            while (count > Dialogue[dialogueIndex].Text.Length)
            {
                count -= Dialogue[dialogueIndex].Text.Length;
                dialogueIndex++;
            }

            DialogueUISystem uiSystem = ModContent.GetInstance<DialogueUISystem>();
            SoundStyle talkSound;
            if (Dialogue[dialogueIndex].TalkSoundPath != null)
                talkSound = new(Dialogue[dialogueIndex].TalkSoundPath, Dialogue[dialogueIndex].TalkVariantCount);
            else if (uiSystem.CurrentSpeaker != null)
                talkSound = new(uiSystem.CurrentSpeaker.TalkSoundPath, uiSystem.CurrentSpeaker.TalkSoundVariants);
            else
                talkSound = new("DialogueHelper/UI/Dialogue/TextSounds/Default", 1);

            int textDelay = 2;
            if (Dialogue[dialogueIndex].TextDelay > -1)
                textDelay = Dialogue[dialogueIndex].TextDelay;
            else if (uiSystem.CurrentSpeaker != null && uiSystem.CurrentSpeaker.TextDelay > -1)
                textDelay = uiSystem.CurrentSpeaker.TextDelay;

            string FullText = "";
            List<int> endPoints = [];
            count = 0;
            for (int i = 0; i < Dialogue.Length; i++)
            {
                FullText += Dialogue[i].Text;
                count += Dialogue[i].Text.Length - 1;
                endPoints.Add(count);
            }

            if (textIndex < FullText.Length)
            {
                if (counter == 0)
                {
                    if (endPoints.Contains(textIndex))
                        storedDelay = Dialogue[endPoints.IndexOf(textIndex)].EndPause;
                    else
                        switch (FullText[textIndex])
                        {
                            case '.':
                            case '?':
                            case '!':
                            case ';':
                            case ':':
                                storedDelay += 10;
                                break;
                            case '-':
                                if (FullText[textIndex + 1] == ' ')
                                    storedDelay += 10;
                                break;
                            case ',':
                                storedDelay += 5;
                                break;
                        }
                }
                //Main.NewText(storedDelay);
                if (++counter % (FullText[textIndex] == ' ' && storedDelay > 0 ? textDelay * storedDelay : textDelay) == 0 && counter >= 0)
                {
                    if (FullText[textIndex] == ' ')
                        storedDelay = 0;
                    counter = 0;
                    SoundEngine.PlaySound(talkSound);
                    ++textIndex;
                }
            }
            else
                Crawling = false;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle innerDimensions = GetInnerDimensions();
            float xPageTop = innerDimensions.X;
            float yPageTop = innerDimensions.Y + 16;
            Vector2 pageTop = new(xPageTop, yPageTop);

            int DialogueStringIndex = 0;
            int subIndex = 0;

            for (int i = 0; i < textIndex; i++)
            {
                DialogueString myDialogue = Dialogue[DialogueStringIndex];
                string myText = myDialogue.Text;

                #region Dialogue Parsing
                if (subIndex >= myText.Length)
                {
                    subIndex = 0;
                    DialogueStringIndex++;

                    myDialogue = Dialogue[DialogueStringIndex];
                    myText = myDialogue.Text;
                }
                #endregion

                char c = myText[subIndex];

                DynamicSpriteFont font = FontAssets.ItemStack.Value;

                if (c == '\r')
                    continue;

                #region Drawing
                Vector2 pos = CharPositions[i];
                //Vector2 scale = myDialogue.Scale;
                float opacity = 1f;
                float time = 10f;

                if (CharUpTimes[i] / time < 1f)
                {
                    float easing = (float)Math.Sin(CharUpTimes[i] / time * MathF.PI / 2f);
                    Vector2 startOffset = Vector2.One * 16f * myDialogue.Scale;
                    startOffset.X *= 0.75f;
                    startOffset.Y *= 1f;
                    pos = Vector2.Lerp(CharPositions[i] + startOffset, CharPositions[i], easing);
                    opacity = easing;
                }

                TextEffect effect = new();
                if (myDialogue.TextEffect != null)
                    effect = (TextEffect)Activator.CreateInstance(Type.GetType(myDialogue.TextEffect));

                Color color = myDialogue.Rainbow ? Main.DiscoColor : myDialogue.GetTextColor();

                SpriteCharacterData characterData = font.SpriteCharacters[c];
                for (int j = 0; j < ChatManager.ShadowDirections.Length; j++)
                {
                    spriteBatch.Draw(characterData.Texture, pageTop + pos + effect.MoveOffset(i) + (ChatManager.ShadowDirections[j] * 2), characterData.Glyph, Color.Black * (myDialogue.Opacity + effect.OpacityOffset(i)), 0f + effect.RotationOffset(i), Vector2.Zero, myDialogue.Scale + effect.ScaleOffset(i), SpriteEffects.None, 1);
                }
                spriteBatch.Draw(characterData.Texture, pageTop + pos + effect.MoveOffset(i), characterData.Glyph, color * (myDialogue.Opacity + effect.OpacityOffset(i)) * opacity, 0f + effect.RotationOffset(i), Vector2.Zero, myDialogue.Scale + effect.ScaleOffset(i), SpriteEffects.None, 1);
                #endregion
                subIndex++;
                CharUpTimes[i]++;
            }
            #region Old Method
            /*
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
            */
            #endregion
        }
    }

    public MouseBlockingUIPanel Textbox;
    public FlippableUIImage Speaker;
    public FlippableUIImage SubSpeaker;

    public delegate bool CharacterNotifier(string characterName, string expressionName);
    public CharacterNotifier AnimationConditionCheck;

    internal string TreeKey;
    internal int DialogueIndex = 0;

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
                    CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
                if (ModContent.GetInstance<DialogueUISystem>().SubSpeaker != null)
                    FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;
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

            DialogueStyle style;
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                style = (DialogueStyle)Activator.CreateInstance(Type.GetType(FormerCharacter.Style));
            else if (CurrentCharacter != null)
                style = (DialogueStyle)Activator.CreateInstance(Type.GetType(CurrentCharacter.Style) ?? typeof(DefaultDialogueStyle));
            else
                style = (DialogueStyle)Activator.CreateInstance(typeof(DefaultDialogueStyle));

            style.PreUICreate(DialogueIndex);
            if (CurrentCharacter != null && CurrentDialogue.CharacterIndex != -1)
            {
                //Main.NewText("Create Speaker: " + CurrentDialogue.CharacterIndex);
                CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;

                Texture2D speakerTexture = ModContent.Request<Texture2D>(CurrentCharacter.Expressions[CurrentDialogue.ExpressionIndex].Path, AssetRequestMode.ImmediateLoad).Value;
                Rectangle speakerFrame = new(0, 0, speakerTexture.Bounds.Width, speakerTexture.Bounds.Height / CurrentCharacter.Expressions[CurrentDialogue.ExpressionIndex].FrameCount);

                Texture2D speakerFrameTexture = new(Main.graphics.GraphicsDevice, speakerFrame.Width, speakerFrame.Height);
                Color[] data = new Color[speakerFrame.Width * speakerFrame.Height];
                speakerTexture.GetData(0, speakerFrame, data, 0, data.Length);
                speakerFrameTexture.SetData(data);
                Speaker = new(speakerFrameTexture)
                {
                    ImageScale = CurrentCharacter.Scale,
                    spriteEffects = !speakerRight ? SpriteEffects.FlipHorizontally : 0,
                };

                if (justOpened || newSpeaker)
                    SetRectangle(Speaker, left: 0, top: Main.screenHeight, width: speakerFrameTexture.Width, height: speakerFrameTexture.Height);
                else
                    SetRectangle(Speaker, left: 0, top: Main.screenHeight - Speaker.Height.Pixels + 16, width: speakerFrameTexture.Width, height: speakerFrameTexture.Height);

                if (speakerRight)
                    Speaker.Left.Pixels = returningSpeaker ? Main.screenWidth / 1.2f - Speaker.Width.Pixels / 2f : Main.screenWidth / 1.25f - Speaker.Width.Pixels / 2f;
                else
                    Speaker.Left.Pixels = returningSpeaker ? 0f + Speaker.Width.Pixels / 2f : Main.screenWidth * 0.05f + Speaker.Width.Pixels / 2f;

                style.PreSpeakerCreate(DialogueIndex, Speaker);
                Append(Speaker);
                style.PostSpeakerCreate(DialogueIndex, Speaker);

                if (CurrentDialogue.Emoticon != null || (ModContent.GetInstance<DialogueUISystem>().FormerDialogueIndex != -1 && CurrentTree.Dialogues[ModContent.GetInstance<DialogueUISystem>().FormerDialogueIndex].Emoticon != null))
                {
                    Emoticon emoticon;
                    if (CurrentDialogue.Emoticon == null)
                    {
                        emoticon = (Emoticon)Activator.CreateInstance(Type.GetType(CurrentTree.Dialogues[ModContent.GetInstance<DialogueUISystem>().FormerDialogueIndex].Emoticon));
                        emoticon.Fading = true;
                        emoticon.Counter = emoticon.TimeToAppear;
                    }
                    else
                        emoticon = (Emoticon)Activator.CreateInstance(Type.GetType(CurrentDialogue.Emoticon));
                    Rectangle area = emoticon.SpeakerHeadArea = CurrentCharacter.Expressions[CurrentDialogue.ExpressionIndex].HeadArea.GetRectangle();
                    SetRectangle(emoticon, area.Center().X, area.Center().Y, 1, 1);
                    Speaker.Append(emoticon);
                }
            }
            if (FormerCharacter != null)
            {
                //Main.NewText("Create Sub-Speaker: " + subSpeakerIndex);
                Texture2D subSpeakerTexture = ModContent.Request<Texture2D>(FormerCharacter.Expressions[0].Path, AssetRequestMode.ImmediateLoad).Value;
                Rectangle subSpeakerFrame = new(0, 0, subSpeakerTexture.Bounds.Width, subSpeakerTexture.Bounds.Height / FormerCharacter.Expressions[0].FrameCount);

                Texture2D subSpeakerFrameTexture = new(Main.graphics.GraphicsDevice, subSpeakerFrame.Width, subSpeakerFrame.Height);
                Color[] data = new Color[subSpeakerFrame.Width * subSpeakerFrame.Height];
                subSpeakerTexture.GetData(0, subSpeakerFrame, data, 0, data.Length);
                subSpeakerFrameTexture.SetData(data);

                SubSpeaker = new(subSpeakerFrameTexture)
                {
                    ImageScale = FormerCharacter.Scale,
                    spriteEffects = speakerRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    Color = Color.Gray,
                };

                SetRectangle(SubSpeaker, left: 0, top: Main.screenHeight - SubSpeaker.Height.Pixels + 16, width: subSpeakerFrameTexture.Width, height: subSpeakerFrameTexture.Height);

                if (speakerRight)
                    SubSpeaker.Left.Pixels = newSpeaker || returningSpeaker ? Main.screenWidth * 0.05f + SubSpeaker.Width.Pixels / 2f : 0f + SubSpeaker.Width.Pixels / 2f;
                else
                    SubSpeaker.Left.Pixels = newSpeaker || returningSpeaker ? Main.screenWidth / 1.25f - SubSpeaker.Width.Pixels / 2f : Main.screenWidth / 1.35f - SubSpeaker.Width.Pixels / 2f;

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

        DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
        Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
        Character CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
        Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;

        //Main.NewText(CurrentDialogue.Responses[1].Cost.TypePath);

        DialogueStyle style;
        if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            style = (DialogueStyle)Activator.CreateInstance(Type.GetType(FormerCharacter.Style));
        else if (CurrentCharacter != null)
            style = (DialogueStyle)Activator.CreateInstance(Type.GetType(CurrentCharacter.Style) ?? typeof(DefaultDialogueStyle));
        else
            style = (DialogueStyle)Activator.CreateInstance(typeof(DefaultDialogueStyle));

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
                float goalLeft = Main.screenWidth * 0.05f + Speaker.Width.Pixels / 2f;
                float goalRight = Main.screenWidth / 1.25f - Speaker.Width.Pixels / 2f;

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
                if (currentExpression.FrameCount != 1 && currentExpression.FrameRate != 0 && counter % currentExpression.FrameRate == 0)
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
            if (CurrentTree.Characters.Length != 0)
            {
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
            else if (style.TextboxOffScreen(Textbox))
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
        if (CurrentDialogue.Responses.Length == 0 && !dialogue.Crawling)
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
        else if (dialogue.Crawling)
            dialogue.textIndex = CurrentDialogue.GetFullText().Length;
    }
    internal void OnButtonClick(UIMouseEvent evt, UIElement listeningElement)
    {
        DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
        int responseCount = CurrentTree.Dialogues[DialogueIndex].Responses.Length;
        UIText text = (UIText)listeningElement.Children.ToArray().First(c => c.GetType() == typeof(UIText));
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
        DialogueStyle style;
        float xResolutionScale = Main.screenWidth / 2560f;
        float yResolutionScale = Main.screenHeight / 1440f;

        DialogueTree CurrentTree = ModContent.GetInstance<DialogueUISystem>().CurrentTree;
        Dialogue CurrentDialogue = CurrentTree.Dialogues[DialogueIndex];
        Character CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;
        Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;

        if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            style = (DialogueStyle)Activator.CreateInstance(Type.GetType(FormerCharacter.Style));
        else if (CurrentCharacter != null)
            style = (DialogueStyle)Activator.CreateInstance(Type.GetType(CurrentCharacter.Style) ?? typeof(DefaultDialogueStyle));
        else
            style = (DialogueStyle)Activator.CreateInstance(typeof(DefaultDialogueStyle));

        Textbox = new MouseBlockingUIPanel();
        if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
        {
            if (style.BackgroundColor.HasValue)
                Textbox.BackgroundColor = style.BackgroundColor.Value;
            else if (FormerCharacter.PrimaryColor != null)
                Textbox.BackgroundColor = FormerCharacter.GetPrimaryColor();
            else
                Textbox.BackgroundColor = new Color(73, 94, 171);

            if (style.BackgroundBorderColor.HasValue)
                Textbox.BorderColor = style.BackgroundBorderColor.Value;
            else if (FormerCharacter.SecondaryColor != null)
                Textbox.BorderColor = FormerCharacter.GetSecondaryColor();
            else
                Textbox.BorderColor = Color.Black;
        }
        else
        {
            if (style.BackgroundColor.HasValue)
                Textbox.BackgroundColor = style.BackgroundColor.Value;
            else if (CurrentCharacter != null && CurrentCharacter.PrimaryColor != null)
                Textbox.BackgroundColor = CurrentCharacter.GetPrimaryColor();
            else
                Textbox.BackgroundColor = new Color(73, 94, 171);

            if (style.BackgroundBorderColor.HasValue)
                Textbox.BorderColor = style.BackgroundBorderColor.Value;
            else if (CurrentCharacter != null && CurrentCharacter.SecondaryColor != null)
                Textbox.BorderColor = CurrentCharacter.GetSecondaryColor();
            else
                Textbox.BorderColor = Color.Black;
        }
        style.OnTextboxCreate(Textbox, Speaker, SubSpeaker);
        Textbox.OnLeftClick += OnBoxClick;
        Append(Textbox);
        if (!ModContent.GetInstance<DialogueUISystem>().swappingStyle)
        {
            DialogueText DialogueText = new(CurrentDialogue.DialogueText);

            style.OnDialogueTextCreate(DialogueText);
            Textbox.Append(DialogueText);

            if (CurrentDialogue.Responses != null)
            {
                List<Response> availableResponses = [];
                for (int i = 0; i < CurrentDialogue.Responses.Length; i++)
                {
                    if (CurrentDialogue.Responses[i].Requirement)
                        availableResponses.Add(CurrentDialogue.Responses[i]);
                }
                int responseCount = availableResponses.Count;
                for (int i = 0; i < responseCount; i++)
                {
                    UIPanel button = new();
                    Color color;

                    if (style.ButtonColor.HasValue)
                        color = style.ButtonColor.Value;
                    else if (CurrentCharacter.PrimaryColor != null)
                        color = CurrentCharacter.GetPrimaryColor();
                    else
                        color = new Color(73, 94, 171);
                    color.A = 125;
                    button.BackgroundColor = color;

                    if (style.ButtonBorderColor.HasValue)
                        button.BorderColor = style.ButtonBorderColor.Value;
                    else if (CurrentCharacter.SecondaryColor != null)
                        button.BorderColor = CurrentCharacter.GetSecondaryColor();
                    else
                        button.BorderColor = Color.Black;

                    style.OnResponseButtonCreate(button, Textbox, responseCount, i);
                    button.OnLeftClick += OnButtonClick;
                    Append(button);

                    UIText text;
                    text = new(availableResponses[i].Title, 0f)
                    {
                        TextColor = availableResponses[i].GetTextColor(),
                        ShadowColor = availableResponses[i].GetTextBorderColor()
                    };

                    text.Width.Pixels = style.ButtonSize.X;
                    text.IsWrapped = true;
                    text.WrappedTextBottomPadding = 0.1f;
                    style.OnResponseTextCreate(text);
                    button.Append(text);
                    if (availableResponses[i].Cost != null)
                    {
                        ItemStack cost = availableResponses[i].Cost;
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

                        int itemID = cost.ItemID;
                        if (itemID == -1)
                            itemID = cost.FetchItemID();
                        if (itemID == -1) //If the ItemType is unable to be found, then break.
                        {
                            costHolder = null;
                            stackText = null;
                            continue;
                        }

                        Texture2D itemTexture = (Texture2D)ModContent.Request<Texture2D>(ItemLoader.GetItem(itemID).Texture);
                        UIImage itemIcon = new(itemTexture);
                        itemIcon.Width.Pixels = itemTexture.Width;
                        itemIcon.Height.Pixels = itemTexture.Height;
                        itemIcon.ImageScale = 18f / itemIcon.Height.Pixels;

                        itemIcon.Top.Pixels -= itemIcon.Height.Pixels / 2;
                        itemIcon.Left.Pixels -= itemIcon.Width.Pixels / 2;

                        itemIcon.Left.Pixels -= itemIcon.Width.Pixels * itemIcon.ImageScale / 2;
                        stackText.Left.Pixels += itemIcon.Width.Pixels * itemIcon.ImageScale / 2;

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
            itemID = price.FetchItemID();
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
}