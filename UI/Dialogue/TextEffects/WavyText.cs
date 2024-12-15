using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogueHelper.UI.Dialogue.TextEffects;
public class WavyText : TextEffect
{
    public override Vector2 MoveOffset(int index)
    {
        return new Vector2(0, (float)Math.Sin((Main.GlobalTimeWrappedHourly + (index * 0.1f)) * 3f)) * 5f;
    }
}