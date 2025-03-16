using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;

namespace DungeonsMatch3
{
    class Slot : Node   
    {
        public Slot() 
        {
            SetSize(360, 160);
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
                batch.CenterBorderedStringXY(Game1._fontMain, "Slot", AbsRectF.TopLeft - Vector2.One * 4, Color.Yellow, Color.Black);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
