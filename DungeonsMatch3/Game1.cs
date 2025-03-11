using Microsoft.Xna.Framework;
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
        Debug,
    }

    private SpriteBatch _spriteBatch;

    static public int ScreenW = 1920;
    static public int ScreenH = 1080;

    static public SpriteFont _fontMain;

    ScreenPlay _screenPlay;

    KeyboardState _key;
    MouseState _mouse;

    static public Vector2 _mousePos;

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
        ScreenManager.Init(_screenPlay, Enums.Count<Layers>(), [(int)Layers.Main, (int)Layers.Debug]);


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _fontMain = Content.Load<SpriteFont>("Fonts/fontMain");
    }

    protected override void Update(GameTime gameTime)
    {
        _key = Keyboard.GetState();
        _mouse = Mouse.GetState();
        WindowManager.Update(gameTime);

        _mousePos = WindowManager.GetMousePosition();


        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (ButtonControl.OnePress("ToggleFullScreen", _key.IsKeyDown(Keys.F11)))
            WindowManager.ToggleFullscreen();

        ScreenManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        ScreenManager.DrawScreen(gameTime, SpriteSortMode.Deferred, BlendState.AlphaBlend);
        ScreenManager.ShowScreen(gameTime, SpriteSortMode.Deferred, BlendState.AlphaBlend);

        base.Draw(gameTime);
    }
}
