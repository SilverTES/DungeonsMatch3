using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AsepriteDotNet.Aseprite;
using MonoGame.Aseprite;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;
using System.Net.Http.Headers;

namespace DungeonsMatch3;

public class Game1 : Game
{
    public enum Layers
    {
        BackFX,
        Main,
        FrontFX, 
        HUD, 
        Debug,
    }

    static public int ScreenW = 1920;
    static public int ScreenH = 1080;

    static public SpriteFont _fontMain;
    static public SpriteFont _fontMain2;
    static public SpriteFont _fontMedium;

    public static SpriteSheet _spriteSheetFireExplosion;
    //public static SpriteSheet _spriteSheetDemonRun;

    static public Texture2D _texGem;
    static public Texture2D _texGemLight;
    static public Texture2D _texGlow;

    static public Texture2D _texAvatar1x1;
    static public Texture2D _texAvatar2x2;
    static public Texture2D _texAvatar2x3;
    static public Texture2D _texAvatar3x3;

    static public Texture2D _texBG;
    static public Texture2D _texCursorA;
    static public Texture2D _texCursorB;
    static public Texture2D _texTrail;
    static public Texture2D _texLightning;
    
    static public Texture2D _texMob00;
    static public Texture2D _texHero00;

    static public SoundEffect _soundClock;
    static public SoundEffect _soundPop;
    static public SoundEffect _soundBlockHit;
    static public SoundEffect _soundSword;
    static public SoundEffect _soundRockSlide;

    static public float _volumeMaster = .5f;

    ScreenPlay _screenPlay;

    static public KeyboardState Key;
    static public MouseState Mouse;

    static public Vector2 _mousePos;

    static public MouseCursor CursorA;
    static public MouseCursor CursorB;

    static public Texture2D _texCircle;
    static public Texture2D _texLine;


    public Game1()
    {
        WindowManager.Init(this, ScreenW, ScreenH);

        Window.AllowUserResizing = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        _screenPlay = new ScreenPlay();
        ScreenManager.Init(_screenPlay, Enums.GetList<Layers>());

        Misc.Log($" NB Layers = { ScreenManager.NbLayers }");

        var layerOrder = ScreenManager.GetLayersOrder();

        for ( int i = 0; i < layerOrder.Count; i++ )
        {
            Console.WriteLine((Layers)layerOrder[i]);
        }

    }

    protected override void LoadContent()
    {
        _texCircle = GFX.CreateCircleTextureAA(GraphicsDevice, 400, 1f);
        _texLine = GFX.CreateLineTextureAA(GraphicsDevice, 40, 7, 5f);

        _fontMain = Content.Load<SpriteFont>("Fonts/fontMain");
        _fontMain2 = Content.Load<SpriteFont>("Fonts/fontMain2");
        _fontMedium = Content.Load<SpriteFont>("Fonts/fontMedium");

        _texGem = Content.Load<Texture2D>("Images/gem");
        _texGemLight = Content.Load<Texture2D>("Images/gemlight");
        _texGlow = Content.Load<Texture2D>("Images/glow");

        _texAvatar1x1 = Content.Load<Texture2D>("Images/avatar1x1");
        _texAvatar2x2 = Content.Load<Texture2D>("Images/avatar2x2");
        _texAvatar2x3 = Content.Load<Texture2D>("Images/avatar2x3");
        _texAvatar3x3 = Content.Load<Texture2D>("Images/avatar3x3");

        _texBG = Content.Load<Texture2D>("Images/background00");
        _texCursorA = Content.Load<Texture2D>("Images/mouse_cursor");
        _texCursorB = Content.Load<Texture2D>("Images/mouse_cursor2");

        _texMob00 = Content.Load<Texture2D>("Images/mob00");
        _texHero00 = Content.Load<Texture2D>("Images/hero00");
        _texTrail = Content.Load<Texture2D>("Images/trail");
        _texLightning = Content.Load<Texture2D>("Images/lightning");

        CursorA = MouseCursor.FromTexture2D(_texCursorA, 0, 0);
        CursorB = MouseCursor.FromTexture2D(_texCursorB, 0, 0);

        _soundClock = Content.Load<SoundEffect>("Sounds/clock");
        _soundPop = Content.Load<SoundEffect>("Sounds/pop");
        _soundBlockHit = Content.Load<SoundEffect>("Sounds/blockhit");
        _soundSword = Content.Load<SoundEffect>("Sounds/sword");
        _soundRockSlide = Content.Load<SoundEffect>("Sounds/rock_slide");

        _spriteSheetFireExplosion = Content.Load<AsepriteFile>("Animations/FireExplosion").CreateSpriteSheet(GraphicsDevice, true);
        //_spriteSheetDemonRun = Content.Load<AsepriteFile>("Animations/Demon_run").CreateSpriteSheet(GraphicsDevice, true);
    }

    protected override void Update(GameTime gameTime)
    {
        Key = Keyboard.GetState();
        Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
        WindowManager.Update(gameTime);

        _mousePos = WindowManager.GetMousePosition();


        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (ButtonControl.OnePress("ToggleFullScreen", Key.IsKeyDown(Keys.F11)))
            WindowManager.ToggleFullscreen();

        ScreenManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        ScreenManager.DrawScreen(gameTime, SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap);
        ScreenManager.ShowScreen(gameTime, SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap);

        base.Draw(gameTime);
    }

    // Méthode pour dessiner une décharge électrique
    public static void DrawElectricEffect(SpriteBatch spriteBatch, Texture2D texLine, Vector2 start, Vector2 end, float time, float thicknessA, float thicknessB, Color colorA, Color colorB, float noiseIntensity = 10f)
    {
        Vector2 direction = end - start;
        float totalLength = direction.Length();
        direction.Normalize();
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X); // Vecteur perpendiculaire

        int segments = (int)(totalLength / texLine.Width) + 1;
        Vector2[] points = new Vector2[segments + 1];
        points[0] = start;
        points[segments] = end;

        // Générer des points intermédiaires avec bruit
        for (int i = 1; i < segments; i++)
        {
            float t = i / (float)segments;
            Vector2 basePos = Vector2.Lerp(start, end, t);
            float noise = (float)(Misc.Rng.NextDouble() - 0.5) * noiseIntensity * (float)Math.Sin(time * 5f);
            points[i] = basePos + perpendicular * noise;
        }

        
        float amount = 0;
        float diff = thicknessB - thicknessA;

        for (float thickness = thicknessA; thickness <= thicknessB; thickness++)
        {
            // Dessiner chaque segment
            for (int i = 0; i < segments; i++)
            {
                Vector2 segmentStart = points[i];
                Vector2 segmentEnd = points[i + 1];
                Vector2 delta = segmentEnd - segmentStart;
                float length = delta.Length();
                float rotation = (float)Math.Atan2(delta.Y, delta.X);

                spriteBatch.Draw(
                    texLine,
                    segmentStart,
                    null,
                    Color.Lerp(colorA, colorB, amount / diff),
                    rotation,
                    new Vector2(0, texLine.Height / 2f),
                    new Vector2(length / texLine.Width, thickness / texLine.Height),
                    SpriteEffects.None,
                    0f);
            }
            amount += .5f;
        }
    }
}
