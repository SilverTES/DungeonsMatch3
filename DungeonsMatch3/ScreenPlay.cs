using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;

namespace DungeonsMatch3
{
    class ScreenPlay : Node
    {
        Arena _arena;
        int _indexFormat = 0;
        Arena.Format[] _format = [
            new Arena.Format(6,10,80,80),
            new Arena.Format(8,10,80,80),
            new Arena.Format(10,10,80,80),
            ];

        KeyboardState _key;
        Addon.Loop _loop;

        Hero _hero;

        public ScreenPlay()
        {
            _arena = (Arena)new Arena().AppendTo(this);
            _arena.Setup(_format[0]);
            _arena.InitGrid();
            SetArenaCentered(_arena);

            _loop = new Addon.Loop(this);
            _loop.SetLoop(0f, 0f, 2f, .05f, Mugen.Animation.Loops.PINGPONG);
            _loop.Start();

            _hero = new Hero();
            _hero.SetPosition(420, 140).AppendTo(this);

        }
        public void SetFormat(int index)
        {
            _arena.ClearGrid();
            _arena.Setup(_format[index]);
            SetArenaCentered(_arena);
            _arena.InitGrid();
        }
        public void SetArenaCentered(Arena arena)
        {
            arena.SetPosition((Game1.ScreenW - arena.Rect.Width) / 2, (Game1.ScreenH - arena.Rect.Height) / 2);
        }
        public override Node Update(GameTime gameTime)
        {
            _loop.Update(gameTime);
            UpdateRect();

            _key = Game1.Key;

            if (ButtonControl.OnePress("+", _key.IsKeyDown(Keys.PageUp)) && _indexFormat < _format.Length - 1) 
            { 
                _indexFormat++;  
                SetFormat(_indexFormat); 
            }
            if (ButtonControl.OnePress("-", _key.IsKeyDown(Keys.PageDown)) && _indexFormat > 0)
            {
                _indexFormat--;
                SetFormat(_indexFormat);
            }

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Game1.Layers.Main)
            {
                //batch.GraphicsDevice.Clear(HSV.ToRGB(160, 0.5f, 0.25f));

                batch.Draw(Game1._texBG, AbsXY + Vector2.UnitY * _loop._current, Color.White);

                batch.Grid(Vector2.Zero, Game1.ScreenW, Game1.ScreenH, 40, 40, Color.Black * .25f, 1f);



            }

            if (indexLayer == (int)Game1.Layers.Debug)
            {
                batch.String(Game1._fontMain, $"Nb Node = {_arena.NbActive()}/{_arena.NbNode()}", Vector2.One * 20 + Vector2.UnitY * 40, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Left);
                batch.String(Game1._fontMain, $"Format Index = {_indexFormat} {_format[_indexFormat].GridSize}", Game1.ScreenW / 2, 20, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Center);

                DrawColorCircle(batch, new Vector2(Game1.ScreenW / 2, 80));
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
