using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Physics;


namespace DungeonsMatch3
{
    class BattleField : Node
    {
        static public Point GridSize = new Point(8, 8);
        static public Point CellSize = new Point(120, 120);

        List2D<Node> _grid;

        Vector2 _mousePos = new Vector2();
        RectangleF _rectOver;
        Point _mapPostionOver;
        Vector2 _mapMouseOver;

        public BattleField() 
        {
            _grid = new List2D<Node>(GridSize.X, GridSize.Y);

            SetSize(CellSize.X * GridSize.X, CellSize.Y * GridSize.Y);

            _rectOver = new RectangleF(0, 0, CellSize.X, CellSize.Y);

            CreateGrid();
        }
        public void CreateGrid()
        {
            for (int i = 0; i < _grid._width; i++)
            {
                for (int j = 0; j < _grid._height; j++)
                {
                    //var gem = new Gem();
                    _grid.Put(i, j, null);
                }
            }
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _mousePos.X = Game1._mousePos.X - _x;
            _mousePos.Y = Game1._mousePos.Y - _y;

            _mapPostionOver.X = (int)(_mousePos.X / CellSize.X);
            _mapPostionOver.Y = (int)(_mousePos.Y / CellSize.Y);

            _rectOver.X = _mapPostionOver.X * CellSize.X + _x;
            _rectOver.Y = _mapPostionOver.Y * CellSize.Y + _y;

            _mapMouseOver.X = _rectOver.X + CellSize.X / 2;
            _mapMouseOver.Y = _rectOver.Y + CellSize.Y / 2;

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public void DoAction()
        {
            var nodes = GroupOf(UID.Get<Enemy>());

            foreach (var node in nodes)
            { 
                var enemy = node as Enemy;
                if (enemy != null) 
                {
                    enemy.TicTurn();
                }
            }
        }
        public void AddInGrid(Enemy enemy)
        {
            _grid.Put(enemy.MapPosition.X, enemy.MapPosition.Y, enemy);
            //enemy.MapPosition = mapPosition;
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
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .25f);
                batch.Rectangle(AbsRectF.Extend(10), Color.DarkSlateBlue, 3f);

                batch.Grid(AbsXY, _rect.Width, _rect.Height, CellSize.X, CellSize.Y, Color.Black * .5f, 1f);

                if (IsInGrid(_mousePos))
                    batch.Rectangle(_rectOver, Color.Cyan * .75f, 5f);
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

            }

            DrawChilds(batch, gameTime);

            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
