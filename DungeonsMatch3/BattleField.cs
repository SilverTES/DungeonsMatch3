﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;
using System.Linq;


namespace DungeonsMatch3
{
    class BattleField : Node
    {
        public enum States
        {
            Play,
            DoAction,

        }

        public Point GridSize = new Point(14, 5);
        public Point CellSize = new Point(128, 128);

        List2D<Node> _grid;

        Vector2 _mousePos = new Vector2();
        RectangleF _rectOver;
        RectangleF _prevRectOver;
        Point _mapPostionOver;
        Vector2 _mapMouseOver;

        MouseState _mouse;

        public RectangleF Rect => _rect;

        Arena _arena;
        public BattleField(Arena arena) 
        {
            _arena = arena;
            _grid = new List2D<Node>(GridSize.X, GridSize.Y);

            SetSize(CellSize.X * GridSize.X, CellSize.Y * GridSize.Y);

            _rectOver = new RectangleF(0, 0, CellSize.X, CellSize.Y);

            CreateGrid();

            ChangeState((int)States.Play);
        }
        public void Setup(SizeTab sizeTab)
        {
            GridSize = sizeTab.Grid;
            CellSize = sizeTab.Cell;

            _rect.Width = GridSize.X * CellSize.X;
            _rect.Height = GridSize.Y * CellSize.Y;

            _rectOver = new RectangleF(0, 0, CellSize.X, CellSize.Y);

            _grid.ResizeVecObject2D(GridSize.X, GridSize.Y);

            CreateGrid();
        }
        public void CreateGrid()
        {
            for (int i = 0; i < _grid._width; i++)
            {
                for (int j = 0; j < _grid._height; j++)
                {
                    _grid.Put(i, j, null);
                }
            }
        }
        public void ClearGrid()
        {
            for (int i = 0; i < _grid._width; i++)
            {
                for (int j = 0; j < _grid._height; j++)
                {
                    DeleteInGrid(new Point(i, j));
                }
            }

            KillAll(UID.Get<Enemy>());
        }
        public void AddRandomEnemy()
        {
            for (int i = 0; i < _grid._width; i++)
            {
                AddInGrid(new Enemy(this, new Point(i, 0), Misc.Rng.Next(2,6)));
            }
        }
        public Enemy FindClosestEnemy()
        {
            var enemies = GroupOf<Enemy>();

            enemies.Sort((e1, e2) => e1.MapPosition.Y.CompareTo(e2.MapPosition.Y));

            return enemies.Last();
        }
        public override Node Update(GameTime gameTime)
        {
            _mouse = Game1.Mouse;

            UpdateRect();

            _mousePos.X = Game1._mousePos.X - _x;
            _mousePos.Y = Game1._mousePos.Y - _y;

            _mapPostionOver.X = (int)(_mousePos.X / CellSize.X);
            _mapPostionOver.Y = (int)(_mousePos.Y / CellSize.Y);

            _prevRectOver = _rectOver;

            _rectOver.X = _mapPostionOver.X * CellSize.X + _x;
            _rectOver.Y = _mapPostionOver.Y * CellSize.Y + _y;

            _mapMouseOver.X = _rectOver.X + CellSize.X / 2;
            _mapMouseOver.Y = _rectOver.Y + CellSize.Y / 2;

            RunState(gameTime);

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.Play:

                    if (!IsInGrid(_mousePos))
                        break;

                    if (ButtonControl.OnePress("Attack", _mouse.LeftButton == ButtonState.Pressed && IsInGrid(_mapPostionOver)) && _arena.State == Arena.States.Action)
                    {
                        Attack(_arena, _mapPostionOver);
                    }

                    if (GroupOf<Enemy>().Count == 0)
                        AddRandomEnemy();

                    var enemy = FindClosestEnemy();
                    if (enemy != null && _arena.State == Arena.States.Action)
                    {
                        Attack(_arena, enemy.MapPosition);
                    }


                    break;
                case States.DoAction:

                    ChangeState((int)States.Play);

                    break;
                default:
                    break;
            }

