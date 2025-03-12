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

        KeyboardState _key;

        Arena.Dimension[] _dimension = [
            new Arena.Dimension(6,6,80,80),
            new Arena.Dimension(8,8,80,80),
            new Arena.Dimension(10,10,80,80),
            new Arena.Dimension(12,12,80,80),
            ];

        int _indexDim = 0;

        public ScreenPlay()
        {
            _arena = (Arena)new Arena().AppendTo(this);
            _arena.Setup(_dimension[0]);
            _arena.InitGrid();
            SetArenaCentered(_arena);
        }
        public void SetDimension(int index)
        {
            _arena.ClearGrid();
            _arena.Setup(_dimension[index]);
            SetArenaCentered(_arena);
            _arena.InitGrid();
        }
        public void SetArenaCentered(Arena arena)
        {
            arena.SetPosition((Game1.ScreenW - arena.Rect.Width) / 2, (Game1.ScreenH - arena.Rect.Height) / 2);
        }
        public override Node Update(GameTime gameTime)
        {
            UpdateRect();

            _key = Game1.Key;

            if (ButtonControl.OnePress("+", _key.IsKeyDown(Keys.PageUp)) && _indexDim < _dimension.Length - 1) 
            { 
                _indexDim++;  
                SetDimension(_indexDim); 
            }
            if (ButtonControl.OnePress("-", _key.IsKeyDown(Keys.PageDown)) && _indexDim > 0)
            {
                _indexDim--;
                SetDimension(_indexDim);
            }

            UpdateChilds(gameTime);

            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.GraphicsDevice.Clear(HSV.ToRGB(200, 0.4f, 0.25f));

                batch.Grid(Vector2.Zero, Game1.ScreenW, Game1.ScreenH, 40, 40, Color.Gray * .5f, 1f);

            }

            if (indexLayer == (int)Game1.Layers.Main)
            {
                batch.String(Game1._fontMain, $" Nb Node = {_arena.NbActive()}/{_arena.NbNode()}", Vector2.One * 20 + Vector2.UnitY * 40, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Left);

                batch.String(Game1._fontMain, $"Dimension Index = {_indexDim} {_dimension[_indexDim].GridSize}", Game1.ScreenW / 2, 20, Color.Yellow, Mugen.GUI.Style.HorizontalAlign.Center);

                for (int i = 0; i < 360; i++)
                {
                    Vector2 pos = new Vector2(240, 240);

                    Vector2 peri = new Vector2();
                    peri.X = (float)Math.Cos(Geo.DegToRad(i)) * 40;
                    peri.Y = (float)Math.Sin(Geo.DegToRad(i)) * 40;

                    batch.Point(pos + peri, 8, HSV.ToRGB(i, 1, 1) * .5f);
                }
            }

            DrawChilds(batch, gameTime, indexLayer);

            return base.Draw(batch, gameTime, indexLayer);
        }

    }
}
