using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoKits.Host;
using MonoKits.Native;
using System;

namespace Minesweeper;

public class MainGame : GameApplication
{
    public MainGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;

        //GraphicsDeviceManager.PreferredBackBufferWidth = 1920;
        //GraphicsDeviceManager.PreferredBackBufferHeight = 1080;

        IsFixedTimeStep = true;
        TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round(1000.0f / 100.0f));
    }

    protected override void Initialize()
    {
        Contents.Title = Texture2D.FromFile(GraphicsDevice, "Content/Images/title.png");
        Contents.Test = Texture2D.FromFile(GraphicsDevice, @"C:\Users\Natsurainko\Desktop\4872c865c4f7595ee4eed024c4ca4da8.png");
        Contents.TiledMap = Content.Load<TiledMap>("tiledmap-assets/tiled/samplemap");

        SDL.SetWindowMinimumSize(Window.Handle, 800, 600);
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        //    Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
}
