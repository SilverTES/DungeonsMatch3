using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using Mugen.Physics;
using System;

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

    static public Texture2D _texAvatar1x1;
    static public Texture2D _texAvatar2x2;
    static public Texture2D _texAvatar2x3;

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

    public Game1()
    {
        WindowManager.Init(this, ScreenW, ScreenH);

        Window.AllowUserResizing = true;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    /// <summary>
    /// Dessine un effet électrique entre pointA et pointB.
    /// </summary>
    /// <param name="spriteBatch">Le SpriteBatch pour dessiner.</param>
    /// <param name="pixel">Texture d'un pixel pour le rendu.</param>
    /// <param name="pointA">Point de départ.</param>
    /// <param name="pointB">Point d'arrivée.</param>
    /// <param name="color">Couleur de l'effet.</param>
    /// <param name="segments">Nombre de segments pour la ligne.</param>
    /// <param name="maxOffset">Amplitude maximale du décalage aléatoire.</param>
    /// <param name="time">Temps pour l'animation.</param>
    public static void DrawElectricEffect(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pointA, Vector2 pointB, Color color, int segments, float maxOffset, float time)
    {
        Vector2[] points = new Vector2[segments + 1];

        points[0] = pointA; // Premier point
        points[segments] = pointB; // Dernier point

        // Calculer la direction de la ligne
        Vector2 direction = pointB - pointA;
        float length = direction.Length();
        Vector2 normalizedDirection = Vector2.Normalize(direction);
        Vector2 perpendicular = new Vector2(-normalizedDirection.Y, normalizedDirection.X); // Vecteur perpendiculaire

        // Générer les points intermédiaires avec décalage aléatoire
        for (int i = 1; i < segments; i++)
        {
            float t = i / (float)segments;
            Vector2 basePoint = pointA + direction * t;

            // Ajouter un décalage aléatoire avec animation
            float offset = (float)(Misc.Rng.NextDouble() - 0.5) * maxOffset * (float)Math.Sin(time * 5 + i);
            points[i] = basePoint + perpendicular * offset;
        }

        // Dessiner les segments entre les points
        for (int i = 0; i < segments; i++)
        {
            Vector2 start = points[i];
            Vector2 end = points[i + 1];

            float distance = Vector2.Distance(start, end);
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            spriteBatch.Draw(
                pixel,
                start,
                null,
                color * (0.8f + (float)Misc.Rng.NextDouble() * 0.2f), // Variation légère de l'opacité
                angle,
                Vector2.Zero,
                new Vector2(distance, 2), // Épaisseur de 2 pixels
                SpriteEffects.None,
                0f
            );
        }
    }


    /// <summary>
    /// Dessine une ligne incurvée entre pointA et pointB avec un point de contrôle pointC.
    /// </summary>
    /// <param name="spriteBatch">Le SpriteBatch pour dessiner.</param>
    /// <param name="pixel">Texture d'un pixel pour le rendu.</param>
    /// <param name="pointA">Point de départ.</param>
    /// <param name="pointB">Point d'arrivée.</param>
    /// <param name="pointC">Point de contrôle pour la courbure.</param>
    /// <param name="color">Couleur de la ligne.</param>
    public static void DrawCurvedLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pointA, Vector2 pointB, Vector2 pointC, Color colorA, Color colorB, float thickness = 1f, int nbSegments = 50)
    {
        int segments = nbSegments; // Nombre de segments pour la courbe
        Vector2 previousPoint = pointA;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;

            // Calculer le point sur la courbe de Bézier quadratique
            float tSquared = t * t;
            float oneMinusT = 1 - t;
            float oneMinusTSquared = oneMinusT * oneMinusT;
            Vector2 currentPoint = oneMinusTSquared * pointA + 2 * oneMinusT * t * pointC + tSquared * pointB;

            // Dessiner un segment entre previousPoint et currentPoint
            float distance = Vector2.Distance(previousPoint, currentPoint);
            float angle = (float)Math.Atan2(currentPoint.Y - previousPoint.Y, currentPoint.X - previousPoint.X);

            //spriteBatch.Draw(
            //    pixel,
            //    previousPoint,
            //    null,
            //    Color.Lerp(colorA, colorB, (float)i / (float)segments),
            //    angle,
            //    Vector2.Zero,
            //    new Vector2(distance, thickness), // Étirer le pixel pour former une ligne
            //    SpriteEffects.None,
            //    0f
            //);

            spriteBatch.Point(previousPoint, thickness, Color.Lerp(colorA, colorB, (float)i / (float)segments));
            spriteBatch.FillRectangleCentered(previousPoint, Vector2.One * thickness, Color.Lerp(colorA, colorB, (float)i / (float)segments), Geo.RAD_45);

            previousPoint = currentPoint;
        }
    }

    protected override void Initialize()
    {
        _screenPlay = new ScreenPlay();
        ScreenManager.Init(_screenPlay, Enums.Count<Layers>(), 
            [
            (int)Layers.BackFX,
            (int)Layers.Main, 
            (int)Layers.FrontFX, 
            (int)Layers.HUD, 
            (int)Layers.Debug,
            ]);

        //ScreenManager.SetLayersOrder([

        //    (int)Layers.BackFX,
        //    (int)Layers.Main,
        //    (int)Layers.HUD,
        //    (int)Layers.FrontFX,
        //    (int)Layers.Debug,

        //]);

        Console.WriteLine($" NB Layers = { ScreenManager.NbLayers }");

        var layerOrder = ScreenManager.GetLayersOrder();

        for ( int i = 0; i < layerOrder.Count; i++ )
        {
            Console.WriteLine((Layers)layerOrder[i]);
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _fontMain = Content.Load<SpriteFont>("Fonts/fontMain");
        _fontMain2 = Content.Load<SpriteFont>("Fonts/fontMain2");
        _fontMedium = Content.Load<SpriteFont>("Fonts/fontMedium");

        _texAvatar1x1 = Content.Load<Texture2D>("Images/avatar1x1");
        _texAvatar2x2 = Content.Load<Texture2D>("Images/avatar2x2");
        _texAvatar2x3 = Content.Load<Texture2D>("Images/avatar2x3");

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
