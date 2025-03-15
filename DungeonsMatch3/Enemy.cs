using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Physics;
using static Mugen.Core.Addon;

namespace DungeonsMatch3
{
    class Enemy : Node  
    {
        public enum Timers
        {
            BeforeSpawn,
            Trail,
            Death,
            Spawn,
            Count,
        }
        TimerEvent _timer;
        public enum States
        {
            None,
            Move,
            Dead,
        }

        int _energy;
        public int NbTurn;
        int _ticTurn;

        public Point MapPosition;
        public Point GoalPosition;

        BattleField _battleField;

        int _tempoMove = 30;
        int _ticMove = 0;
        Vector2 _from;
        Vector2 _goal;

        public Shake Shake = new();

        Loop _loop;

        public Enemy(BattleField battleField, Point mapPosition, int nbTurn = 2)
        {
            _type = UID.Get<Enemy>();
            _battleField = battleField;

            SetSize(_battleField.CellSize.ToVector2());

            ChangeState((int)States.None);

            MapPosition = mapPosition;
            NbTurn = nbTurn;

            float angleDelta = .005f;
            _loop = new Addon.Loop(this);
            _loop.SetLoop(0, -Geo.RAD_225 * angleDelta, Geo.RAD_225 * angleDelta, .001f, Loops.PINGPONG);
            _loop.Start();
            AddAddon(_loop);

            _timer = new TimerEvent((int)Timers.Count);
            _timer.SetTimer((int)Timers.Trail, TimerEvent.Time(0, 0, .001f));
            _timer.SetTimer((int)Timers.Death, TimerEvent.Time(0, 0, .5f));
            _timer.SetTimer((int)Timers.Spawn, TimerEvent.Time(0, 1.5f, 0));
            //_timer.SetTimer((int)Timers.BeforeSpawn, tempoBeforeSpawn);

            _timer.StartTimer((int)Timers.BeforeSpawn);

            _timer.StartTimer((int)Timers.Trail);

            Init();
        }
        public override Node Init()
        {
            _energy = 32;
            _ticTurn = NbTurn;

            return base.Init();
        }
        public void SetDamage(int damage)
        {
            _energy += damage;
        }
        public void ExploseMe()
        {
            _battleField.DeleteInGrid(this);
            KillMe();
        }
        public void TicTurn()
        {
            //Shake.SetIntensity(4);

            _ticTurn--;
            if (_ticTurn <= 0)
            {
                _ticTurn = NbTurn;
                Action();
            }
        }
        public void Action()
        {
            //Console.WriteLine("< Action >");

            var goalPosition = MapPosition + new Point(0, 1);

            if (_battleField.IsInGrid(goalPosition) && _battleField.IsNull(goalPosition))
            {
                MoveTo(goalPosition);
            }
        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            UpdateRect();

            RunState(gameTime);

            return base.Update(gameTime);
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.None:

                    if (_energy <= 0)
                        ExploseMe();

                    break;

                case States.Move:

                    if (_timer.OnTimer((int)Timers.Trail))
                        new Trail(AbsRectF.Center, Vector2.One, .025f, Color.WhiteSmoke).AppendTo(_parent);

                    _x = Easing.GetValue(Easing.QuarticEaseOut, _ticMove, _from.X, _goal.X, _tempoMove);
                    _y = Easing.GetValue(Easing.QuarticEaseOut, _ticMove, _from.Y, _goal.Y, _tempoMove);

                    _ticMove++;

                    if (_ticMove > _tempoMove)
                    {
                        _ticMove = 0;

                        MapPosition = GoalPosition;
                        
                        _battleField.SetInGrid(this);

                        ChangeState((int)States.None);
                    }

                    break;

                case States.Dead:

                    break;

                default:
                    break;
            }

            base.RunState(gameTime);
        }
        public void MoveTo(Point goalPosition)
        {
            GoalPosition = goalPosition;

            _battleField.DeleteInGrid(this);

            _from = _battleField.MapPositionToVector2(MapPosition);
            _goal = _battleField.MapPositionToVector2(goalPosition);

            ChangeState((int)States.Move);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                var shake = Shake.GetVector2();

                //batch.FillRectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue * .5f);
                //batch.Rectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue, 5f);
                
                //batch.Draw(Game1._texMob00, AbsRect, Color.White);

                GFX.Draw(batch, Game1._texMob00, Color.White, _loop._current, AbsXY + (Game1._texMob00.Bounds.Size.ToVector2() / 2) + Shake.GetVector2(), Position.CENTER, Vector2.One * 1);

                //batch.CenterBorderedStringXY(Game1._fontMain, "Enemy", shake + AbsRectF.TopCenter, Color.Yellow, Color.Black);
                batch.CenterBorderedStringXY(Game1._fontMain, $"{_energy}", shake + AbsRectF.TopLeft + Vector2.One * 24, Color.Yellow, Color.Black);

                //if (_state != (int)States.Move)
                    DrawTicTurn(batch);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }

        void DrawTicTurn(SpriteBatch batch)
        {
            for (int i = 1; i < NbTurn; i++)
            {
                var pos = AbsRectF.BottomLeft;
                batch.Point(pos.X + i * 10, pos.Y - 10, 5, _ticTurn == 1 ? Color.Red : Color.Black);

                if (i < _ticTurn)
                    batch.Point(pos.X + i * 10, pos.Y - 10, 3, Color.Yellow);

                batch.Circle(pos.X + i * 10, pos.Y - 10, 5, 8, Color.White);
            }
        }
    }
}
