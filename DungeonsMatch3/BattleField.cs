﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;
using System.Linq;
using System.Net.Http.Headers;


namespace DungeonsMatch3
{
    class BattleField : Node
    {
        public enum Timers
        {
            None,
            Damage,
        }
        TimerEvent _timer;
        public enum States
        {
            Play,
            DoDamage,
            DoAction,

        }

        //public Point Target = new Point();

        public Point GridSize;
        public Vector2 CellSize;

        List2D<Node> _grid;

        Vector2 _mousePos = new Vector2();
        RectangleF _rectOver;
        RectangleF _prevRectOver;
        Point _mapPositionOver;
        public Point MapPositionOver => _mapPositionOver;
        Vector2 _mouseCellCenterOver;

        MouseState _mouse;

        public RectangleF Rect => _rect;

        Arena _arena;

        Addon.Loop _loop;
        public BattleField(Arena arena) 
        {
            _arena = arena;
            _grid = new List2D<Node>(GridSize.X, GridSize.Y);

            SetSize(CellSize.X * GridSize.X, CellSize.Y * GridSize.Y);

            _rectOver = new RectangleF(0, 0, CellSize.X, CellSize.Y);

            CreateGrid();

            ChangeState((int)States.Play);

            _timer = new TimerEvent(Enums.Count<Timers>());
            _timer.SetTimer((int)Timers.Damage, TimerEvent.Time(0, 0, 1));

            _loop = new Addon.Loop(this);
            _loop.SetLoop(0, -2f, 2f, .5f, Mugen.Animation.Loops.PINGPONG);
            _loop.Start();
            AddAddon(_loop);

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
        public void AddRandomEnemy(int nbEnemy = 3)
        {
            for (int i = 0; i < nbEnemy; i++)
            {
                int x, y;
                Point size;

                do
                {
                    x = Misc.Rng.Next(0, 5);
                    y = Misc.Rng.Next(0, GridSize.Y);

                    size = Enemy.Sizes[Misc.Rng.Next(0, Enemy.Sizes.Length)];                

                } while (!AddInGrid(new Enemy(this, new Point(x, y), size, size.X * size.Y, size.X * size.Y * 16, TimerEvent.Time(0, 0, .05f * i * 4))));
            }
        }
        public Enemy FindClosestEnemy()
        {
            var enemies = GroupOf<Enemy>();

            if (enemies.Count == 0) 
                return null;

            enemies.Sort((e1, e2) => e1.MapPosition.X.CompareTo(e2.MapPosition.X));

            return enemies.Last();
        }
        public override Node Update(GameTime gameTime)
        {
            _timer.Update();
            _mouse = Game1.Mouse;

            UpdateRect();

            _mousePos.X = Game1._mousePos.X - _x;
            _mousePos.Y = Game1._mousePos.Y - _y;

            _mapPositionOver.X = (int)Math.Floor(_mousePos.X / CellSize.X);
            _mapPositionOver.Y = (int)Math.Floor(_mousePos.Y / CellSize.Y);

            _prevRectOver = _rectOver;

            _rectOver.X = _mapPositionOver.X * CellSize.X + _x;
            _rectOver.Y = _mapPositionOver.Y * CellSize.Y + _y;

            _mouseCellCenterOver.X = _rectOver.X + CellSize.X / 2;
            _mouseCellCenterOver.Y = _rectOver.Y + CellSize.Y / 2;

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

                    if (ButtonControl.OnePress("Attack", _mouse.LeftButton == ButtonState.Pressed && IsInGrid(_mapPositionOver)) && _arena.State == Arena.States.Action)
                    {
                        if (Attack(_arena, _mapPositionOver))
                        {
                            //_timer.StartTimer((int)Timers.Damage);
                            //ChangeState((int)States.DoDamage);
                        }
                    }


                    if (_rectOver != _prevRectOver && IsInGrid(_mousePos) && !IsNull(_mapPositionOver))
                    {
                        Game1._soundClock.Play(.1f * Game1._volumeMaster, .5f, 0f);
                        //new Trail(_rectOver.Center, Vector2.One, .025f, Color.WhiteSmoke * .75f).AppendTo(_parent);
                        //new FxExplose(_rectOver.Center, Color.Gray).AppendTo(_parent);
                        //Console.WriteLine("RectOver !=");
                    }

                    //var enemy = FindClosestEnemy();
                    //if (enemy != null && _arena.State == Arena.States.Action)
                    //{
                    //    Attack(_arena, enemy.MapPosition);
                    //}


                    break;

                case States.DoAction:

                    ChangeState((int)States.Play);

                    break;

                case States.DoDamage:

                    if (_timer.OnTimer((int)Timers.Damage))
                    {
                        //Console.WriteLine("Dherhdesrhjerj");
                        _timer.StopTimer((int)Timers.Damage);
                        DoAction();
                        ChangeState((int)States.DoAction);
                    }

                    break;

                default:
                    break;
            }

            base.RunState(gameTime);
        }
        public void DoAction()
        {
            var enemies = GroupOf<Enemy>();

            enemies.Sort((e1, e2) => e2.MapPosition.X.CompareTo(e1.MapPosition.X));

            foreach (var enemy in enemies)
            { 
                enemy.TicTurn();
            }

            
        }
        public bool Attack(Arena arena, Point mapPosition)
        {   
            var node = _grid.Get(mapPosition.X, mapPosition.Y);

            if (node != null)
            {
                if (node._type == UID.Get<Enemy>())
                {
                    var enemy = (Enemy)node;
                    var overKill = enemy.SetDamage(arena.TotalAttack);
                    enemy.Shake.SetIntensity(8, .1f);

                    Game1._soundSword.Play(.8f * Game1._volumeMaster, .1f, 0f);

                    string str = "-" + _arena.TotalAttack;

                    if (overKill < 0)
                        str = "OVERKILL " + overKill;

                    new PopInfo(str, Color.White, _arena.CurrentColor, 0, 16, 32).SetPosition(enemy.AbsRectF.TopCenter).AppendTo(_parent);

                    new FxExplose(enemy.AbsRectF.Center, _arena.CurrentColor, 17, 20).AppendTo(_parent);

                    arena.ChangeState((int)Arena.States.FinishTurn);

                    _timer.StartTimer((int)Timers.Damage);
                    ChangeState((int)States.DoDamage);

                    return true;
                }
            }

            return false;

        }
        public bool AddInGrid(Enemy enemy)
        {
            //if (!IsNull(enemy.MapPosition))
            //    return false;
            if (!CanAddInGrid(enemy))
                return false;

            //_grid.Put(enemy.MapPosition.X, enemy.MapPosition.Y, enemy);
            SetInGrid(enemy);

            enemy.SetPosition(MapPositionToVector2(enemy.MapPosition));
            enemy.AppendTo(this);

            return true;
        }
        public bool CanAddInGrid(Enemy enemy)
        {
            for (int i = 0; i < enemy.Size.X; i++)
            {
                for (int j = 0; j < enemy.Size.Y; j++)
                {
                    if (!IsNull(enemy.MapPosition + new Point(i, j)))
                        return false;
                }
            }

            return true;
        }
        public void SetInGrid(Enemy enemy)
        {
            if (!CanSetInGrid(enemy)) 
                return;

            for (int i = 0; i < enemy.Size.X; i++)
            {
                for (int j = 0; j < enemy.Size.Y; j++)
                {
                    _grid.Put(enemy.MapPosition.X + i, enemy.MapPosition.Y + j, enemy);
                }
            }

        }
        public void DeleteInGrid(Enemy enemy)
        {
            for (int i = 0; i < enemy.Size.X; i++)
            {
                for (int j = 0; j < enemy.Size.Y; j++)
                {
                    _grid.Put(enemy.MapPosition.X + i, enemy.MapPosition.Y + j, null);
                }
            }
        }
        public void DeleteInGrid(Point mapPosition)
        {
            _grid.Put(mapPosition.X, mapPosition.Y, null);
        }
        public bool CanSetInGrid(Enemy enemy)
        {
            for (int i = 0; i < enemy.Size.X; i++)
            {
                for (int j = 0; j < enemy.Size.Y; j++)
                {
                    if (!IsInGrid(enemy.MapPosition + new Point(i, j)))
                        return false;
                }
            }

            return true;
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
            if (!IsInGrid(mapPosition))
                return false;

            return _grid.Get(mapPosition.X, mapPosition.Y) == null;
        }
        public T GetCell<T>(Point mapPosition) where T : Node
        {
            return (T)_grid.Get(mapPosition.X, mapPosition.Y);
        }
        public Vector2 MapPositionToVector2(Enemy enemy)
        {

            return enemy.MapPosition.ToVector2() * CellSize;
        }
        public Vector2 MapPositionToVector2(Point mapPosition)
        {
            return mapPosition.ToVector2() * CellSize;
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
                batch.FillRectangle(AbsRectF, Color.Black * .5f);
                //batch.Rectangle(AbsRectF.Extend(10), Color.DarkSlateBlue, 3f);

                if (_rectOver != _prevRectOver && IsInGrid(_mousePos))
                {
                    //new Trail(_rectOver.Center, Vector2.One *.5f, .025f, Color.WhiteSmoke * .75f).AppendTo(_parent);
                    new Trail((Rectangle)_rectOver, .025f, Color.WhiteSmoke * .75f).AppendTo(_parent);
                    //new FxExplose(_rectOver.Center, Color.Gray).AppendTo(_parent);
                    //Console.WriteLine("RectOver !=");
                }

                //batch.Grid(AbsXY, _rect.Width, _rect.Height, CellSize.X, CellSize.Y, Color.Black * .5f, 3f);
                batch.Grid(AbsXY, _rect.Width, _rect.Height, CellSize.X, CellSize.Y, Color.Gray * .1f, 3f);

            }

