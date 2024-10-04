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
            if (CurrentDialogue.Music == null || !(!Main.gameMenu && !Main.dedServ))
                return;
            int MusicID = MusicLoader.GetMusicSlot(ModLoader.GetMod(CurrentDialogue.Music.ModName), CurrentDialogue.Music.FilePath);
            Main.musicBox2 = MusicID;
        }
    }
}
