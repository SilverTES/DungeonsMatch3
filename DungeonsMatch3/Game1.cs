using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.Input;

namespace DungeonsMatch3;

public class Game1 : Game
{
    public enum Layers
    {
        Main,
        FX, 
        Debug,
    }

    static public int ScreenW = 1920;
    static public int ScreenH = 1080;

    static public SpriteFont _fontMain;
    static public SpriteFont _fontMedium;
    static public Texture2D _texBG;
    static public Texture2D _texCursorA;
    static public Texture2D _texCursorB;

    static public SoundEffect _soundClock;
    static public SoundEffect _soundPop;
    static public SoundEffect _soundBlockHit;

    static public float _volumeMaster = .5f;

    ScreenPlay _screenPlay;

    static public KeyboardState Key;
    static public MouseState Mouse;

    static public Vector2 _mousePos;

    static public MouseCursor CursorA;
    static public MouseCursor CursorB;

    public Game1()
    {
        WindowManager.Init(this, ScreenW, ScreenH);

        Window.AllowUserResizing = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _screenPlay = new ScreenPlay();
        ScreenManager.Init(_screenPlay, Enums.Count<Layers>(), [(int)Layers.Main, (int)Layers.FX, (int)Layers.Debug]);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _fontMain = Content.Load<SpriteFont>("Fonts/fontMain");
        _fontMedium = Content.Load<SpriteFont>("Fonts/fontMedium");

        _texBG = Content.Load<Texture2D>("Images/background00");
        _texCursorA = Content.Load<Texture2D>("Images/mouse_cursor");
        _texCursorB = Content.Load<Texture2D>("Images/mouse_cursor2");
        CursorA = MouseCursor.FromTexture2D(_texCursorA, 0, 0);
        CursorB = MouseCursor.FromTexture2D(_texCursorB, 0, 0);

        _soundClock = Content.Load<SoundEffect>("Sounds/clock");
        _soundPop = Content.Load<SoundEffect>("Sounds/pop");
        _soundBlockHit = Content.Load<SoundEffect>("Sounds/blockhit");
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
}