            if (indexLayer == (int)Game1.Layers.FrontFX)
            {
                var enemy = FindClosestEnemy();
                if (enemy != null && !IsInGrid(_mapPositionOver) && _arena.GetState() == (int)Arena.States.Action)
                {
                    batch.BevelledRectangle(enemy.AbsRectF.Extend(_loop._current + 8), Vector2.One * 4, Color.OrangeRed * .5f, 3f);
                    batch.BevelledRectangle(enemy.AbsRectF.Extend(_loop._current + 4), Vector2.One * 4, Color.Red * 1f, 3f);
                }

                if (_arena.GetState() == (int)Arena.States.Action && IsInGrid(_mapPositionOver))
                {
                    var target = _grid.Get(_mapPositionOver.X, _mapPositionOver.Y);
                    if (target != null) 
                    {
                        batch.BevelledRectangle(target.AbsRectF.Extend(_loop._current + 8), Vector2.One * 4, Color.OrangeRed * .5f, 3f);
                        batch.BevelledRectangle(target.AbsRectF.Extend(_loop._current + 4), Vector2.One * 4, Color.Red * 1f, 3f);
                    }
                }
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
                            //batch.CenterStringXY(Game1._fontMain, $"{ node._type }", AbsXY + MapPositionToVector2(i, j) + CellSize / 2, Color.White * .75f);
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
