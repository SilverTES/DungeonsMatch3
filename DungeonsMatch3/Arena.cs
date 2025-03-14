using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonsMatch3
{
    class Arena : Node
    {
        public enum States
        {
            Play,
            SelectGems,
            ExploseSelectedGems,
            PushGemsToDown,
            AddNewGemsToDown,
            PushGemsToUp,
            AddNewGemsToUp,
        }
        public struct Format
        {
            public Point GridSize;
            public Point CellSize;

            public Format(int gridW, int gridH, int cellW, int cellH)
            {
                GridSize = new Point(gridW, gridH);
                CellSize = new Point(cellW, cellH);
            }
        }

        public Point[] ClosePoints = [
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(1, 0),
            new Point(-1, -1),
            new Point(1, -1),
            new Point(1, 1),
            new Point(-1, 1),
            ];

        public Point GridSize;
        public Point CellSize;

        public RectangleF Rect => _rect;

        Vector2 _mousePos = new Vector2();

        List2D<Gem> _grid;

        RectangleF _rectOver;
        Point _mapPostionOver;
        Vector2 _mapMouseOver;
        Gem _currentGemOver;
        Color _currentColor = Color.Black;

        List<Gem> _gemSelecteds = [];

        MouseState _mouse;

        public Arena()
        {
            _grid = new List2D<Gem>(GridSize.X, GridSize.Y);

            SetState((int)States.Play);
        }
        public void Setup(Format format)
        {
            GridSize = format.GridSize;
            CellSize = format.CellSize;

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
                    var gem = new Gem();
                    _grid.Put(i, j, gem);
                }
            }
        }
        protected override void RunState(GameTime gameTime)
        {
            switch ((States)_state)
            {
                case States.Play:

                    Play();

                    break;

                case States.SelectGems:

                    SelectGems();

                    break;

                case States.ExploseSelectedGems:

                    ExploseSelectedGems();
                    DeSelectAllGems();

                    
                    int pushDirection = Misc.Rng.Next(0, 10);
                    
                    if (pushDirection > 5)
                        ChangeState((int)States.PushGemsToUp);
                    else 
                        ChangeState((int)States.PushGemsToDown);

                    break;

                case States.PushGemsToDown:

                    PushGemsToDown();
                    ChangeState((int)States.AddNewGemsToDown);

                    break;

                case States.AddNewGemsToDown:

                    AddNewGemsToDown();
                    ChangeState((int)States.Play);

                    break;

                case States.PushGemsToUp:

                    PushGemsToUp();
                    ChangeState((int)States.AddNewGemsToUp);

                    break;

                case States.AddNewGemsToUp:

                    AddNewGemsToUp();
                    ChangeState((int)States.Play);

                    break;

                default:
                    break;
            }
        } 
        public override Node Update(GameTime gameTime)
        {
            _mouse = Game1.Mouse;

            if (Collision2D.PointInCircle(_mousePos + AbsXY, _mapMouseOver, 40))
                Mouse.SetCursor(Game1.CursorB);
            else
                Mouse.SetCursor(Game1.CursorA);

            UpdateRect();

            _rect.Width = GridSize.X * CellSize.X;
            _rect.Height = GridSize.Y * CellSize.Y;

            _mousePos.X = Game1._mousePos.X - _x;
            _mousePos.Y = Game1._mousePos.Y - _y;

            _mapPostionOver.X = (int)(_mousePos.X / CellSize.X);
            _mapPostionOver.Y = (int)(_mousePos.Y / CellSize.Y);

            _rectOver.X = _mapPostionOver.X * CellSize.X + _x; 
            _rectOver.Y = _mapPostionOver.Y * CellSize.Y + _y;

            _mapMouseOver.X = _rectOver.X + CellSize.X / 2;
            _mapMouseOver.Y = _rectOver.Y + CellSize.Y / 2;

            RunState(gameTime);

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        private void Play()
        {
            if (!IsInArena(_mousePos))
                ResetGridGemsAsSameColor();

            if (Collision2D.PointInCircle(_mousePos + AbsXY, _mapMouseOver, 30))
            {
                var gemOver = _grid.Get(_mapPostionOver.X, _mapPostionOver.Y);
                if (gemOver != null)
                {
                    _currentGemOver = gemOver;
                    
                    if (_mouse.LeftButton == ButtonState.Pressed && IsInArena(_mapPostionOver))
                    {
                        if (!_gemSelecteds.Contains(gemOver))
                        {
                            _currentColor = gemOver.Color;
                            SelectGem(gemOver);
                            ChangeState((int)States.SelectGems);
                        }
                    }

                    _currentGemOver.Shake(1);
                    FindSameGems(_currentGemOver);
                }
            }
        }
        public void ResetGridGemsAsSameColor()
        {
            for (int i = 0; i < _grid._width; i++)
            {
                for (int j = 0; j < _grid._height; j++)
                {
                    var gem = _grid.Get(i, j);

                    if (gem != null)
                    {
                        gem.IsSameColor = false;
                        gem.NbSameColor = 0;
                    }
                }
            }
        }
        public List<Gem> FindSameGems(Gem gem)
        {
            ResetGridGemsAsSameColor();

            List<Gem> result = [];
            Queue<Gem> queue = [];

            if (gem == null) return result;

            queue.Enqueue(gem);
            result.Add(gem);

            while(queue.Count > 0)
            {
                var nextGem = queue.Dequeue();

                Point scan = new();

                for (int i = 0; i < ClosePoints.Length; i++)
                {
                    scan = nextGem.MapPosition + ClosePoints[i];

                    if (!IsInArena(scan)) continue;

                    var closeGem = _grid.Get(scan.X, scan.Y);
                    if (closeGem != null)
                    {
                        if (closeGem.Color == gem.Color)
                        {
                            //closeGem.Shake(2);

                            if (!closeGem.IsSameColor)
                            {
                                gem.IsSameColor = true;

                                closeGem.IsSameColor = true;
                                queue.Enqueue(closeGem);
                                result.Add(closeGem);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < result.Count; i++)
            {
                result[i].NbSameColor = result.Count;
            }

            return result;
        }
        public void SelectGems()
        {
            if (_mouse.LeftButton == ButtonState.Released)
            {
                if (_gemSelecteds.Count >= 3)
                {
                    ChangeState((int)States.ExploseSelectedGems);
                }
                else
                {
                    DeSelectAllGems();
                    ChangeState((int)States.Play);
                }

            }
            else
            {
                var gem = _grid.Get(_mapPostionOver.X, _mapPostionOver.Y);

                if (gem != null)
                {

                    if (!_gemSelecteds.Contains(gem)) // Select a non selected gem
                    {
                        if (gem.Color == _currentColor && IsClose(_gemSelecteds.Last().MapPosition, gem.MapPosition))
                        {
                            SelectGem(gem);
                            new FxExplose(gem.AbsXY, gem.Color, 10, 10).AppendTo(this);
                        }
                    }
                    else // Select an already selected gem
                    {
                        if (gem.Color == _currentColor && (_gemSelecteds.Last().MapPosition != gem.MapPosition))
                        {
                            DeSelectGem(_gemSelecteds.Last());
                            //Console.Write("< Deselect >");
                        }
                    }
                }
            }
        }
        public void PushGemsToDown()
        {
            for (int row = _grid._height; row >= 0; row--)
            {
                for (int col = 0; col < _grid._width; col++)
                {
                    var gem = _grid.Get(col, row);

                    if (gem != null)
                    {
                        gem.IsFall = false;
                        // scan vertical
                        for (int scanY = row + 1; scanY < _grid._height; scanY++)
                        {
                            var gemAtBottom = _grid.Get(col, scanY);

                            if (gemAtBottom != null)
                                break;

                            if (gemAtBottom == null)
                            {
                                gem.IsFall = true;
                                gem.DownPosition = new Point(col, scanY);
                            }
                        }

                        if (gem.IsFall)
                        {
                            DeleteGem(gem);
                            AddGem(gem, gem.DownPosition);
                            gem.MoveTo(gem.DownPosition);
                        }
                    }
                }
            }
        }
        public void PushGemsToUp()
        {
            for (int row = 0; row < _grid._height; row++)
            //for (int row = _grid._height; row >= 0; row--)
            {
                for (int col = 0; col < _grid._width; col++)
                {
                    var gem = _grid.Get(col, row);

                    if (gem != null)
                    {
                        gem.IsFall = false;
                        // scan vertical
                        //for (int scanY = row + 1; scanY < _grid._height; scanY++)
                        for (int scanY = row - 1; scanY >= 0; scanY--)
                        {
                            var gemAtBottom = _grid.Get(col, scanY);

                            if (gemAtBottom != null)
                                break;

                            if (gemAtBottom == null)
                            {
                                gem.IsFall = true;
                                gem.DownPosition = new Point(col, scanY);
                            }
                        }

                        if (gem.IsFall)
                        {
                            DeleteGem(gem);
                            AddGem(gem, gem.DownPosition);
                            gem.MoveTo(gem.DownPosition);
                        }
                    }
                }
            }
        }
        public void AddNewGemsToDown()
        {
            for (int row = _grid._height - 1; row >= 0; row--)
            {
                for (int col = 0; col < _grid._width; col++)
                {
                    var gem = _grid.Get(col, row);
                    if (gem == null)
                    {
                        var newGem = new Gem(this, RandomColor(), new Point(col, -1));
                        newGem.SetPosition(MapPositionToVector2(newGem.MapPosition)).AppendTo(this);

                        newGem.DownPosition = new Point(col, row);

                        AddGem(newGem, newGem.DownPosition);
                        newGem.MoveTo(newGem.DownPosition);
                    }
                }
            }
        }
        public void AddNewGemsToUp()
        {
            for (int row = 0; row <_grid._height; row++)
            {
                for (int col = 0; col < _grid._width; col++)
                {
                    var gem = _grid.Get(col, row);
                    if (gem == null)
                    {
                        var newGem = new Gem(this, RandomColor(), new Point(col, _grid._height + 1));
                        newGem.SetPosition(MapPositionToVector2(newGem.MapPosition)).AppendTo(this);

                        newGem.DownPosition = new Point(col, row);

                        AddGem(newGem, newGem.DownPosition);
                        newGem.MoveTo(newGem.DownPosition);
                    }
                }
            }
        }
        public void SelectGem(Gem gem)
        {
            gem.IsSelected = true;
            _gemSelecteds.Add(gem);
        }
        public void DeSelectGem(Gem gem)
        {
            if (_gemSelecteds.Contains(gem))
            {
                gem.IsSelected = false;
                _gemSelecteds.Remove(gem);
            }
        }
        public void DeSelectAllGems()
        {
            var gems = GroupOf(UID.Get<Gem>());
            
            for (int i = 0; i < gems.Count; i++)
            {
                var gem = (Gem)gems[i];
                gem.IsSelected = false;
            }

            _gemSelecteds.Clear();
        }
        public void ExploseSelectedGems()
        {
            Console.WriteLine($"Explose Selected = {_gemSelecteds.Count}");

            for (int i = 0; i < _gemSelecteds.Count; i++)
            {
                var gem = _gemSelecteds[i];
                _grid.Put(gem.MapPosition.X, gem.MapPosition.Y, null);

                gem.ExploseMe();
            }
            _gemSelecteds.Clear();

        }
        public Color RandomColor()
        {
            return Gem.Colors[Misc.Rng.Next(0, Gem.Colors.Length)];
        }
        public void InitGrid()
        {
            for (int i = 0; i < _grid._width; i++)
            {
                for (int j = 0; j < _grid._height; j++)
                {
                    var color = RandomColor();

                    var gem = (Gem)new Gem(this, color, new Point(i, j)).SetPosition(MapPositionToVector2(new Point(i, j))).AppendTo(this);

                    AddGem(gem);
                }
            }
        }
        public void ClearGrid()
        {
            for (int i = 0; i < _grid._width; i++)
            {
                for (int j = 0; j < _grid._height; j++)
                {
                    var gem = _grid.Get(i, j);

                    if (gem != null)
                    {
                        _grid.Put(i, j, null);
                    }
                    
                }
            }

            KillAll(UID.Get<Gem>());
        }
        public void AddGem(Gem gem)
        {
            _grid.Put(gem.MapPosition.X, gem.MapPosition.Y, gem);
        }
        public void AddGem(Gem gem, Point mapPosition)
        {
            _grid.Put(mapPosition.X, mapPosition.Y, gem);
        }
        public void DeleteGem(Gem gem)
        {
            _grid.Put(gem.MapPosition.X, gem.MapPosition.Y, null);
        }
        public void DeleteGem(Point mapPosition)
        {
            _grid.Put(mapPosition.X, mapPosition.Y, null);
        }
        public bool IsClose(Point A, Point B)
        {
            if (!IsInArena(A) || !IsInArena(B))
                return false;

            if (Math.Abs(A.X - B.X) < 2 &&
                Math.Abs(A.Y - B.Y) < 2) return true;

            return false;
        }
        public bool IsInArena(Vector2 position)
        {
            return Misc.PointInRect(position + XY, _rect);
        }
        public bool IsInArena(Point mapPosition)
        {
            if (mapPosition.X < 0 || mapPosition.X >= _grid._width || mapPosition.Y < 0 || mapPosition.Y >= _grid._height)
                return false;

            return true;
        }
        public Vector2 MapPositionToVector2(Point mapPosition)
        {
            return (mapPosition * CellSize).ToVector2() + CellSize.ToVector2() / 2;
        }
        public Vector2 MapPositionToVector2(int i, int j)
        {
            return MapPositionToVector2(new Point(i, j));
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .80f);
                //batch.Grid(AbsXY, AbsRectF.Width, AbsRectF.Height, CellSize.X, CellSize.Y, Color.Gray * .5f, 1);

                //if (IsInArena(_mousePos))
                //    batch.Rectangle(_rectOver, Color.Cyan * .5f, 4f);

                batch.Rectangle(AbsRectF.Extend(4), Color.Black, 3);

                DrawGemsLink(batch);
            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                //batch.LeftTopString(Game1._fontMain, $"NB close Gems = {FindSameGems(_currentGemOver).Count}", Vector2.UnitX * 20 + Vector2.UnitY * 120, Color.Yellow);
                //batch.LeftTopString(Game1._fontMain, $"{_mousePos}", Vector2.One * 20, Color.Yellow);
                //batch.LeftTopString(Game1._fontMain, $"{(States)_state}", Vector2.One * 20 + Vector2.UnitY * 80, Color.Cyan);
                //batch.LeftTopString(Game1._fontMain, $"{_currentColor} {_gemSelecteds.Count}", Vector2.One * 20 + Vector2.UnitY * 120, _currentColor);

                //for (int i = 0; i < _grid._width; i++)
                //{
                //    for (int j = 0; j < _grid._height; j++)
                //    {
                //        var gem = _grid.Get(i, j);
                //        if (gem != null) 
                //        { 
                //            batch.CenterStringXY(Game1._fontMain, $"{System.Array.IndexOf(Gem.Colors, gem.Color)}", AbsXY + MapPositionToVector2(i, j), Color.White);
                //        }
                //    }
                //}

                //batch.Point(_mapMouseOver, 4, Color.OrangeRed);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
        public void DrawGemsLink(SpriteBatch batch)
        {
            for (int i = 0; i < _gemSelecteds.Count - 1; i++)
            {
                batch.Line(AbsXY + MapPositionToVector2(_gemSelecteds[i].MapPosition), AbsXY + MapPositionToVector2(_gemSelecteds[i + 1].MapPosition), _currentColor, 15f);
                batch.Line(AbsXY + MapPositionToVector2(_gemSelecteds[i].MapPosition), AbsXY + MapPositionToVector2(_gemSelecteds[i + 1].MapPosition), Color.White, 5f);
            }

            if (_state == (int)States.SelectGems && _gemSelecteds.Count > 0)
                batch.Line(AbsXY + MapPositionToVector2(_gemSelecteds.Last().MapPosition), AbsXY + _mousePos, _currentColor, 15f);
        }
    }
}
