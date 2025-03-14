using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using System;


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

        int _energy = 40;
        public static int NbTurnToMove = 2;
        int _ticTurnToMove = 2;

        public Point MapPosition;
        public Point GoalPosition;

        BattleField _battleField;

        int _tempoMove = 30;
        int _ticMove = 0;
        Vector2 _from;
        Vector2 _goal;

        public Shake Shake = new();

        public Enemy(BattleField battleField)
        {
            _type = UID.Get<Enemy>();
            _battleField = battleField;
            SetSize(BattleField.CellSize.ToVector2());

            ChangeState((int)States.None);
        }
        public void TicTurn()
        {
            Shake.SetIntensity(4);

            _ticTurnToMove--;
            if (_ticTurnToMove < 0)
            {
                _ticTurnToMove = NbTurnToMove;
                Action();
            }
        }
        public void Action()
        {
            Console.WriteLine("< Action >");


            GoalPosition = MapPosition + new Point(-1, 0);

            if (_battleField.IsInGrid(GoalPosition))
                MoveTo(GoalPosition);
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
        public void MoveTo(Point mapPosition)
        {
            _from = _battleField.MapPositionToVector2(MapPosition);
            _goal = _battleField.MapPositionToVector2(mapPosition);

            ChangeState((int)States.Move);
            //Console.WriteLine("Gem Move Down");
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                var shake = Shake.GetVector2();

                batch.FillRectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue * .5f);
                batch.Rectangle(AbsRectF.Extend(-10) + shake, Color.DarkSlateBlue, 5f);

                batch.CenterBorderedStringXY(Game1._fontMain, "Enemy", shake + AbsRectF.TopCenter, Color.Yellow, Color.Black);
                batch.CenterBorderedStringXY(Game1._fontMain, $"{_energy}", shake + AbsRectF.BottomCenter, Color.Yellow, Color.Black);

                if (_state != (int)States.Move)
                    for (int i = 0; i < NbTurnToMove; i++)
                    {
                        var pos = AbsRectF.BottomLeft;
                        batch.Point(pos.X + i * 10 + 20, pos.Y - 20, 4, Color.Black);

                        if  (i < _ticTurnToMove)
                            batch.Point(pos.X + i * 10 + 20, pos.Y - 20, 4, Color.Yellow);
                    }
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
