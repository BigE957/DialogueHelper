using DialogueHelper.UI;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using static DialogueHelper.UI.Dialogue.DialogueUIState;

namespace DialogueHelper.UI.Dialogue.DialogueStyles;

public class BaseDialogueStyle
{
    public virtual Vector2 ButtonSize => new(100, 50);
    public virtual Color? ButtonColor => null;
    public virtual Color? ButtonBorderColor => null;
    public virtual Color? BackgroundColor => null;
    public virtual Color? BackgroundBorderColor => null;
    #region UI Creation Methods
    public virtual void PreUICreate(int dialogueIndex)
    {

    }
    public virtual void PreSpeakerCreate(int dialogueIndex, FlippableUIImage speaker)
    {

    }
    public virtual void PostSpeakerCreate(int dialogueIndex, FlippableUIImage speaker)
    {

    }
    public virtual void PreSubSpeakerCreate(int dialogueIndex, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {

    }
    public virtual void PostSubSpeakerCreate(int dialogueIndex, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {

    }
    public virtual void OnTextboxCreate(UIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {

    }
    public virtual void OnDialogueTextCreate(DialogueText text)
    {

    }
    public virtual void OnResponseButtonCreate(UIPanel button, MouseBlockingUIPanel textbox, int responseCount, int buttonCounter)
    {

    }
    public virtual void OnResponseTextCreate(UIText text)
    {

    }
    public virtual void OnResponseCostCreate(UIText text, UIPanel costHolder)
    { }
    public virtual void PostUICreate(int dialogueIndex, UIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {

    }
    public virtual bool TextboxOffScreen(UIPanel textbox)
    {
        return false;
    }
    #endregion
    #region Update Methods
    public virtual void PostUpdateActive(MouseBlockingUIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {

    }
    public virtual void PostUpdateClosing(MouseBlockingUIPanel textbox, FlippableUIImage speaker, FlippableUIImage subSpeaker)
    {

    }
    #endregion
}
