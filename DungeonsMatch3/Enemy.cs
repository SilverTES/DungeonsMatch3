using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;


namespace DungeonsMatch3
{
    internal class Enemy : Unit
    {
        public Enemy(BattleField battleField, Point mapPosition, Point size, int nbTurn = 2, int maxEnergy = 32, float tempoBeforeSpawn = 0) : base(battleField, mapPosition, size, nbTurn, maxEnergy, tempoBeforeSpawn)
        {
            _type = UID.Get<Enemy>();
        }
        public override Node Update(GameTime gameTime)
        {
            return base.Update(gameTime);
        }
        public override void Action()
        {
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
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            base.Draw(batch, gameTime, indexLayer);

            if (indexLayer == (int)Game1.Layers.Main)
            {
                Color color = Color.White;

                if (_state == (int)States.Damage)
                    color = Color.IndianRed * 1f;

                if (_state == (int)States.Dead)
                    color = Color.Red;

                Texture2D tex = null;

                if (_size == Size1x1) tex = Game1._texAvatar1x1;
                if (_size == Size2x2) tex = Game1._texAvatar2x2;
                if (_size == Size2x3) tex = Game1._texAvatar2x3;
                if (_size == Size3x3) tex = Game1._texAvatar3x3;

                if (tex != null)
                {
                    GFX.Draw(batch, tex, Color.Black * .5f * (_battleField.IsInGrid(MapPosition) ? 1f : .75f) * _alphaSpawn, _loop._current, AbsXY + (tex.Bounds.Size.ToVector2() / 2) + Shake.GetVector2() + Vector2.One * 4, Position.CENTER, Vector2.One * _scaleSpawn);
                    GFX.Draw(batch, tex, color * (_battleField.IsInGrid(MapPosition) ? 1f : .75f) * _alphaSpawn, _loop._current, AbsXY + (tex.Bounds.Size.ToVector2() / 2) + Shake.GetVector2(), Position.CENTER, Vector2.One * _scaleSpawn);
                }

                //batch.CenterBorderedStringXY(Game1._fontMain, "Enemy", shake + AbsRectF.TopCenter, Color.Yellow, Color.Black);
            }

            return this;  
        }
    }
}
