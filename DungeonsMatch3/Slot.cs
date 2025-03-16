using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;

namespace DungeonsMatch3
{
    class Slot : Node   
    {
        bool _flip = false;
        public Slot() 
        {
            SetSize(320, 240);
        }
        public Slot Flip(bool flip = true)
        {
            _flip = flip;
            return this;
        }
        public override Node Update(GameTime gameTime)
        {
            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRectF.Extend(0), Color.Black * .5f);
                //batch.Rectangle(AbsRectF.Extend(0), Color.DarkSlateBlue, 5f);

                //batch.CenterBorderedStringXY(Game1._fontMain, "Enemy", shake + AbsRectF.TopCenter, Color.Yellow, Color.Black);

                //batch.Draw(Game1._texHero00, AbsRect, Color.White);

                var tex = Game1._texHero00;

                GFX.Draw(batch, tex, Color.White, 0, AbsXY + (tex.Bounds.Size.ToVector2() / 2), Position.CENTER, Vector2.One, _flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

                batch.BevelledRectangle(AbsRectF, Vector2.One * 8, Color.DarkSlateBlue, 3f);


                batch.CenterBorderedStringXY(Game1._fontMain, "Slot", AbsRectF.TopCenter, Color.Yellow, Color.Black);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
