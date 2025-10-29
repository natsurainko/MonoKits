using FontStashSharp;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;


//using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Minesweeper;

internal static class Contents
{
    public readonly static FontSystem FontSystem;

    public static DynamicSpriteFont Unifont { get; set; } = null!;

    //public static SpriteFont Unifont_Default { get; set; } = null!;

    public static Texture2D Title { get; set; } = null!;

    public static Texture2D Test { get; set; } = null!;

    public static TiledMap TiledMap { get; set; } = null!;

    static Contents()
    {
        FontSystemDefaults.FontResolutionFactor = 2.0f;
        FontSystemDefaults.KernelWidth = 1;
        FontSystemDefaults.KernelHeight = 1;

        FontSystem = new FontSystem();
        FontSystem.AddFont(File.ReadAllBytes(@"Content/Fonts/unifont-17.0.02.otf"));

        Unifont = FontSystem.GetFont(16);
    }
}
