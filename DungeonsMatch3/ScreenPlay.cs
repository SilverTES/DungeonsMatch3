using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.GUI;
using Mugen.Input;
using Mugen.Physics;
using System;

namespace DungeonsMatch3
{
    public struct SizeTab
    {
        public Point Grid;
        public Point Cell;

        public SizeTab(int gridW, int gridH, int cellW, int cellH)
        {
            Grid = new Point(gridW, gridH);
            Cell = new Point(cellW, cellH);
        }
    }

    class ScreenPlay : Node
    {
        Arena _arena;
        //int _indexSizeTab = 0;
        //SizeTab[] _sizeTab = [
        //    new SizeTab(6, 8, 80, 80),
        //    new SizeTab(8, 10, 80, 80),
        //    new SizeTab(10, 10, 80, 80),
        //    //new Format(6, 10, 80, 80),
        //    //new Format(8, 10, 80, 80),
        //    //new Format(10, 10, 80, 80),
        //    ];

        KeyboardState _key;
        Addon.Loop _loop;

        Hero[] _hero = new Hero[3];

        BattleField _battlefield;

        //Enemy _enemy;

        Slot[] _slot = new Slot[10];

        Container _divMain;
        Container _divSlot;
        Container _divArena;
        Container _divBattle;

        public ScreenPlay()
        {
            

            _arena = (Arena)new Arena().AppendTo(this);
            _arena.Setup(new SizeTab(6, 8, 80, 80));
            _arena.InitGrid();

            _loop = new Addon.Loop(this);
            _loop.SetLoop(0f, 0f, 2f, .05f, Mugen.Animation.Loops.PINGPONG);
            _loop.Start();

            _battlefield = (BattleField)new BattleField().AppendTo(this);
            _battlefield.Setup(new SizeTab(6, 8, 120, 120));

            _battlefield.AddInGrid(new Enemy(_battlefield, new Point(5,0), 3));
            _battlefield.AddInGrid(new Enemy(_battlefield, new Point(5,1), 2));
            _battlefield.AddInGrid(new Enemy(_battlefield, new Point(5,2), 1));

            _divMain = new Container(Style.Space.One * 20, Style.Space.One * 0, Position.HORIZONTAL);
            
            _divSlot = new Container(Style.Space.One * 10, Style.Space.One * 10, Position.VERTICAL);
            _divArena = new Container(Style.Space.One * 10, Style.Space.One * 20, Position.VERTICAL);
            _divBattle = new Container(Style.Space.One * 10, Style.Space.One * 10, Position.VERTICAL);

            //for (int i = 0; i < _hero.Length; i++)
            //{
            //    _hero[i] = (Hero)new Hero().AppendTo(this);

            //    _container.Add(_hero[i]);
            //}
            //_container.Add(new Hero().SetSize(80, 140).AppendTo(this));
            //_container.Add(new Hero().SetSize(180, 80).AppendTo(this));

            for (int i = 0; i < 3; i++)
            {
                _slot[i] = (Slot)new Slot().AppendTo(this);
                _divSlot.Insert(_slot[i]);
            }

            _slot[4] = (Slot)new Slot().AppendTo(this);
            _divArena.Insert(_slot[4]);
            _divArena.Insert(_arena);

            //var hero = (Hero)new Hero().AppendTo(this);
            //_divBattle.Insert(hero);
            _divBattle.Insert(_battlefield);

            _divMain.Insert(_divSlot);
            _divMain.Insert(_divArena);
            _divMain.Insert(_divBattle);

            _divMain.SetPosition((Game1.ScreenW - _divMain.Rect.Width) / 2, (Game1.ScreenH - _divMain.Rect.Height) / 2);
            _divMain.Refresh();


        }
        //public void SetFormat(int index)
        //{
        //    _arena.ClearGrid();
        //    _arena.Setup(_sizeTab[index]);
        //    _container.Refresh();
        //    _arena.InitGrid();
        //}
        public void Shuffle()
        {
            _arena.ClearGrid();
            _arena.InitGrid();

            var enemies = _battlefield.GroupOf<Enemy>();

            foreach (Enemy enemy in enemies) 
            { 
                //Console.WriteLine($"enemy = {enemy}");
                enemy.Init();
                enemy.MoveTo(new Point(BattleField.GridSize.X - 1, enemy.MapPosition.Y));
            }
        }
        public override Node Update(GameTime gameTime)
        {
            _loop.Update(gameTime);
            UpdateRect();

            _key = Game1.Key;

            #region Debug
            //if (ButtonControl.OnePress("+", _key.IsKeyDown(Keys.PageUp)) && _indexSizeTab < _sizeTab.Length - 1) 
            //{ 
            //    _indexSizeTab++;  
            //    SetFormat(_indexSizeTab); 
            //}
            //if (ButtonControl.OnePress("-", _key.IsKeyDown(Keys.PageDown)) && _indexSizeTab > 0)
            //{
            //    _indexSizeTab--;
            //    SetFormat(_indexSizeTab);
            //}
            if (ButtonControl.OnePress("Shuffle", _key.IsKeyDown(Keys.F5)))
            {
                Shuffle();
            }
            #endregion


            UpdateChilds(gameTime);

            if (_arena.OnFinishTurn)
            {
                Console.WriteLine("Arena.OnFinisTurn");
                _battlefield.DoAction();
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Game1.Layers.Main)
            {
                //batch.GraphicsDevice.Clear(HSV.ToRGB(160, 0.5f, 0.25f));

                batch.Draw(Game1._texBG, AbsXY + Vector2.UnitY * _loop._current, Color.White);

                //batch.Grid(Vector2.Zero, Game1.ScreenW, Game1.ScreenH, 40, 40, Color.Black * .25f, 1f);



            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                //batch.String(Game1._fontMain, $"Nb Node = {_arena.NbActive()}/{_arena.NbNode()}", Vector2.One * 20 + Vector2.UnitY * 40, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Left);
                //batch.String(Game1._fontMain, $"Format Index = {_indexFormat} {_format[_indexFormat].GridSize}", Game1.ScreenW / 2, 20, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Center);

                //DrawColorCircle(batch, new Vector2(Game1.ScreenW / 2, 80));

                //_mainContainer.DrawDebug(batch, Color.Red);
                //_slotContainer.DrawDebug(batch, Color.Red);
                //_battleContainer.DrawDebug(batch, Color.Red);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }

        public void DrawColorCircle(SpriteBatch batch, Vector2 position, float radius = 40f, float thickness = 8f)
        {
            Vector2 peri = new Vector2();

            for (int i = 0; i < 360; i++)
            {
                peri.X = (float)Math.Cos(Geo.DegToRad(i)) * radius;
                peri.Y = (float)Math.Sin(Geo.DegToRad(i)) * radius;

                batch.Point(position + peri, thickness, HSV.ToRGB(i, 1, 1) * .5f);
            }
        }

    }
}
