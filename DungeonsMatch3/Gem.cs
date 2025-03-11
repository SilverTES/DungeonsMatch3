using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using Mugen.Physics;

namespace DungeonsMatch3
{
    class Gem : Node
    {
        Color _color;

        float _angle;
        public Gem(Color color)
        {
            _type = UID.Get<Gem>();
            _color = color;
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _angle += .005f;
            if (_angle > Geo.RAD_360) _angle = Geo.RAD_0;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.Point(AbsXY, 16, _color);
                batch.Circle(AbsXY, 28, 8, _color * .5f, 4, _angle);
                batch.Circle(AbsXY, 20, 8, _color, 4, _angle);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
