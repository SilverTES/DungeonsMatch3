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
            IsNull,
            IsSpawn,
            Damage,
            Move,
            Dead,
        }

        Specs _specs = new();

        //int _energy;
        public int NbTurn;
        int _ticTurn;

        public Point MapPosition;
        public Point GoalPosition;

        BattleField _battleField;

        int _tempoMove = 30;
        int _ticMove = 0;
        Vector2 _from;
        Vector2 _goal;

        protected float _ticScale = 0f;
        protected float _tempoScale = 60;
        protected float _scaleSpawn = 0f;
        protected float _alphaSpawn = 0f;

        protected float _tempoBeforeSpawn = 0f;

        public Shake Shake = new();

        Loop _loop;

        public Enemy(BattleField battleField, Point mapPosition, int nbTurn = 2, float tempoBeforeSpawn = 0f)
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
            _timer.SetTimer((int)Timers.BeforeSpawn, tempoBeforeSpawn);

            _timer.StartTimer((int)Timers.BeforeSpawn);

            _timer.StartTimer((int)Timers.Trail);

            SetState((int)States.IsNull);

            if (tempoBeforeSpawn == 0)
                ChangeState((int)States.IsSpawn);

            Init();
        }
        public override Node Init()
        {
            _specs.MaxEnergy = 32;
            _specs.Energy = _specs.MaxEnergy;

            _ticTurn = NbTurn;

            return base.Init();
        }
        public void SetDamage(int damage)
        {
            //_specs.Energy += damage;

            _specs.SetDamage(damage);
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

            // Move 
            var goalPosition = MapPosition + new Point(1, 0);

            if (_battleField.IsInGrid(goalPosition) && _battleField.IsNull(goalPosition))
            {
                MoveTo(goalPosition);
            }
        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            _specs.Update(gameTime);

            UpdateRect();

            RunState(gameTime);

            return base.Update(gameTime);
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.None:

                    if (_specs.Energy <= 0)
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

            base.RunState(gameTime);
        }
        void IsNull(GameTime gameTime)
        {
            if (_timer.OnTimer((int)Timers.BeforeSpawn))
            {
                ChangeState((int)States.IsSpawn);
                _ticScale = 0f;
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

                ChangeState((int)States.None);
            }
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
                var canvas = RectangleF.GetRectangleCentered(AbsRectF.Center, AbsRectF.GetSize() * _scaleSpawn);

                //batch.FillRectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue * .5f);
                //batch.Rectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue, 5f);

                //batch.Draw(Game1._texMob00, AbsRect, Color.White);

                Color color = Color.White;

                if (_state == (int)States.Damage)
                    color = Color.IndianRed * 1f;

                if (_state == (int)States.Dead)
                    color = Color.Red;

                //GFX.Draw(batch, Game1._texMob00, Color.White, _loop._current, AbsXY + (Game1._texMob00.Bounds.Size.ToVector2() / 2) + Shake.GetVector2(), Position.CENTER, Vector2.One * 1);

                var tex = Game1._texMob00;

                GFX.Draw(batch, tex, color * (_battleField.IsInGrid(MapPosition) ? 1f : .75f) * _alphaSpawn, _loop._current, AbsXY + (tex.Bounds.Size.ToVector2() / 2) + Shake.GetVector2(), Position.CENTER, Vector2.One * _scaleSpawn);

                //batch.CenterBorderedStringXY(Game1._fontMain, "Enemy", shake + AbsRectF.TopCenter, Color.Yellow, Color.Black);
                

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

            GFX.Bar(batch, canvas.TopCenter + Vector2.UnitY * 2 - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.MaxEnergy, 8, Color.Red * _alphaSpawn);
            GFX.Bar(batch, canvas.TopCenter + Vector2.UnitY * 2 - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.Energy, 8, fg * _alphaSpawn);
            GFX.BarLines(batch, canvas.TopCenter + Vector2.UnitY * 2 - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.MaxEnergy, 8, Color.Black * _alphaSpawn, 2);

            GFX.Bar(batch, canvas.TopCenter + (Vector2.UnitY * -0.25f) - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.MaxEnergy, 2, Color.White * .5f * _alphaSpawn);

            batch.CenterBorderedStringXY(Game1._fontMain2, $"{_specs.Energy}", canvas.TopLeft + Vector2.One * 20 + shake * .5f, fg * _alphaSpawn, bg * _alphaSpawn);
        }
        void DrawTicTurn(SpriteBatch batch)
        {
            for (int i = 1; i < NbTurn; i++)
            {
                var pos = AbsRectF.BottomLeft;
                batch.Point(pos.X + i * 10, pos.Y - 10, 5, _ticTurn == 1 ? Color.Red * _alphaSpawn : Color.Black * _alphaSpawn);

                if (i < _ticTurn)
                    batch.Point(pos.X + i * 10, pos.Y - 10, 3, Color.Yellow * _alphaSpawn);

                batch.Circle(pos.X + i * 10, pos.Y - 10, 5, 8, Color.White * _alphaSpawn);
            }
        }
    }
}
