using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;

namespace DungeonsMatch3
{
    class Enemy : Node  
    {
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

        public Enemy(BattleField battleField, Point mapPosition, int nbTurn = 2)
        {
            _type = UID.Get<Enemy>();
            _battleField = battleField;
            SetSize(BattleField.CellSize.ToVector2());

            ChangeState((int)States.None);

            MapPosition = mapPosition;
            NbTurn = nbTurn;

            Init();
        }
        public override Node Init()
        {
            _energy = 40;
            _ticTurn = NbTurn;

            return base.Init();
        }
        public void TicTurn()
        {
            Shake.SetIntensity(4);

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

            var goalPosition = MapPosition + new Point(-1, 0);

            if (_battleField.IsInGrid(goalPosition))
                MoveTo(goalPosition);
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            RunState(gameTime);

            return base.Update(gameTime);
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.None:

                    break;

                case States.Move:

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

                batch.FillRectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue * .5f);
                batch.Rectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue, 5f);

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
                batch.Point(pos.X + i * 8 + 10, pos.Y - 20, 5, Color.Black);

                if (i < _ticTurn)
                    batch.Point(pos.X + i * 8 + 10, pos.Y - 20, 3, Color.Yellow);
            }
        }
    }
}
