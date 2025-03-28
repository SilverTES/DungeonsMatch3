﻿using AsepriteDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using Mugen.Animation;
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
        public Vector2 Cell;

        public SizeTab(int gridW, int gridH, int cellW, int cellH)
        {
            Grid = new Point(gridW, gridH);
            Cell = new Vector2(cellW, cellH);
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
        Container _divSlotLeft;
        Container _divSlotRight;
        Container _divArena;
        Container _divBattle;

        int _wave = 1;
        int _maxWave = 8;
        public int Wave => _wave;

        //AnimatedSprite _sprite;

        public ScreenPlay()
        {

            //_sprite = Game1._spriteSheetDemonRun.CreateAnimatedSprite("Run");
            //_sprite.Play(0);

            //_sprite.ScaleX = 1f;
            //_sprite.ScaleY = 1f;
            //_sprite.Origin = new Vector2(_sprite.Width / 2, _sprite.Height / 2);
            ////_spriteSlash.Origin = origin;
            //_sprite.Play(1);
            //_sprite.Color = Color.White * .95f;

            _arena = (Arena)new Arena().AppendTo(this);
            _arena.Setup(new SizeTab(8, 8, 64, 64));
            _arena.InitGrid();

            _loop = new Addon.Loop(this);
            _loop.SetLoop(0f, 0f, 2f, .05f, Mugen.Animation.Loops.PINGPONG);
            _loop.Start();

            _battlefield = (BattleField)new BattleField(_arena).AppendTo(this);
            _battlefield.Setup(new SizeTab(40, 16, 32, 32));

            _battlefield.AddRandomEnemy();


            _arena.SetBattlefield(_battlefield);

            _divMain = new Container(Style.Space.One * 4, Style.Space.One * 0, Position.VERTICAL);
            
            _divSlotLeft = new Container(Style.Space.One * 4, Style.Space.One * 10, Position.VERTICAL);
            _divSlotRight = new Container(Style.Space.One * 4, Style.Space.One * 10, Position.VERTICAL);
            _divArena = new Container(Style.Space.One * 4, Style.Space.One * 4, Position.HORIZONTAL);
            _divBattle = new Container(Style.Space.One * 4, Style.Space.One * 4, Position.HORIZONTAL);

            //for (int i = 0; i < _hero.Length; i++)
            //{
            //    _hero[i] = (Hero)new Hero().AppendTo(this);

            //    _container.Add(_hero[i]);
            //}
            //_container.Add(new Hero().SetSize(80, 140).AppendTo(this));
            //_container.Add(new Hero().SetSize(180, 80).AppendTo(this));

            for (int i = 0; i < 2; i++)
            {
                _slot[i] = (Slot)new Slot(_arena, _battlefield).AppendTo(this);
                _divSlotLeft.Insert(_slot[i]);

                _slot[i] = (Slot)new Slot(_arena, _battlefield).Flip().AppendTo(this);
                _divSlotRight.Insert(_slot[i]);
            }

            //_slot[4] = (Slot)new Slot().AppendTo(this);
            //_divArena.Insert(_slot[4]);

            //var hero = (Hero)new Hero().AppendTo(this);
            //_divBattle.Insert(hero);

            _divBattle.Insert(_battlefield);
            //_divBattle.Insert(_divSlotRight);

            _divArena.Insert(_divSlotLeft);
            _divArena.Insert(_arena);
            _divArena.Insert(_divSlotRight);

            //_divMain.Insert(_divSlotLeft);
            _divMain.Insert(_divBattle);
            _divMain.Insert(_divArena);

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

            //var enemies = _battlefield.GroupOf<Enemy>();

            //foreach (Enemy enemy in enemies) 
            //{ 
            //    //Console.WriteLine($"enemy = {enemy}");
            //    enemy.Init();
            //    enemy.MoveTo(new Point(enemy.MapPosition.X, 0));
            //}
            _battlefield.ClearGrid();
            _battlefield.AddRandomEnemy();

        }
        public override Node Update(GameTime gameTime)
        {
            //_sprite.Update(gameTime);

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
            if (ButtonControl.OnePress("InsertArena", _key.IsKeyDown(Keys.D1)))
            {
                _divMain.Insert(1, _divArena);
                _divMain.SetPosition((Game1.ScreenW - _divMain.Rect.Width) / 2, (Game1.ScreenH - _divMain.Rect.Height) / 2);
                _divMain.Refresh();
            }
            if (ButtonControl.OnePress("RemoveArena", _key.IsKeyDown(Keys.D2)))
            {
                _divMain.Remove(_divArena);
                _divMain.SetPosition((Game1.ScreenW - _divMain.Rect.Width) / 2, (Game1.ScreenH - _divMain.Rect.Height) / 2);
                _divMain.Refresh();
            }
            #endregion


            UpdateChilds(gameTime);

            if (_arena.OnFinishTurn)
            {
                Misc.Log("Arena.OnFinishTurn");
                //_battlefield.DoAction();
            }

            // Debug
            //if (_battlefield.GroupOf<Enemy>().Count < 4)
            if (_arena.NbTurns >= _arena.MaxTurns)
            {
                _arena.NbTurns = 0;
                _wave++;

                if (_wave <= _maxWave)
                {
                    _battlefield.AddRandomEnemy(4);
                    //_arena.MaxTurns++;
                }
            }

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Game1.Layers.Main)
            {
                //batch.GraphicsDevice.Clear(HSV.ToRGB(160, 0.5f, 0.25f));

                //batch.Grid(Vector2.Zero, Game1.ScreenW, Game1.ScreenH, 40, 40, Color.Black * .25f, 1f);

                //Game1.DrawCurvedLine(batch, GFX._whitePixel, new Vector2(10, 10), new Vector2(100, 100), new Vector2(100, 0), Color.Yellow, 4f);

            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                var str = $"Wave {Wave}";

                if (_wave >= _maxWave)
                    str = "Final Wave";

                batch.String(Game1._fontMain2, str, Vector2.One * 20 + Vector2.UnitY * 10, Color.Gold, Style.HorizontalAlign.Left);

                //batch.String(Game1._fontMain, $"Nb Node = {_arena.NbActive()}/{_arena.NbNode()}", Vector2.One * 20 + Vector2.UnitY * 40, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Left);
                //batch.String(Game1._fontMain, $"Format Index = {_indexFormat} {_format[_indexFormat].GridSize}", Game1.ScreenW / 2, 20, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Center);

                //DrawColorCircle(batch, new Vector2(Game1.ScreenW / 2, 80));

                //_mainContainer.DrawDebug(batch, Color.Red);
                //_slotContainer.DrawDebug(batch, Color.Red);
                //_battleContainer.DrawDebug(batch, Color.Red);
                // Exemple d'appel pour un effet électrique

                //batch.Draw(Game1._texTrail, new Rectangle(500, 50, 128, 128), Color.White);

                //batch.Draw(_sprite, Game1._mousePos);

                //Game1.DrawFilledCircle(batch, new Vector2(400, 200), 40, Color.Gold * .5f);
                //batch.Draw(Game1._texCircle, new Rectangle(800, 20, 1500, 1500), Color.Red * .5f);

                //Game1.DrawElectricEffect(batch, Game1._texLine, Vector2.One * 100, Game1._mousePos, (float)gameTime.TotalGameTime.TotalSeconds, 3f, 5f, Color.White, Color.Blue *.5f, 80f);

                //batch.FilledCircle(Game1._texCircle, Game1._mousePos, new Vector2(40, 30), _arena.CurrentColor * .5f);
                
                //batch.Point(Game1._mousePos, 3, Color.Black);

                //batch.LineTexture(Game1._texLine, Game1._mousePos, Vector2.Zero, 7, Color.Yellow);
                //Game1.DrawLine(batch, Game1._texLine, Game1._mousePos, Vector2.Zero, 3, Color.Red);

                //batch.CurvedLine(Vector2.One * 40, Game1._mousePos, Vector2.UnitX * Game1._mousePos.X, Color.Yellow, Color.Red, 4);
            }

            if (indexLayer == (int)Game1.Layers.BackFX)
            {
                batch.Draw(Game1._texBG, AbsXY + Vector2.UnitY * _loop._current - Vector2.UnitY * 2, Color.White);
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }

        public void DrawColorCircle(SpriteBatch batch, Vector2 position, float radius = 40f, float thickness = 1f)
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
