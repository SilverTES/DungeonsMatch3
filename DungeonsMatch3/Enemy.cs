using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Physics;
using System.Collections.Generic;


namespace DungeonsMatch3
{
    internal class Enemy : Unit
    {
        bool top, bottom, left, right;
        public Enemy(BattleField battleField, Point mapPosition, Point size, int nbTurn = 2, int maxEnergy = 32, float tempoBeforeSpawn = 0) : base(battleField, mapPosition, size, nbTurn, maxEnergy, tempoBeforeSpawn)
        {
            _type = UID.Get<Enemy>();
            _passLevel = 2;

        }
        protected override void RunState(GameTime gameTime)
        {
            base.RunState(gameTime);

            switch (State.CurState)
            {
                case States.None:

                    //PathFinding.Neighbours(_battleField.Grid, MapPosition, Size, out top, out bottom, out left, out right);

                    break;
                case States.IsNull:
                    break;
                case States.IsSpawn:
                    break;
                case States.Damage:
                    break;
                case States.Move:

                    break;
                case States.Dead:
                    break;
                default:
                    break;
            }

        }
        public override Node Update(GameTime gameTime)
        {
            // Debug
            if (MapPosition.X == _battleField.Grid.Width - Size.X)
            {
                ExploseMe();
            }

            return base.Update(gameTime);
        }
        public override void Action()
        {
            if (State.CurState == States.None)
            {

                int passLevel = 2;


                List<Point> path = new();

                for (int row =  0; row <= _battleField.Grid.Height - Size.Y; row++)
                {
                    path = PathFinding.FindPath(_battleField.Grid, MapPosition, new Point(_battleField.Grid.Width - Size.X, 0), Size, passLevel, _index);
                    if (path != null)
                        break;
                }
                if (path == null) return;
                if (path.Count <= 1) return;
                // Move 
                var goalPosition = path[1];

                var delta = goalPosition - MapPosition;

                var neighbours = PathFinding.Neighbours(_battleField.Grid, MapPosition, Size, out top, out bottom, out left, out right);
                if (neighbours.Count > 0)
                {
                    //Misc.Log($"NeighBour = {neighbours.Count}");
                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        if (neighbours[i]._type == UID.Get<Enemy>())
                            passLevel = 1;
                    }

                }

                if (delta.X == 0 && delta.Y < 0 && top) return; else passLevel = 2;// voisin en haut
                if (delta.X == 0 && delta.Y > 0 && bottom) return; else passLevel = 2;// voisin en bas                
                if (delta.X < 0 && delta.Y == 0 && left) return; else passLevel = 2;// voisin à gauche
                if (delta.X > 0 && delta.Y == 0 && right) return; else passLevel = 2;// voisin à droite

                //passLevel = 1;

                for (int row = 0; row <= _battleField.Grid.Height - Size.Y; row++)
                {
                    path = PathFinding.FindPath(_battleField.Grid, MapPosition, new Point(_battleField.Grid.Width - Size.X, 0), Size, passLevel, _index);
                    if (path != null)
                        break;
                }
                if (path == null) return;
                if (path.Count <= 1) return;
                // Move 
                goalPosition = path[1];

                if (!CanMoveTo(_battleField.GroupOf<Enemy>(), this, goalPosition, 1, _index))
                    return;

                //bool refind = false;

                //if (delta.X == 0 && delta.Y < 0 && !top) refind = true; // voisin en haut
                //if (delta.X == 0 && delta.Y > 0 && !bottom) refind = true; // voisin en bas                
                //if (delta.X < 0 && delta.Y == 0 && !left) refind = true; // voisin à gauche
                //if (delta.X > 0 && delta.Y == 0 && !right) refind = true; // voisin à droite

                //if (refind)
                //{
                //    Misc.Log("ReFind");
                //    passLevel = 2;

                //    for (int row = 0; row <= _battleField.Grid.Height - Size.Y; row++)
                //    {
                //        path = PathFinding.FindPath(_battleField.Grid, MapPosition, new Point(_battleField.Grid.Width - Size.X, 0), Size, passLevel, _index);
                //        if (path != null)
                //            break;
                //    }

                //    if (path == null)
                //        return;

                //    if (path.Count <= 1)
                //        return;

                //    goalPosition = path[1];
                //}

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
                //if (_battleField.CanSetInGrid(this))
                //    canMove = false;

                //if (_battleField.IsInGrid(goalPosition) && canMove)
                {
                    MoveTo(goalPosition);
                    //Game1._soundRockSlide.Play(.1f * Game1._volumeMaster, .5f, 0f);
                }
            }

        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            base.Draw(batch, gameTime, indexLayer);

            if (indexLayer == (int)Game1.Layers.Main)
            {
                Color color = Color.White;

                if (State.CurState == States.Damage)
                    color = Color.IndianRed * 1f;

                if (State.CurState == States.Dead)
                    color = Color.Red;

                Texture2D tex = null;

                if (Size == GetSize(BodySize._1x1)) tex = Game1._texAvatar1x1;
                if (Size == GetSize(BodySize._2x2)) tex = Game1._texAvatar2x2;
                if (Size == GetSize(BodySize._2x3)) tex = Game1._texAvatar2x3;
                if (Size == GetSize(BodySize._3x3)) tex = Game1._texAvatar3x3;

                if (tex != null)
                {
                    //GFX.Draw(batch, tex, Color.Black * .5f * (_battleField.IsInGrid(MapPosition) ? 1f : .75f) * _alphaSpawn, _loop._current, AbsXY + (tex.Bounds.Size.ToVector2() / 2) + Shake.GetVector2() + Vector2.One * 4, Position.CENTER, Vector2.One * _scaleSpawn);

                    GFX.Draw(batch, tex, color * (_battleField.IsInGrid(MapPosition) ? 1f : .75f) * _alphaSpawn, _loop._current, AbsXY + (tex.Bounds.Size.ToVector2() / 2) + Shake.GetVector2(), Position.CENTER, Vector2.One * _scaleSpawn);
                }

            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                //batch.CenterBorderedStringXY(Game1._fontMain, $"{(States)_state}", Shake.GetVector2() + AbsRectF.BottomCenter, Color.Yellow, Color.Black);
                //batch.CenterBorderedStringXY(Game1._fontMain, $"{_passLevel}", Shake.GetVector2() + AbsRectF.TopRight, Color.Cyan, Color.Black);

                //_battleField.DrawPath(batch, MapPosition, new Point(_battleField.Grid.Width - Size.X, 0), Size, 2, _index, Color.Red * .1f);

                //if (top) batch.Point(AbsRectF.TopCenter + Vector2.UnitY * 4, 3, Color.Yellow);
                //if (bottom) batch.Point(AbsRectF.BottomCenter - Vector2.UnitY * 4, 3, Color.Yellow);
                //if (left) batch.Point(AbsRectF.LeftMiddle + Vector2.UnitY * 4, 3, Color.Yellow);
                //if (right) batch.Point(AbsRectF.RightMiddle - Vector2.UnitY * 4, 3, Color.Yellow);
            }

            return this;  
        }
    }
}
