using Terraria.ModLoader;
using Terraria;

namespace DialogueHelper.Content.UI.Dialogue
{
    public class DialogueMusicPlayer : ModPlayer
    {
        public override void PostUpdateEquips()
        {
            if (ModContent.GetInstance<DialogueUISystem>() != null && !ModContent.GetInstance<DialogueUISystem>().isDialogueOpen)
                return;

            DialogueUISystem dialogueUISystem = ModContent.GetInstance<DialogueUISystem>();
            DialogueUIState UI = dialogueUISystem.DialogueUIState;
            Dialogue CurrentDialogue = dialogueUISystem.CurrentTree.Dialogues[UI.DialogueIndex];
            if (CurrentDialogue.MusicID == -1 || !(!Main.gameMenu && !Main.dedServ))
                return;
            Main.musicBox2 = CurrentDialogue.MusicID;
        }
    }
}
