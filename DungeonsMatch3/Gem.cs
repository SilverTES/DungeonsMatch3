using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using Mugen.Physics;
using Mugen.GFX;
using Mugen.Animation;
using System;

namespace DungeonsMatch3
{
    class Gem : Node
    {
        public enum States
        {
            None,
            Move,
            Dead,
        }

        static public Color[] Colors =
        [
            Color.Red,
            Color.DodgerBlue,
            Color.Green,
            Color.Yellow,
            Color.Violet,
        ];

        public Color Color;

        float _angle;

        public bool _isSelected = false;

        public Point MapPosition;

        Arena _arena;

        bool _isMove = false;
        int _tempoMove = 60;
        int _ticMove = 0;
        Vector2 _from;
        Vector2 _goal;

        public Point DownPosition;
        

        int _tempoDead = 24;

        float _radius = 32;
        float _ticRadius = 0;

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

            ChangeState((int)States.None);
        }
        public void ExploseMe()
        {
            ChangeState((int)States.Dead);
        }
        public void MoveTo(Point mapPosition)
        {
            _from = _arena.MapPositionToVector2(MapPosition);
            _goal = _arena.MapPositionToVector2(mapPosition);

            ChangeState((int)States.Move);
            Console.WriteLine("Gem Move Down");
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.None:

                    _angle += .005f;
                    if (_angle >= Geo.RAD_360) _angle = Geo.RAD_0;

                    break;

                case States.Move:


                    _x = Easing.GetValue(Easing.ElasticEaseOut, _ticMove, _from.X, _goal.X, _tempoMove);
                    _y = Easing.GetValue(Easing.ElasticEaseOut, _ticMove, _from.Y, _goal.Y, _tempoMove);

                    _ticMove++;

                    if (_ticMove > _tempoMove)
                    {
                        _ticMove = 0;
                        ChangeState((int)States.None);
                    }

                    break;

                case States.Dead:

                    _tempoDead--;

                    if (_tempoDead <= 0)
                        KillMe();

                    _ticRadius++;

                    _radius = Easing.GetValue(Easing.BounceEaseOut,_ticRadius, 32, 0, 24);

                    break;

                default:
                    break;
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            RunState(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.Point(AbsXY, _radius / 2, Color);
                batch.Circle(AbsXY, _radius - 12, 8, Color, 4, _angle);
                batch.Circle(AbsXY, _radius - 4, 8, Color, 4, _angle);
                batch.Circle(AbsXY, _radius, 8, Color.Black, 5, _angle);
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
