﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace DialogueHelper.UI.Dialogue.Emoticons
{
    public class BaseEmoticon : UIElement
    {
        public int Counter = 0;
        public int FrameNum = 0;
        public float ImageScale = 1f;
        public float Opacity = 1f;
        internal bool Fading = false;
        public float Rotation;
        public Color Color = Color.White;
        public SpriteEffects spriteEffects;
        internal Rectangle SpeakerHeadArea;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Fading)
            {
                if(--Counter < 0)
                    Remove();
            }
            else
                Counter++;
        }

        public virtual int TimeToAppear => 0;

        public virtual Vector2 OffsetPosition() => Vector2.Zero;
    }
}
