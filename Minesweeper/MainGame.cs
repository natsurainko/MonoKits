using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        GraphicsDeviceManager.PreferMultiSampling = true;
        GraphicsDeviceManager.PreparingDeviceSettings += (s, e) => e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;

        IsFixedTimeStep = true;
        TargetElapsedTime = new TimeSpan(0, 0, 0, 0, (int)Math.Round(1000.0f / 100.0f));
    }

    protected override void Initialize()
    {
        Contents.Title = Texture2D.FromFile(GraphicsDevice, "Content/Images/title.png");

        SDL.SetWindowMinimumSize(Window.Handle, 800, 600);
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
}
