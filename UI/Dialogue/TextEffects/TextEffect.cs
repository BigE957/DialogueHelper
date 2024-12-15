using Microsoft.Xna.Framework;

namespace DialogueHelper.UI.Dialogue.TextEffects
{
    public class TextEffect
    {
        public virtual Vector2 MoveOffset(int index) => Vector2.Zero;

        public virtual Vector2 ScaleOffset(int index) => Vector2.Zero;

        public virtual float OpacityOffset(int index) => 0f;

        public virtual float RotationOffset(int index) => 0f;
    }
}
