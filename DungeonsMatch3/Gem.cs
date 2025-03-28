﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Mugen.Core;
using Mugen.Physics;
using Mugen.GFX;
using Mugen.Animation;

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
        public State<States> State { get; private set; } = new State<States>(States.None);

        static public Color[] Colors =
        [
            Color.Red,
            Color.DodgerBlue,
            Color.ForestGreen,
            //Color.DarkOrange,
            //Color.BlueViolet,
            Color.Gold,
            Color.Gray, // Gem do nothing !
            //Color.HotPink,
        ];

        public Color Color;

        float _angle = (float)Geo.DegToRad(25);

        public bool IsSelected = false;
        public bool IsSameColor = false;    // true if found some close gems with same color
        public int NbSameColor = 0; 

        public Point MapPosition;

        Arena _arena;


        int _tempoMove = 30;
        int _ticMove = 0;
        Vector2 _from;
        Vector2 _goal;

        public Point GoalPosition;
        public bool IsFall = false;

        static public float Radius = 36;
        float _radius;
        float _ticRadius = 0;

        int _tempoDead = 24;

        public Shake Shake = new();
        public Gem()
        {
            _type = UID.Get<Gem>();
        }
        //public void Shake(float intensity, float step = 2, bool shakeX = true, bool shakeY = true)
        //{
        //    _shake.SetIntensity(intensity, step, shakeX, shakeY);
        //}
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

            State.Change(States.None);

            _radius = Radius;

        }
        public void ExploseMe()
        {
            new FxExplose(AbsXY, Color, 10, 40).AppendTo(_parent);
            new PopInfo(NbSameColor.ToString(), Color.White, Color, 0, 32, 32).SetPosition(XY).AppendTo(_parent);

            State.Change(States.Dead);
        }
        public void MoveTo(Point goalPosition)
        {
            GoalPosition = goalPosition;
            _arena.SetInGrid(this, goalPosition);

            _from = _arena.MapPositionToVector2(MapPosition);
            _goal = _arena.MapPositionToVector2(goalPosition);

            State.Change(States.Move);
            //Console.WriteLine("Gem Move Down");
        }
        private void RunState(GameTime gameTime)
        {
            switch (State.CurState)
            {
                case States.None:

                    //_angle += .005f;
                    if (_angle >= Geo.RAD_360) _angle = Geo.RAD_0;

                    break;

                case States.Move:


                    _x = Easing.GetValue(Easing.BounceEaseOut, _ticMove, _from.X, _goal.X, _tempoMove);
                    _y = Easing.GetValue(Easing.BounceEaseOut, _ticMove, _from.Y, _goal.Y, _tempoMove);

                    _ticMove++;

                    if (_ticMove > _tempoMove)
                    {
                        _ticMove = 0;

                        MapPosition = GoalPosition;

                        State.Change(States.None);

                    }

                    break;

                case States.Dead:

                    _tempoDead--;

                    if (_tempoDead <= 0)
                    {
                        KillMe();
                    }

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
                var shake = Shake.GetVector2();

                //batch.Point(AbsXY + shake, _radius / 2, Color);

                //batch.Circle(AbsXY + shake, _radius - 12, 8, Color, 4, _angle);
                //batch.Circle(AbsXY + shake, _radius - 4, 8, Color, 4, _angle);
                //batch.Circle(AbsXY + shake, _radius, 8, Color.Black, 2, _angle);


                GFX.Draw(batch, Game1._texGem, Color, _angle, AbsXY + shake, Position.CENTER, Vector2.One * .4f);

            }

            if (indexLayer == (int)Game1.Layers.FrontFX)
            {
                var shake = Shake.GetVector2();

                if (IsSameColor && NbSameColor > 2)// && _arena.GetState() == (int)Arena.States.SelectGems)
                    GFX.Draw(batch, Game1._texGemLight, Color.White, _angle, AbsXY + shake, Position.CENTER, Vector2.One * .4f);
                    //batch.Circle(AbsXY + shake, _radius + 2, 8, Color.White * 1f, 2f);

                if (IsSelected)
                {
                    //batch.Point(AbsXY + shake, _radius / 4, Color.White);
                    GFX.Draw(batch, Game1._texGlow, Color.White, 0, AbsXY + shake, Position.CENTER, Vector2.One * .3f);
                }
            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                //if (_isSelected)
                //    batch.Circle(AbsXY, 40, 24, Color.Black, 4);

                //batch.CenterStringXY(Game1._fontMain, MapPosition.ToString(), AbsXY , Color.White);
                //batch.CenterStringXY(Game1._fontMain, NbSameColor.ToString(), AbsXY , Color.White);

                //batch.Circle(AbsXY, Arena.RadiusGemOver, 24, Color.White, 1, _angle);

            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
