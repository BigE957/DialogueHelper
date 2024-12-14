using Microsoft.Xna.Framework;

namespace DialogueHelper.UI.Dialogue.TextEffects
{
    public class TextEffect
    {
        public virtual Vector2 MoveOffset() => Vector2.Zero;

        public virtual Vector2 ScaleOffset() => Vector2.Zero;

        public virtual float OpacityOffset() => 0f;

        public virtual Color ColorOffset() => new(Vector3.Zero);

        public virtual float RotationOffset() => 0f;
    }
}
