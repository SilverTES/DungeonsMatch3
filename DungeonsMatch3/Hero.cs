using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;

namespace DungeonsMatch3
{
    class Hero : Node
    {
        public Hero() 
        {
            SetSize(240, 240);

        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue * .5f);
                batch.Rectangle(AbsRectF, Color.DarkSlateBlue, 3f);

                batch.CenterBorderedStringXY(Game1._fontMain, "Hero", AbsRectF.TopCenter, Color.Yellow, Color.Black);
            }
            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
