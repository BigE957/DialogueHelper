using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DialogueHelper.UI.Dialogue.Emoticons
{
    public class Confused : BaseEmoticon
    {
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ImageScale = MathHelper.Clamp(MathHelper.Lerp(0, 1f, Counter / 60f), 0, 1f);
            Opacity = MathHelper.Clamp(MathHelper.Lerp(0, 1f, Counter / 90f), 0, 1f);
        }

        public override Vector2 OffsetPosition() =>  new(MathHelper.Clamp(MathHelper.Lerp(0, SpeakerHeadArea.Width * 2, Counter / 60f), 0, SpeakerHeadArea.Width * 2), MathHelper.Clamp(MathHelper.Lerp(0, SpeakerHeadArea.Height * 2, Counter / 60f), 0, SpeakerHeadArea.Height * 2));

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            Texture2D texture = ModContent.Request<Texture2D>("DialogueHelper/UI/Dialogue/Emoticons/ConfusedIcon").Value;
            float FrameCount = 4f;

            if(Counter % 12 == 0 && ++FrameNum == FrameCount)
                FrameNum = 0;

            Vector2 origin = texture.Size();
            origin.Y /= FrameCount;           

            Rectangle source = new(0, (int)origin.Y * FrameNum, (int)origin.X, (int)origin.Y * (FrameNum + 1));

            origin *= 0.5f;

            Vector2 position = dimensions.Position() + (origin * ImageScale);
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new(16 * (i - 1), (float)Math.Cos((Counter + (8 * i)) * 6) * 2);
                spriteBatch.Draw(texture, position + offset + OffsetPosition(), source, Color * Opacity, Rotation + (MathHelper.Pi / 12 * (i - 1) + (float)Math.Sin(Counter * 10) * 15), origin, ImageScale * (i == 1 ? 1f : 0.75f), spriteEffects, 0f);
            }
        }
    }
}
