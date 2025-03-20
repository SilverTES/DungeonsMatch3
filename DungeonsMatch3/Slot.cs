using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;

namespace DungeonsMatch3
{
    class Slot : Node   
    {
        bool _flip = false;

        MouseState _mouse;

        Vector2 _mousePos = new Vector2();



        int _points = 0;
        
        Arena _arena;
        BattleField _battleField;
        public Slot(Arena arena, BattleField battleField) 
        {
            SetSize(320, 160);
            _arena = arena;
            _battleField = battleField;
        }
        public Slot Flip(bool flip = true)
        {
            _flip = flip;
            return this;
        }
        public override Node Update(GameTime gameTime)
        {
            _mouse = Game1.Mouse;

            _mousePos.X = Game1._mousePos.X;
            _mousePos.Y = Game1._mousePos.Y;

            UpdateRect();

            if (Misc.PointInRect(_mousePos, AbsRect) && ButtonControl.OnePress("OnClick", _mouse.LeftButton == ButtonState.Pressed) && _arena.GetState() == (int)Arena.States.Action)
            {
                //Misc.Log($"Coucou {_index}");

                _points += _arena.TotalAttack;

                _arena.ChangeState((int)Arena.States.FinishTurn);
                _battleField.DoAction();

                Game1._soundBlockHit.Play(.8f * Game1._volumeMaster, .1f, 0f);

                new PopInfo($"+{_points}", Color.White, _arena.CurrentColor, 0, 16, 32).SetPosition(_mousePos - Vector2.UnitY * 20).AppendTo(_parent);
                new FxExplose(_mousePos, _arena.CurrentColor, 17, 20).AppendTo(_parent);


            }


            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRectF.Extend(0), Color.Black * .5f);
                //batch.Rectangle(AbsRectF.Extend(0), Color.DarkSlateBlue, 5f);

                //batch.CenterBorderedStringXY(Game1._fontMain, "Enemy", shake + AbsRectF.TopCenter, Color.Yellow, Color.Black);

                //batch.Draw(Game1._texHero00, AbsRect, Color.White);

                //var tex = Game1._texHero00;

                //GFX.Draw(batch, tex, Color.White, 0, AbsXY + (tex.Bounds.Size.ToVector2() / 2), Position.CENTER, Vector2.One, _flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

                batch.BevelledRectangle(AbsRectF, Vector2.One * 8, Color.DarkSlateBlue, 3f);


                //batch.CenterBorderedStringXY(Game1._fontMain2, "Slot", AbsRectF.TopCenter, Color.Yellow, Color.Black);
                batch.CenterBorderedStringXY(Game1._fontMain2, $"{_points}", AbsRectF.TopCenter - Vector2.UnitY * 10, Color.Yellow, Color.Black);
            }

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
