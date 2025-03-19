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
        static Point _size1x1 = new Point(1, 1);
        static Point _size1x2 = new Point(1, 2);
        static Point _size2x1 = new Point(2, 1);
        static Point _size2x2 = new Point(2, 2);
        static Point _size2x3 = new Point(2, 3);
        static Point _size3x2 = new Point(3, 3);
        static Point _size3x3 = new Point(3, 3);

        public static Point[] Sizes =
        [
            _size1x1, 
            _size2x2,
            _size2x3, 
            _size3x3
        ];

        public static Point Size1x1 => _size1x1;
        public static Point Size1x2 => _size1x2;
        public static Point Size2x1 => _size2x1;
        public static Point Size2x2 => _size2x2;
        public static Point Size2x3 => _size2x3;
        public static Point Size3x2 => _size3x2;
        public static Point Size3x3 => _size3x3;

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

        Point _size = new();
        public Point Size => _size;
        public Vector2 SizeVector2 => _size.ToVector2() * _battleField.CellSize;

        //int _energy;

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

        public Enemy(BattleField battleField, Point mapPosition, Point size, int nbTurn = 2, int maxEnergy = 32, float tempoBeforeSpawn = 0f)
        {
            _type = UID.Get<Enemy>();
            _battleField = battleField;

            _size = size;
            SetSize(_size.ToVector2() * _battleField.CellSize);

            MapPosition = mapPosition;
            _specs.NbTurn = nbTurn;
            _specs.MaxEnergy = maxEnergy;

            float angleDelta = .005f;

            _loop = new Loop(this);
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
            KillMe();
        }
        public void TicTurn()
        {
            //Shake.SetIntensity(4);

            _specs._ticTurn--;
            if (_specs._ticTurn <= 0)
            {
                _specs._ticTurn = _specs.NbTurn;
                Action();
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
        public void Action()
        {
            //Console.WriteLine("< Action >");

            // Move 
            var goalPosition = MapPosition + new Point(1, 0);

            bool canMove = true;
            // scan if he can go right
            for (int j = 0; j < Size.Y; j++)
            {
                if (!_battleField.IsNull(MapPosition + new Point(Size.X, j)))
                {
                    canMove = false; 
                    break;
                }
            }

            if (_battleField.IsInGrid(goalPosition) && canMove)
            {
                MoveTo(goalPosition);
                Game1._soundRockSlide.Play(.1f * Game1._volumeMaster, .5f, 0f);
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
                        new Trail(AbsRect, .025f, Color.WhiteSmoke).AppendTo(_parent);

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

                ChangeState((int)States.None);
            }
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                var canvas = RectangleF.GetRectangleCentered(AbsRectF.Center, AbsRectF.GetSize() * _scaleSpawn);

                batch.FillRectangle(AbsRectF.Extend(-10) + Shake.GetVector2(), Color.DarkSlateBlue * .5f);
                batch.Rectangle(AbsRectF.Extend(-10) + Shake.GetVector2(), Color.DarkSlateBlue, 5f);

                //batch.Draw(Game1._texMob00, AbsRect, Color.White);

                Color color = Color.White;

                if (_state == (int)States.Damage)
                    color = Color.IndianRed * 1f;

                if (_state == (int)States.Dead)
                    color = Color.Red;

                //GFX.Draw(batch, Game1._texMob00, Color.White, _loop._current, AbsXY + (Game1._texMob00.Bounds.Size.ToVector2() / 2) + Shake.GetVector2(), Position.CENTER, Vector2.One * 1);

                Texture2D tex = null; 
                
                if (_size == Size1x1) tex = Game1._texAvatar1x1;
                if (_size == Size2x2) tex = Game1._texAvatar2x2;
                if (_size == Size2x3) tex = Game1._texAvatar2x3;
                if (_size == Size3x3) tex = Game1._texAvatar3x3;

                if (tex != null)
                    GFX.Draw(batch, tex, color * (_battleField.IsInGrid(MapPosition) ? 1f : .75f) * _alphaSpawn, _loop._current, AbsXY + (tex.Bounds.Size.ToVector2() / 2) + Shake.GetVector2(), Position.CENTER, Vector2.One * _scaleSpawn);

                //batch.CenterBorderedStringXY(Game1._fontMain, "Enemy", shake + AbsRectF.TopCenter, Color.Yellow, Color.Black);

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

            GFX.Bar(batch, canvas.TopCenter + Vector2.UnitY * 2 - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.MaxEnergy, 8, Color.Red * _alphaSpawn);
            GFX.Bar(batch, canvas.TopCenter + Vector2.UnitY * 2 - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.Energy, 8, fg * _alphaSpawn);
            GFX.BarLines(batch, canvas.TopCenter + Vector2.UnitY * 2 - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.MaxEnergy, 8, Color.Black * _alphaSpawn, 2);

            GFX.Bar(batch, canvas.TopCenter + (Vector2.UnitY * -0.25f) - Vector2.UnitX * (_specs.MaxEnergy / 2) + shake * .5f, _specs.MaxEnergy, 2, Color.White * .5f * _alphaSpawn);

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
