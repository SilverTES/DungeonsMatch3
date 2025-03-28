﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Physics;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using static Mugen.Core.Addon;

namespace DungeonsMatch3
{
    class Unit : Node
    {
        public enum BodySize
        {
            _1x1,
            _2x2,
            _2x3,
            _3x3,

        }

        public static Point[] Sizes =
        [
            new Point(1 * 2, 1 * 2),
            new Point(2 * 2, 2 * 2),
            new Point(2 * 2, 3 * 2),
            new Point(3 * 2, 3 * 2),
        ];

        public static Point GetSize(BodySize size) => Sizes[(int)size];

        public enum Timers
        {
            BeforeSpawn,
            Trail,
            Death,
            Spawn,
        }
        protected Timer<Timers> _timer = new();
        public enum States
        {
            None,
            IsNull,
            IsSpawn,
            Damage,
            Move,
            Dead,
        }
        public State<States> State { get; private set; } = new State<States>(States.IsNull);

        protected Specs _specs = new();
        public Point Size { get; private set; } = new();
        public Vector2 SizeVector2 => Size.ToVector2() * _battleField.CellSize;
        public Point MapPosition;
        public Point GoalPosition;

        protected BattleField _battleField;

        protected int _tempoMove = 30;
        protected int _ticMove = 0;
        protected Vector2 _from;
        protected Vector2 _goal;

        protected float _ticScale = 0f;
        protected float _tempoScale = 60;
        protected float _scaleSpawn = 0f;
        protected float _alphaSpawn = 0f;

        protected float _tempoBeforeSpawn = 0f;

        public Shake Shake = new();

        protected Loop _loop;

        public Unit(BattleField battleField, Point mapPosition, Point size, int nbTurn = 2, int maxEnergy = 32, float tempoBeforeSpawn = 0f)
        {
            _passLevel = 3;
            _type = UID.Get<Unit>();
            _battleField = battleField;

            Size = size;
            SetSize(Size.ToVector2() * _battleField.CellSize);

            MapPosition = mapPosition;
            _specs.NbTurn = nbTurn;
            _specs.MaxEnergy = maxEnergy;

            float angleDelta = .005f;

            _loop = new Loop(this);
            _loop.SetLoop(0, -Geo.RAD_225 * angleDelta, Geo.RAD_225 * angleDelta, .001f, Loops.PINGPONG);
            _loop.Start();
            AddAddon(_loop);

            _timer.Set(Timers.Trail, Timer.Time(0, 0, .001f));
            _timer.Set(Timers.Death, Timer.Time(0, 0, .5f));
            _timer.Set(Timers.Spawn, Timer.Time(0, 1.5f, 0));
            _timer.Set(Timers.BeforeSpawn, tempoBeforeSpawn);

            _timer.Start(Timers.BeforeSpawn);
            _timer.Start(Timers.Trail);

            if (tempoBeforeSpawn == 0)
                State.Change(States.IsSpawn);

            Init();
        }
        public override Node Init()
        {
            _specs.Energy = _specs.MaxEnergy;
            _specs._ticTurn = _specs.NbTurn;

            return base.Init();
        }
        public int SetDamage(int damage)
        {
            //_specs.Energy += damage;

            return _specs.SetDamage(damage);
        }
        public void ExploseMe()
        {
            _battleField.DeleteInGrid(this);

            new FireExplosion().AppendTo(_parent).SetPosition(_rect.Center);

            KillMe();
        }
        public void TicTurn()
        {
            _specs._ticTurn--;
            if (_specs._ticTurn <= 0)
            {
                _specs._ticTurn = _specs.NbTurn;
                Action();
            }
        }
        // Vérifier si une position est valide (pas de chevauchement avec autres unités)
        public bool CanMoveTo<T>(List<T> units, Unit currentUnit, Point goal, int passLevel, int indexToIgnore) where T : Unit
        {
            int x = goal.X;
            int y = goal.Y;
            int width = Size.X;
            int height = Size.Y;

            var grid = _battleField.Grid;

            // Vérifier les limites de la grille
            if (x < 0 || x + width - 1 >= grid.Width || y < 0 || y + height - 1 >= grid.Height)
                return false;

            // Vérifier les obstacles statiques
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    var cell = grid.Get(x + i, y + j);
                    if (cell != null)
                    {
                        if (indexToIgnore == cell._index)
                            continue;

                        if (indexToIgnore != cell._index)
                            return false;

                        if (passLevel < cell._passLevel)
                            return false;
                    }
                }

            // Vérifier les autres unités
            foreach (var otherUnit in units)
            {
                if (otherUnit == currentUnit) continue; // Ignorer l'unité elle-même

                int otherX = (int)otherUnit.MapPosition.X;
                int otherY = (int)otherUnit.MapPosition.Y;
                int otherWidth = (int)otherUnit.Size.X;
                int otherHeight = (int)otherUnit.Size.Y;

                // Collision AABB (Axis-Aligned Bounding Box)
                if (x < otherX + otherWidth && x + width > otherX &&
                    y < otherY + otherHeight && y + height > otherY)
                {
                    Misc.Log("collision !!");
                    return false;
                }
            }

