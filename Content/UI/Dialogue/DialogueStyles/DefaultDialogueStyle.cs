using static DialogueHelper.Content.UI.Dialogue.DialogueUIState;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader.UI;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;

namespace DialogueHelper.Content.UI.Dialogue.DialogueStyles
{
    public class DefaultDialogueStyle : BaseDialogueStyle
    {
        public override Vector2 ButtonSize => new(150, 50);
        public override void OnTextboxCreate(UIPanel textbox, UIImage speaker, UIImage subSpeaker)
        {
            bool speakerRight = ModContent.GetInstance<DialogueUISystem>().speakerRight;
            bool spawnBottom = ModContent.GetInstance<DialogueUISystem>().justOpened || ModContent.GetInstance<DialogueUISystem>().styleSwapped;
            bool newSubSpeaker = ModContent.GetInstance<DialogueUISystem>().newSubSpeaker;
            textbox.SetPadding(0);
            SetRectangle(textbox, left: 0, top: spawnBottom ? Main.screenHeight * 1.05f : Main.screenHeight / 1.75f, width: Main.screenWidth / 1.75f, height: Main.screenHeight / 3);
            if (newSubSpeaker && !ModContent.GetInstance<DialogueUISystem>().styleSwapped)
                textbox.Left.Pixels = speakerRight ? Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f : Main.screenWidth / 12f;
            else
                textbox.Left.Pixels = speakerRight ? Main.screenWidth / 12f : Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f;

        }
        public override void OnDialogueTextCreate(DialogueText text)
        {
            text.Top.Pixels = 25;
            text.Left.Pixels = 15;
        }
        public override void OnResponseButtonCreate(UIPanel button, MouseBlockingUIPanel textbox, int responseCount, int i)
        {
            button.Width.Set(20, 0);
            button.Height.Set(10, 0);
            button.Left.Pixels = (textbox.Width.Pixels + 200) * (i / responseCount);
            button.Top.Set(2000, 0);
        }
        public override void OnResponseTextCreate(UIText text)
        {
            text.HAlign = text.VAlign = 0.5f;
        }
        public override void OnResponseCostCreate(UIText text, UIPanel costHolder)
        {
            text.VAlign = 0f;
            costHolder.HAlign = 0.5f;
        }
        public override void PostUICreate(int dialogueIndex, UIPanel textbox, UIImage speaker, UIImage subSpeaker)
        {
            Dialogue CurrentDialogue = ModContent.GetInstance<DialogueUISystem>().CurrentTree.Dialogues[dialogueIndex];
            Character CurrentCharacter = ModContent.GetInstance<DialogueUISystem>().CurrentSpeaker;

            MouseBlockingUIPanel NameBox;
            NameBox = new MouseBlockingUIPanel();
            NameBox.SetPadding(0);
            SetRectangle(NameBox, left: -25f, top: -25f, width: 300f, height: 60f);
            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            {
                Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;
                if (FormerCharacter.PrimaryColor != null)
                    NameBox.BackgroundColor = FormerCharacter.getPrimaryColor();
                else
                    NameBox.BackgroundColor = new Color(73, 94, 171);

                if (FormerCharacter.SecondaryColor != null)
                    NameBox.BorderColor = FormerCharacter.getSecondaryColor();
            }
            else
            {
                if (CurrentCharacter.PrimaryColor != null)
                    NameBox.BackgroundColor = CurrentCharacter.getPrimaryColor();
                else
                    NameBox.BackgroundColor = new Color(73, 94, 171);

                if (CurrentCharacter.SecondaryColor != null)
                    NameBox.BorderColor = CurrentCharacter.getSecondaryColor();
            }
            textbox.Append(NameBox);

            UIText NameText;
            if (CurrentDialogue.CharacterIndex == -1)
                NameText = new UIText("...");
            else
            {
                if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
                {
                    Character FormerCharacter = ModContent.GetInstance<DialogueUISystem>().SubSpeaker;
                    NameText = new UIText(FormerCharacter.Name, 1f, true);
                }
                else
                    NameText = new UIText(CurrentCharacter.Name, 1f, true);
            }
            NameText.Width.Pixels = NameBox.Width.Pixels;
            NameText.HAlign = 0.5f;
            NameText.Top.Set(15, 0);
            NameBox.Append(NameText);
        }
        public override void PostUpdateActive(MouseBlockingUIPanel textbox, UIImage speaker, UIImage subSpeaker)
        {
            float xResolutionScale = Main.screenWidth / 2560f;
            float yResolutionScale = Main.screenHeight / 1440f;

            if (ModContent.GetInstance<DialogueUISystem>().swappingStyle)
            {
                if (!TextboxOffScreen(textbox))
                {
                    float goalHeight = Main.screenHeight * 1.5f;

                    textbox.Top.Pixels += (goalHeight - textbox.Top.Pixels) / 20;
                    if (goalHeight - textbox.Top.Pixels < 10)
                        textbox.Top.Pixels = goalHeight;
                }
                else
                {
                    ModContent.GetInstance<DialogueUISystem>().styleSwapped = true;
                    ModContent.GetInstance<DialogueUISystem>().swappingStyle = false;
                    textbox.RemoveAllChildren();
                    textbox.Remove();

                    ModContent.GetInstance<DialogueUISystem>().DialogueUIState.SpawnTextBox();
                }
            }
            else
            {
                float goalHeight = Main.screenHeight / 1.75f;

                if (textbox.Top.Pixels > goalHeight)
                {
                    textbox.Top.Pixels -= (textbox.Top.Pixels - goalHeight) / 10;
                    if (textbox.Top.Pixels - goalHeight < 1)
                        textbox.Top.Pixels = goalHeight;

                }
                float goalLeft = Main.screenWidth / 12f;
                float goalright = Main.screenWidth - textbox.Width.Pixels - Main.screenWidth / 12f;

                if (ModContent.GetInstance<DialogueUISystem>().speakerRight && textbox.Left.Pixels > 125f)
                {
                    textbox.Left.Pixels -= (-goalLeft + textbox.Left.Pixels) / 20;
                    if (-goalLeft + textbox.Left.Pixels < 1)
                        textbox.Left.Pixels = goalLeft;
                }
                else if (!ModContent.GetInstance<DialogueUISystem>().speakerRight && textbox.Left.Pixels < 600f)
                {
                    textbox.Left.Pixels += (goalright - textbox.Left.Pixels) / 20;
                    if (goalright - textbox.Left.Pixels < 1)
                        textbox.Left.Pixels = goalright;
                }
                #region Button Updates
                DialogueText dialogue = (DialogueText)textbox.Children.Where(c => c.GetType() == typeof(DialogueText)).First();
                UIElement[] responseButtons;
                if (ModContent.GetInstance<DialogueUISystem>().DialogueUIState.Children.Where(c => c.GetType() == typeof(UIPanel) && c.Children.First().GetType() == typeof(UIText)).Any())
                    responseButtons = ModContent.GetInstance<DialogueUISystem>().DialogueUIState.Children.Where(c => c.GetType() == typeof(UIPanel) && c.Children.First().GetType() == typeof(UIText)).ToArray();
                else
                    responseButtons = textbox.Children.Where(c => c.GetType() == typeof(UIPanel)).ToArray();
                for (int i = 0; i < responseButtons.Length; i++)
                {
                    UIElement button = responseButtons[i];
                    if (!dialogue.crawling && button.Width.Pixels < ButtonSize.X)
                    {
                        if (!textbox.HasChild(button))
                            textbox.AddOrRemoveChild(button, true);
                        button.Top.Set(0, 0);
                        button.VAlign = 0.86f;

                        button.Left.Pixels = textbox.Width.Pixels / (2 * responseButtons.Length) + (textbox.Width.Pixels * (float)(i / (float)responseButtons.Length) - button.Width.Pixels / 2);
                        button.Left.Pixels -= 3;

                        button.Width.Pixels = MathHelper.Clamp(button.Width.Pixels + ButtonSize.X / 30, 0f, ButtonSize.X);
                        button.Height.Pixels = MathHelper.Clamp(button.Height.Pixels + ButtonSize.Y / 30, 0f, ButtonSize.Y);

                        button.Top.Pixels += button.Height.Pixels / 2;

                        foreach (UIElement child in button.Children)
                        {
                            if (child.GetType() == typeof(UIText))
                            {
                                if (button.Width.Pixels < ButtonSize.X / 1.5f)
                                    continue;
                                //Main.NewText(child.Width.Pixels);
                                UIText textChild = (UIText)child;
                                textChild.SetText(textChild.Text, MathHelper.Clamp(0.875f * ((button.Width.Pixels - ButtonSize.X / 1.5f) / ButtonSize.X) * 3f, 0f, 0.875f), false);
                                textChild.Top.Pixels = button.Top.Pixels - (int)(button.Height.Pixels / 2) + 4;
                                textChild.IsWrapped = true;
                                textChild.WrappedTextBottomPadding = -2f;
                                if (button.Children.Count() > 1)
                                    textChild.Top.Pixels = -4;
                            }
                            else
                                child.Top.Pixels = -Main.screenHeight * 1.25f;
                        }
                    }
                    else if (button.Children.Count() > 1)
                    {
                        UIElement child = (UIElement)button.Children.Where(c => c.GetType() == typeof(UIPanel)).First();
                        child.Top.Pixels = child.Parent.Height.Pixels / 4;
                    }
                }
                #endregion
            }
        }
        public override void PostUpdateClosing(MouseBlockingUIPanel textbox, UIImage speaker, UIImage subSpeaker)
        {
            if (!TextboxOffScreen(textbox))
            {
                float goalHeight = Main.screenHeight * 1.1f;

                textbox.Top.Pixels += (goalHeight - textbox.Top.Pixels) / 20;
                if (goalHeight - textbox.Top.Pixels < 10)
                    textbox.Top.Pixels = goalHeight;
            }

        }
        public override bool TextboxOffScreen(UIPanel textbox)
        {
            return textbox.Top.Pixels >= Main.screenHeight * 1.05f;
        }
    }
}
