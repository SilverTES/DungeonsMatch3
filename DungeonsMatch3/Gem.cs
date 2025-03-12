using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using Mugen.Physics;
using Mugen.GFX;
using System.Collections.Generic;
using System.Linq;

namespace DungeonsMatch3
{
    class Gem : Node
    {
        static public Color[] Colors =
        [
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.Violet,
        ];

        public Color Color;

        float _angle;

        public bool _isSelected = false;

        public Point MapPosition;

        Arena _arena;

        public Gem()
        {
            _type = UID.Get<Gem>();
        }
        //public int MyIndexColor()
        //{
        //    return Colors.FirstOrDefault(x => x.Value == Color).Key;
        //}
        public Gem(Arena arena, Color color, Point mapPosition)
        {
            _type = UID.Get<Gem>();

            _arena = arena;
            Color = color;
            MapPosition = mapPosition;
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _angle += .005f;
            if (_angle >= Geo.RAD_360) _angle = Geo.RAD_0;

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.Point(AbsXY, 16, Color);
                batch.Circle(AbsXY, 28, 8, Color, 4, _angle);
                batch.Circle(AbsXY, 20, 8, Color, 4, _angle);
            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                if (_isSelected)
                    batch.Circle(AbsXY, 40, 24, Color.White, 4);

                //batch.CenterStringXY(Game1._fontMain, MapPosition.ToString(), AbsXY , Color.White);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
