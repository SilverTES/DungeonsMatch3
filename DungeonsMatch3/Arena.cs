using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;

namespace DungeonsMatch3
{
    class Arena : Node
    {
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

        Color[] _colors = [
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.Violet,
        ];

        public Point GridSize;
        public Point CellSize;

        public RectangleF Rect => _rect;

        Vector2 _mousePos = new Vector2();

        RectangleF _rectOver;
        Point _mapPostionOver; 

        List2D<Cell> _grid;

        public Arena()
        {
            _grid = new List2D<Cell>(GridSize.X, GridSize.Y);

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
                    var cell = new Cell();
                    _grid.Put(i, j, cell);
                }
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _rect.Width = GridSize.X * CellSize.X;
            _rect.Height = GridSize.Y * CellSize.Y;

            _mousePos.X = Game1._mousePos.X - _x;
            _mousePos.Y = Game1._mousePos.Y - _y;

            _mapPostionOver.X = (int)(_mousePos.X / CellSize.X) * CellSize.X;
            _mapPostionOver.Y = (int)(_mousePos.Y / CellSize.Y) * CellSize.Y;

            _rectOver.X = _mapPostionOver.X + _x; 
            _rectOver.Y = _mapPostionOver.Y + _y;

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public Color RandomColor()
        {
            return _colors[Misc.Rng.Next(0, _colors.Length)];
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
                    var cell = _grid.Get(i, j);

                    if (cell != null)
                    {
                        cell._type = Const.NoIndex;
                        if (cell._owner != null)
                        {
                            cell._owner.KillMe();
                            cell._owner = null;
                        }
                    }
                    
                }
            }
        }
        public void AddGem(Point mapPosition, Color color)
        {
            var gem = new Gem(color).SetPosition(MapPositionToVector2(mapPosition)).AppendTo(this);

            var cell = new Cell();
            cell._type = UID.Get<Gem>();
            cell._owner = gem;

            _grid.Put(mapPosition.X, mapPosition.Y, cell);
        }
        public bool IsInArena(Vector2 position)
        {
            return Misc.PointInRect(position + XY, _rect);
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

                for (int i = 0; i < _grid._width; i++)
                {
                    for (int j = 0; j < _grid._height; j++)
                    {
                        int type = _grid.Get(i, j)._type;
                        batch.CenterStringXY(Game1._fontMain, type != Const.NoIndex ? type.ToString() : ".", AbsXY + MapPositionToVector2(i,j), Color.White);
                    }
                }
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
