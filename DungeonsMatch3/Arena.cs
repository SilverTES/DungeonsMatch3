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
            None,
            Selected,
            ExploseSelected,
        }
        public struct Dimension
        {
            public Point GridSize;
            public Point CellSize;

            public Dimension(int gridW, int gridH, int cellW, int cellH)
            {
                GridSize = new Point(gridW, gridH);
                CellSize = new Point(cellW, cellH);
            }
        }

        public Point GridSize;
        public Point CellSize;

        public RectangleF Rect => _rect;

        Vector2 _mousePos = new Vector2();

        RectangleF _rectOver;
        Point _mapPostionOver;
        Vector2 _mapMouseOver;

        List2D<Gem> _grid;

        List<Gem> _gemSelecteds = [];
        Color _currentColor = Color.Black;

        MouseState _mouse;

        public Arena()
        {
            _grid = new List2D<Gem>(GridSize.X, GridSize.Y);

            SetState((int)States.None);
        }
        public void Setup(Dimension dimension)
        {
            GridSize = dimension.GridSize;
            CellSize = dimension.CellSize;

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
                case States.None:

                    if (_mouse.LeftButton == ButtonState.Pressed && IsInArena(_mapPostionOver) && Collision2D.PointInCircle(_mousePos + AbsXY, _mapMouseOver, 32))
                    {
                        var gem = _grid.Get(_mapPostionOver.X, _mapPostionOver.Y);

                        if (gem != null)
                        {
                            if (!_gemSelecteds.Contains(gem))
                            {
                                _currentColor = gem.Color;
                                SelectGem(gem);
                            }
                        }
                        ChangeState((int)States.Selected);
                    }

                    break;

                case States.Selected:

                    if (_mouse.LeftButton == ButtonState.Released)
                    {
                        if (_gemSelecteds.Count >= 3)
                        {
                            ChangeState((int)States.ExploseSelected);
                        }
                        else
                        {
                            DeSelectAllGem();
                            ChangeState((int)States.None);
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

                    break;

                case States.ExploseSelected:

                    ExploseSelectedGem();

                    DeSelectAllGem();

                    ChangeState((int)States.None);

                    break;
                default:
                    break;
            }
        } 
        public override Node Update(GameTime gameTime)
        {
            _mouse = Game1.Mouse;

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
        public void SelectGem(Gem gem)
        {
            gem._isSelected = true;
            _gemSelecteds.Add(gem);
        }
        public void DeSelectGem(Gem gem)
        {
            if (_gemSelecteds.Contains(gem))
            {
                gem._isSelected = false;
                _gemSelecteds.Remove(gem);
            }
        }
        public void DeSelectAllGem()
        {
            var gems = GroupOf(UID.Get<Gem>());
            
            for (int i = 0; i < gems.Count; i++)
            {
                var gem = (Gem)gems[i];
                gem._isSelected = false;
            }

            _gemSelecteds.Clear();
        }
        public void ExploseSelectedGem()
        {
            Console.WriteLine($"Explose Selected = {_gemSelecteds.Count}");

            for (int i = 0; i < _gemSelecteds.Count; i++)
            {
                var gem = _gemSelecteds[i];
                _grid.Put(gem.MapPosition.X, gem.MapPosition.Y, null);

                gem.KillMe();
            }
            _gemSelecteds.Clear();

        }
        //public Color CurrentColor()
        //{
        //    return _currentColor != Const.NoIndex ? Gem.Colors[_currentColor] : Color.Black;
        //}
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

                    AddGem(new Point(i, j), RandomColor());
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
                        gem.KillMe();
                        _grid.Put(i, j, null);
                    }
                    
                }
            }
        }
        public void AddGem(Point mapPosition, Color color)
        {
            var gem = (Gem)new Gem(this, color, mapPosition).SetPosition(MapPositionToVector2(mapPosition)).AppendTo(this);
            _grid.Put(mapPosition.X, mapPosition.Y, gem);
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
                batch.FillRectangle(AbsRectF, Color.DarkSlateBlue);
                //batch.Grid(AbsXY, AbsRectF.Width, AbsRectF.Height, CellSize.X, CellSize.Y, Color.Gray * .5f, 1);

                if (IsInArena(_mousePos))
                    batch.Rectangle(_rectOver, Color.Cyan * .5f, 4f);
                
                batch.Rectangle(AbsRectF.Extend(4), Color.Black, 3);
            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                batch.LeftTopString(Game1._fontMain, $"{_mousePos}", Vector2.One * 20, Color.Yellow);
                batch.LeftTopString(Game1._fontMain, $"{(States)_state}", Vector2.One * 20 + Vector2.UnitY * 80, Color.Cyan);
                batch.LeftTopString(Game1._fontMain, $"{_currentColor} {_gemSelecteds.Count}", Vector2.One * 20 + Vector2.UnitY * 120, _currentColor);

                //for (int i = 0; i < _grid._width; i++)
                //{
                //    for (int j = 0; j < _grid._height; j++)
                //    {
                //        int type = _grid.Get(i, j)._type;
                //        batch.CenterStringXY(Game1._fontMain, type != Const.NoIndex ? type.ToString() : ".", AbsXY + MapPositionToVector2(i,j), Color.White);
                //    }
                //}

                for (int i = 0; i < _gemSelecteds.Count-1; i++)
                {
                    batch.Line(AbsXY + MapPositionToVector2(_gemSelecteds[i].MapPosition), AbsXY + MapPositionToVector2(_gemSelecteds[i+1].MapPosition), _currentColor, 15f);

                    //batch.Circle(AbsXY + MapPositionToVector2(_selecteds[i].MapPosition), 40, 24, Color.White, 4);
                }

                if (_state == (int)States.Selected && _gemSelecteds.Count > 0)
                    batch.Line(AbsXY + MapPositionToVector2(_gemSelecteds.Last().MapPosition), AbsXY + _mousePos, _currentColor, 15f);

                batch.Point(_mapMouseOver, 4, Color.OrangeRed);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