            return true;
        }
        public void MoveTo(Point goalPosition)
        {
            GoalPosition = goalPosition;

            _battleField.DeleteInGrid(this);
            _battleField.SetInGrid(this, goalPosition);

            _from = _battleField.MapPositionToVector2(MapPosition);
            _goal = _battleField.MapPositionToVector2(goalPosition);

            State.Change(States.Move);
        }
        public virtual void Action()
        {
            // Move 
            //var goalPosition = MapPosition + new Point(1, 0);

            //bool canMove = true;
            //// scan if he can go right
            //for (int j = 0; j < Size.Y; j++)
            //{
            //    if (!_battleField.IsNull(MapPosition + new Point(Size.X, j)))
            //    {
            //        canMove = false; 
            //        break;
            //    }
            //}

            //if (_battleField.IsInGrid(goalPosition) && canMove)
            //{
            //    MoveTo(goalPosition);
            //    Game1._soundRockSlide.Play(.1f * Game1._volumeMaster, .5f, 0f);
            //}
        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            _specs.Update(gameTime);

            UpdateRect();

            RunState(gameTime);

            return base.Update(gameTime);
        }
        protected virtual void RunState(GameTime gameTime)
        {
            switch (State.CurState)
            {
                case States.None:

                    if (_specs.Energy <= 0)
                        ExploseMe();

                    break;

                case States.Move:

                    if (_timer.On(Timers.Trail))
                        new Trail(AbsRect, .025f, Color.WhiteSmoke).AppendTo(_parent);

                    _x = Easing.GetValue(Easing.QuarticEaseOut, _ticMove, _from.X, _goal.X, _tempoMove);
                    _y = Easing.GetValue(Easing.QuarticEaseOut, _ticMove, _from.Y, _goal.Y, _tempoMove);

                    _ticMove++;

                    if (_ticMove > _tempoMove)
                    {
                        _ticMove = 0;

                        //_battleField.DeleteInGrid(this);

                        MapPosition = GoalPosition;

                        _battleField.SetInGrid(this);

                        State.Change(States.None);

                        Game1._soundClock.Play(.1f * Game1._volumeMaster, .5f, 0f);
                    }

                    break;

                case States.Damage:


                    break;

                case States.Dead:

                    break;

                case States.IsNull:

                    IsNull(gameTime);
                    break;

                case States.IsSpawn:

                    IsSpawn(gameTime);
                    break;
                default:
                    break;
            }
        }
        void IsNull(GameTime gameTime)
        {
            if (_timer.On(Timers.BeforeSpawn))
            {
                State.Change(States.IsSpawn);
                _ticScale = 0f;

                Game1._soundSword.Play(.2f * Game1._volumeMaster, .5f, 0f);
            }
        }
        void IsSpawn(GameTime gameTime)
        {
            _scaleSpawn = Easing.GetValue(Easing.QuinticEaseOut, _ticScale, 2, 1, _tempoScale);
            _alphaSpawn = 2 - _scaleSpawn;

            _ticScale++;
            if (_ticScale >= _tempoScale)
            {
                _scaleSpawn = 1;

                State.Change(States.None);
            }
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                var canvas = RectangleF.GetRectangleCentered(AbsRectF.Center, AbsRectF.GetSize() * _scaleSpawn);

                batch.FillRectangle(canvas.Extend(-10) + Shake.GetVector2(), Color.DarkSlateBlue * .5f);
                batch.Rectangle(canvas.Extend(-10) + Shake.GetVector2(), Color.DarkSlateBlue, 5f);

            }

            if (indexLayer == (int)Game1.Layers.HUD)
            {
                DrawEnergyBar(batch);
                
                //if (_state != (int)States.IsSpawn)
                    DrawTicTurn(batch);
            }


            return base.Draw(batch, gameTime, indexLayer);
        }
        void DrawEnergyBar(SpriteBatch batch)
        {
            var shake = Shake.GetVector2();
            var canvas = RectangleF.GetRectangleCentered(AbsRectF.Center, AbsRectF.GetSize() * _scaleSpawn);

            Color fg = Color.GreenYellow;
            Color bg = Color.Green;

            if (_specs.Energy <= 20)
            {
                fg = Color.Yellow;
                bg = Color.Red;
            }

            Vector2 pos = canvas.TopCenter + Vector2.UnitY * 2 - Vector2.UnitX * (_specs.MaxEnergy / 4) + shake * .5f;

            GFX.Bar(batch, pos, _specs.MaxEnergy / 2, 8, Color.Red * _alphaSpawn);
            GFX.Bar(batch, pos, _specs.Energy / 2, 8, fg * _alphaSpawn);
            GFX.BarLines(batch, pos, _specs.MaxEnergy / 2, 8, Color.Black * _alphaSpawn, 2);
            GFX.Bar(batch, pos - Vector2.UnitY * 2f, _specs.MaxEnergy / 2, 2, Color.White * .5f * _alphaSpawn);

            batch.CenterBorderedStringXY(Game1._fontMain, $"{_specs.Energy}", canvas.TopLeft + Vector2.One * 10 + shake * .5f, fg * _alphaSpawn, bg * _alphaSpawn);
        }
        void DrawTicTurn(SpriteBatch batch)
        {
            for (int i = 1; i < _specs.NbTurn; i++)
            {
                var pos = AbsRectF.BottomLeft;

                batch.Point(pos.X + i * 10, pos.Y - 10, 5, _specs._ticTurn == 1 ? Color.Red * _alphaSpawn : Color.Black * _alphaSpawn);

                if (i < _specs._ticTurn)
                    batch.Point(pos.X + i * 10, pos.Y - 10, 3, Color.Yellow * _alphaSpawn);

                batch.Circle(pos.X + i * 10, pos.Y - 10, 5, 8, Color.White * _alphaSpawn);
            }
        }
    }
}
