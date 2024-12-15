using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace DialogueHelper.UI.Dialogue.TextEffects;
public class ShakyText : TextEffect
{
    public override Vector2 MoveOffset(int index)
    {
        return new Vector2((float)Math.Sin((Main.GlobalTimeWrappedHourly + index) * 32f), (float)Math.Sin((Main.GlobalTimeWrappedHourly + index) * 16f)) * 1.5f;
    }
}