            base.RunState(gameTime);
        }
        public void DoAction()
        {
            var enemies = GroupOf<Enemy>();

            enemies.Sort((e1, e2) => e1.MapPosition.X.CompareTo(e2.MapPosition.X));

            foreach (var enemy in enemies)
            { 
                enemy.TicTurn();
            }

            ChangeState((int)States.DoAction);
        }
        public void Attack(Arena arena, Point mapPosition)
        {   
            var node = _grid.Get(mapPosition.X, mapPosition.Y);

            if (node != null)
            {
                if (node._type == UID.Get<Enemy>())
                {
                    var enemy = (Enemy)node;
                    enemy.SetDamage(-arena.TotalAttack);
                    enemy.Shake.SetIntensity(8, .1f);

                    Game1._soundSword.Play(.8f * Game1._volumeMaster, 1f, 0f);
                    new PopInfo($"-{_arena.TotalAttack}", Color.White, _arena.CurrentColor, 0, 16, 32).SetPosition(enemy.AbsRectF.TopCenter).AppendTo(_parent);

                    arena.ChangeState((int)Arena.States.FinishTurn);
                }
            }

        }
        public void AddInGrid(Enemy enemy)
        {
            _grid.Put(enemy.MapPosition.X, enemy.MapPosition.Y, enemy);
            enemy.SetPosition(MapPositionToVector2(enemy.MapPosition));
            enemy.AppendTo(this);
        }
        public void SetInGrid(Enemy enemy)
        {
            _grid.Put(enemy.MapPosition.X, enemy.MapPosition.Y, enemy);
        }
        public void DeleteInGrid(Enemy enemy)
        {
            _grid.Put(enemy.MapPosition.X, enemy.MapPosition.Y, null);
        }
        public void DeleteInGrid(Point mapPosition)
        {
            _grid.Put(mapPosition.X, mapPosition.Y, null);
        }
        public bool IsInGrid(Vector2 position)
        {
            return Misc.PointInRect(position + XY, _rect);
        }
        public bool IsInGrid(Point mapPosition)
        {
            if (mapPosition.X < 0 || mapPosition.X >= _grid._width || mapPosition.Y < 0 || mapPosition.Y >= _grid._height)
                return false;

            return true;
        }
        public bool IsNull(Point mapPosition)
        {
            return _grid.Get(mapPosition.X, mapPosition.Y) == null;
        }
        public Vector2 MapPositionToVector2(Point mapPosition)
        {
            return (mapPosition * CellSize).ToVector2();// + CellSize.ToVector2() / 2;
        }
        public Vector2 MapPositionToVector2(int i, int j)
        {
            return MapPositionToVector2(new Point(i, j));
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .25f);
                //batch.Rectangle(AbsRectF.Extend(10), Color.DarkSlateBlue, 3f);

                if (_rectOver != _prevRectOver && IsInGrid(_mousePos))
                {
                    new Trail(_rectOver.Center, Vector2.One, .025f, Color.WhiteSmoke * .75f).AppendTo(_parent);
                    //new FxExplose(_rectOver.Center, Color.Gray).AppendTo(_parent);
                    //Console.WriteLine("RectOver !=");
                }

                batch.Grid(AbsXY, _rect.Width, _rect.Height, CellSize.X, CellSize.Y, Color.Black * .5f, 1f);


            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                //batch.LeftTopString(Game1._fontMain, $"NB close Gems = {FindSameGems(_currentGemOver).Count}", Vector2.UnitX * 20 + Vector2.UnitY * 120, Color.Yellow);
                //batch.LeftTopString(Game1._fontMain, $"{_mousePos}", Vector2.One * 20, Color.Yellow);
                //batch.LeftTopString(Game1._fontMain, $"{(States)_state}", Vector2.One * 20 + Vector2.UnitY * 80, Color.Cyan);
                //batch.LeftTopString(Game1._fontMain, $"{_currentColor} {_gemSelecteds.Count}", Vector2.One * 20 + Vector2.UnitY * 120, _currentColor);

                for (int i = 0; i < _grid._width; i++)
                {
                    for (int j = 0; j < _grid._height; j++)
                    {
                        var node = _grid.Get(i, j);
                        if (node != null)
                        {
                            batch.CenterStringXY(Game1._fontMain, $"{ node._type }", AbsXY + MapPositionToVector2(i, j) + CellSize.ToVector2() / 2, Color.White * .5f);
                        }
                    }
                }

                //batch.Point(_mapMouseOver, 4, Color.OrangeRed);

                batch.CenterStringXY(Game1._fontMain, $"{(States)_state}", AbsRectF.TopCenter, Color.Cyan);

            }

            if (indexLayer == (int)Game1.Layers.HUD)
            {
                //if (IsInGrid(_mousePos))
                //    batch.Rectangle(_rectOver, Color.Cyan * .5f, 3f);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
