using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.Event;
using Mugen.GFX;
using Mugen.Input;
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
            Action,
            FinishTurn,
            SelectGems,
            ExploseSelectedGems,
            PushGemsToDown,
            AddNewGemsToDown,
            //PushGemsToUp,
            //AddNewGemsToUp,
        }

        public States State => (States)_state;

        public enum Timers
        {
            None,
            Help,
        }
        TimerEvent _timers;

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
        public Vector2 CellSize;

        public int NbTurns = 0;
        public int MaxTurns = 8;
        public bool OnFinishTurn = false;
        public RectangleF Rect => _rect;

        List2D<Gem> _grid;

        Vector2 _mousePos = new Vector2();
        RectangleF _rectOver;
        Point _mapPositionOver;
        Vector2 _centerCellOver;

        Gem _currentGemOver;
        Color _currentColor = Color.Black;
        public Color CurrentColor => _currentColor;

        List<Gem> _gemSelecteds = [];

        public static float RadiusGemOver = Gem.Radius - 10;

        MouseState _mouse;

        public int Multiplier = 0;
        public int Attack = 2;

        public int TotalAttack = 0;

        BattleField _battleField;

        Addon.Loop _loop;

        public Arena(BattleField battleField = null)
        {
            _battleField = battleField;
            _grid = new List2D<Gem>(GridSize.X, GridSize.Y);

            SetState((int)States.Play);

            _timers = new TimerEvent(Enums.Count<Timers>());

            _timers.SetTimer((int)Timers.Help, TimerEvent.Time(0, 0, 3), true);
            _timers.StartTimer((int)Timers.Help);

            _loop = new Addon.Loop(this);
            _loop.SetLoop(0, -2f, 2f, .5f, Mugen.Animation.Loops.PINGPONG);
            _loop.Start();

        }
        public void SetBattlefield(BattleField battleField)
        { 
            _battleField = battleField; 
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
                    OnFinishTurn = false;
                    TotalAttack = 0;
                    Play();

                    break;

                case States.SelectGems:
                    
                    SelectGems();

                    Multiplier = _gemSelecteds.Count;
                    TotalAttack = Attack * Multiplier;

                    break;

                case States.ExploseSelectedGems:

                    //Console.WriteLine($"Explose Selected = {_gemSelecteds.Count}");
                    Game1._soundPop.Play(.5f * Game1._volumeMaster, 1.0f, 0.0f);


                    ExploseSelectedGems();
                    DeSelectAllGems();

                    
                    //int pushDirection = Misc.Rng.Next(0, 10);
                    
                    //if (pushDirection > 5)
                    //    ChangeState((int)States.PushGemsToUp);
                    //else 
                        ChangeState((int)States.PushGemsToDown);

                    _timers.StartTimer((int)Timers.Help);

                    break;

                case States.PushGemsToDown:

                    PushGemsToDown();
                    ChangeState((int)States.AddNewGemsToDown);

                    break;

                //case States.PushGemsToUp:

                //    PushGemsToUp();
                //    ChangeState((int)States.AddNewGemsToUp);

                //    break;

                case States.AddNewGemsToDown:

                    AddNewGemsToDown();
                    ChangeState((int)States.Action);

                    break;


                //case States.AddNewGemsToUp:

                //    AddNewGemsToUp();
                //    ChangeState((int)States.Action);

                //    break;

                case States.Action:

                    if (IsInGrid(_mousePos))
                    {
                        if (ButtonControl.OnePress("AutoAttack", _mouse.LeftButton == ButtonState.Pressed))
                        {
                            var enemy = _battleField.FindClosestEnemy();

                            if (enemy != null)
                                _battleField.AttackEnemy(this, enemy.MapPosition);
                        }
                    }

                    break;

                case States.FinishTurn:

                    OnFinishTurn = true;

                    NbTurns++;

                    ChangeState((int)States.Play);

                    break;

                default:
                    break;
            }
        }
        //private bool IsPointInCircle(Vector2 point, Vector2 circleCenter, float radius)
        //{
        //    float dx = point.X - circleCenter.X;
        //    float dy = point.Y - circleCenter.Y;
        //    float distanceSquared = dx * dx + dy * dy;
        //    float radiusSquared = radius * radius;
        //    return distanceSquared <= radiusSquared;
        //}
        public override Node Update(GameTime gameTime)
        {
            _timers.Update();
            _loop.Update(gameTime);

            _mouse = Game1.Mouse;

            if (Collision2D.PointInCircle(_mousePos + AbsXY, _centerCellOver, RadiusGemOver) && IsInGrid(_centerCellOver))
                Mouse.SetCursor(Game1.CursorB);
            else
                Mouse.SetCursor(Game1.CursorA);

            UpdateRect();

            _rect.Width = GridSize.X * CellSize.X;
            _rect.Height = GridSize.Y * CellSize.Y;

            _mousePos.X = Game1._mousePos.X - _x;
            _mousePos.Y = Game1._mousePos.Y - _y;

            _mapPositionOver.X = (int)Math.Floor(_mousePos.X / CellSize.X);
            _mapPositionOver.Y = (int)Math.Floor(_mousePos.Y / CellSize.Y);

            _rectOver.X = _mapPositionOver.X * CellSize.X + _x; 
            _rectOver.Y = _mapPositionOver.Y * CellSize.Y + _y;

            _centerCellOver.X = _rectOver.X + CellSize.X / 2;
            _centerCellOver.Y = _rectOver.Y + CellSize.Y / 2;

            RunState(gameTime);

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        private void Play()
        {
            if (!IsInGrid(_mousePos))
                ResetGridGemsAsSameColor();

            if (Collision2D.PointInCircle(_mousePos + AbsXY, _centerCellOver, RadiusGemOver))
            {
                var gemOver = _grid.Get(_mapPositionOver.X, _mapPositionOver.Y);
                if (gemOver != null)
                {
                    _currentGemOver = gemOver;
                    
                    if (_mouse.LeftButton == ButtonState.Pressed && IsInGrid(_mapPositionOver))
                    {
                        if (!_gemSelecteds.Contains(gemOver))
                        {
                            _currentColor = gemOver.Color;
                            SelectGem(gemOver);
                            ChangeState((int)States.SelectGems);

                            Game1._soundClock.Play(.5f * Game1._volumeMaster, 1.0f, 0.0f);
                        }
                    }

                    _currentGemOver.Shake.SetIntensity(1);
                    FindSameGems(_currentGemOver);
                }
            }
            
            if (_timers.OnTimer((int)Timers.Help))
            {

                //Console.WriteLine("Help Help");

                var result = SearchSameGems();

                ResetGridGemsAsSameColor();

                for (int i = 0; i < result.Count; i++)
                {
                    result[i].Shake.SetIntensity(3, .01f);
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
        public List<Gem> SearchSameGems()
        {
            List<Gem> result = [];

            for (int i = 0; i < _grid._width; i++)
            {
                for (int j = 0; j < _grid._height; j++)
                {
                    var gem = _grid.Get(i, j);

                    if (gem != null)
                    {
                        var gems = FindSameGems(gem);
                        if (gems.Count > 2)
                        {
                            result = gems;
                            break;
                        }

                    }
                }
            }

            return result;
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

                    if (!IsInGrid(scan)) continue;

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

                    //NbTurns++;
                }
                else
                {
                    DeSelectAllGems();
                    ChangeState((int)States.Play);
                }

            }
            else
            {
                var gem = _grid.Get(_mapPositionOver.X, _mapPositionOver.Y);

                if (gem != null)
                {

                    if (!_gemSelecteds.Contains(gem)) // Select a non selected gem
                    {
                        if (gem.Color == _currentColor && IsClose(_gemSelecteds.Last().MapPosition, gem.MapPosition))
                        {
                            if (Collision2D.PointInCircle(_mousePos + AbsXY, _centerCellOver, RadiusGemOver))
                            {
                                SelectGem(gem);
                                new FxExplose(gem.AbsXY, gem.Color, 5, 10).AppendTo(this);
                                Game1._soundClock.Play(.5f * Game1._volumeMaster, 1.0f, 0.0f);
                            }
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
                            if (_grid.Get(col, scanY) == null)
                            {
                                gem.IsFall = true;
                                gem.GoalPosition = new Point(col, scanY);
                            }
                        }

                        if (gem.IsFall)
                        {
                            DeleteInGrid(gem);
                            gem.MoveTo(gem.GoalPosition);
                        }
                    }
                }
            }
        }
        //public void PushGemsToUp()
        //{
        //    for (int row = 0; row < _grid._height; row++)
        //    {
        //        for (int col = 0; col < _grid._width; col++)
        //        {
        //            var gem = _grid.Get(col, row);

        //            if (gem != null)
        //            {
        //                gem.IsFall = false;
        //                // scan vertical
        //                for (int scanY = row - 1; scanY >= 0; scanY--)
        //                {
        //                    if (_grid.Get(col, scanY) == null)
        //                    {
        //                        gem.IsFall = true;
        //                        gem.GoalPosition = new Point(col, scanY);
        //                    }
        //                }

        //                if (gem.IsFall)
        //                {
        //                    DeleteInGrid(gem);
        //                    gem.MoveTo(gem.GoalPosition);
        //                }
        //            }
        //        }
        //    }
        //}
        public void AddNewGemsToDown()
        {
            for (int row = _grid._height - 1; row >= 0; row--)
            {
                for (int col = 0; col < _grid._width; col++)
                {
                    if (_grid.Get(col, row) == null)
                        AddInGrid(new Gem(this, RandomColor(), new Point(col, -1))).MoveTo(new Point(col, row));
                }
            }
        }
        //public void AddNewGemsToUp()
        //{
        //    for (int row = 0; row <_grid._height; row++)
        //    {
        //        for (int col = 0; col < _grid._width; col++)
        //        {
        //            if (_grid.Get(col, row) == null)
        //                AddInGrid(new Gem(this, RandomColor(), new Point(col, _grid._height + 1))).MoveTo(new Point(col, row));
        //        }
        //    }
        //}
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
            for (int i = 0; i < _gemSelecteds.Count; i++)
                DeleteInGrid(_gemSelecteds[i]).ExploseMe();

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
                    AddInGrid(new Gem(this, RandomColor(), new Point(i, j)));
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

            KillAll(UID.Get<Gem>());
        }
        public Gem AddInGrid(Gem gem)
        {
            //_grid.Put(gem.MapPosition.X, gem.MapPosition.Y, gem);
            SetInGrid(gem);
            gem.SetPosition(MapPositionToVector2(gem.MapPosition)).AppendTo(this);

            return gem;
        }
        public Gem SetInGrid(Gem gem, Point mapPosition)
        {
            _grid.Put(mapPosition.X, mapPosition.Y, gem);

            return gem;
        }
        public Gem SetInGrid(Gem gem)
        {
            _grid.Put(gem.MapPosition.X, gem.MapPosition.Y, gem);

            return gem;
        }
        public Gem DeleteInGrid(Gem gem)
        {
            _grid.Put(gem.MapPosition.X, gem.MapPosition.Y, null);

            return gem;
        }
        public void DeleteInGrid(Point mapPosition)
        {
            _grid.Put(mapPosition.X, mapPosition.Y, null);
        }
        public bool IsClose(Point A, Point B)
        {
            if (!IsInGrid(A) || !IsInGrid(B))
                return false;

            if (Math.Abs(A.X - B.X) < 2 &&
                Math.Abs(A.Y - B.Y) < 2) return true;

            return false;
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
            return (mapPosition.ToVector2() * CellSize) + CellSize / 2;
        }
        public Vector2 MapPositionToVector2(int i, int j)
        {
            return MapPositionToVector2(new Point(i, j));
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.FillRectangle(AbsRectF, Color.Black * .5f);
                //batch.Grid(AbsXY, AbsRectF.Width, AbsRectF.Height, CellSize.X, CellSize.Y, Color.Gray * .5f, 1);

                //if (IsInGrid(_mousePos))
                //    batch.Rectangle(_rectOver, Color.Cyan * .5f, 4f);

                //batch.Rectangle(AbsRectF.Extend(4), Color.Black, 3);

                DrawGemsLink(batch, gameTime);
            }

            if (indexLayer == (int)Game1.Layers.FrontFX)
            {
                if (_state == (int)States.Action)
                {
                    batch.FillRectangle(AbsRectF, Color.Black * .25f);

                    batch.BevelledRectangle(AbsRectF.Extend(_loop._current), Vector2.One * 4, _currentColor * 1f, 3f);
                    batch.BevelledRectangle(AbsRectF.Extend(_loop._current + 2), Vector2.One * 4, _currentColor * .5f, 1f);

                    if (IsInGrid(_mapPositionOver))
                    {
                        var enemy = _battleField.FindClosestEnemy();
                        if (enemy != null)
                        {
                            var A = _battleField.AbsXY + _battleField.MapPositionToVector2(enemy) + enemy.SizeVector2 / 2;
                            var B = AbsXY + _mousePos;

                            DrawCurve(batch, A, B);
                        }
                    }
                    else
                    {
                        var enemy = _battleField.GetCell<Unit>(_battleField.MapPositionOver);
                        if (enemy != null)
                        {
                            var A = _battleField.AbsXY + _battleField.MapPositionToVector2(enemy) + enemy.SizeVector2 / 2;
                            var B = AbsRectF.Center;

                            DrawCurve(batch, A, B);
                        }
                    }

                }

                

            }

            if (indexLayer == (int)Game1.Layers.HUD)
            {
                if (_state == (int)States.SelectGems || _state == (int)States.Action)
                    DrawAttack(batch, AbsXY + _mousePos - Vector2.UnitY * 40 + Vector2.UnitX * 40);
            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                //batch.LeftTopString(Game1._fontMain, $"NB close Gems = {FindSameGems(_currentGemOver).Count}", Vector2.UnitX * 20 + Vector2.UnitY * 120, Color.Yellow);
                //batch.LeftTopString(Game1._fontMain, $"{_mousePos}", Vector2.One * 20, Color.Yellow);
                batch.CenterStringXY(Game1._fontMain, $"{(States)_state}", AbsRectF.BottomCenter - Vector2.UnitY * 20, Color.Cyan);
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

                batch.CenterStringXY(Game1._fontMain2, $"Turns {NbTurns} / {MaxTurns}", AbsRectF.TopCenter, Color.Yellow);

                //batch.Point(_mousePos + AbsXY, 4, Color.Red);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }
        public void DrawCurve(SpriteBatch batch, Vector2 A, Vector2 B)
        {
            Game1.DrawCurvedLine(batch, GFX._whitePixel, A, B, new Vector2(B.X, A.Y), Color.Transparent, _currentColor * 1f, 9f);
            Game1.DrawCurvedLine(batch, GFX._whitePixel, A, B, new Vector2(B.X, A.Y), Color.White * 0f, Color.White * .5f, 3f);
        }
        public void DrawAttack(SpriteBatch batch, Vector2 position)
        {
            //batch.Point(position, 24, Color.Black * .5f);
            var text = $"{TotalAttack}";
            var textSize = Game1._fontMedium.MeasureString(text);

            batch.FillRectangleCentered(position, textSize * 2.2f, Color.Black * .8f, 0);
            batch.RectangleCentered(position, textSize * 2.0f, _currentColor, 3f);
            batch.CenterBorderedStringXY(Game1._fontMedium, text , position, _currentColor, Color.White);
        }
        public void DrawGemsLink(SpriteBatch batch, GameTime gameTime)
        {
            for (int i = 0; i < _gemSelecteds.Count - 1; i++)
            {
                var p1 = AbsXY + MapPositionToVector2(_gemSelecteds[i].MapPosition);
                var p2 = AbsXY + MapPositionToVector2(_gemSelecteds[i + 1].MapPosition);

                batch.Line(p1, p2, _currentColor, 13f);
                batch.Line(p1, p2, Color.White, 5f);

                //Game1.DrawElectricEffect(
                //    batch,
                //    GFX._whitePixel,
                //    p1,  // Point A
                //    p2,  // Point B
                //    _currentColor,            // Couleur de l'éclair
                //    10,                     // Nombre de segments
                //    20f,                    // Amplitude maximale du décalage
                //    (float)gameTime.TotalGameTime.TotalSeconds, // Pour l'animation
                //    4f
                //);

                //Game1.DrawElectricEffect(
                //    batch,
                //    GFX._whitePixel,
                //    p1,  // Point A
                //    p2,  // Point B
                //    Color.White,            // Couleur de l'éclair
                //    10,                     // Nombre de segments
                //    20f,                    // Amplitude maximale du décalage
                //    (float)gameTime.TotalGameTime.TotalSeconds, // Pour l'animation
                //    2f
                //);
            }

            if (_state == (int)States.SelectGems && _gemSelecteds.Count > 0)
            {
                var p1 = AbsXY + MapPositionToVector2(_gemSelecteds.Last().MapPosition);
                var p2 = AbsXY + _mousePos;

                batch.Line(p1, p2, _currentColor, 13f);

                //Game1.DrawElectricEffect(
                //    batch,
                //    GFX._whitePixel,
                //    p1,  // Point A
                //    p2,  // Point B
                //    Color.White,            // Couleur de l'éclair
                //    10,                     // Nombre de segments
                //    20f,                    // Amplitude maximale du décalage
                //    (float)gameTime.TotalGameTime.TotalSeconds, // Pour l'animation
                //    4f
                //);

            }
        }
    }
}
