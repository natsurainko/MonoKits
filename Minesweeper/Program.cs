using Microsoft.Extensions.Hosting;
using Minesweeper;
using Minesweeper.Layers;
using MonoKits.Components;
using MonoKits.Host;

var builder = new GameApplicationBuilder<MainGame>();

builder.AddComponent(game => new GuiComponent(game, root =>
{
    Game3DLayer game3DLayer = new(game);
    UILayer uILayer = new(game, game3DLayer);

    root.Children.Add(game3DLayer);
    root.Children.Add(uILayer);
}));

IHost host = builder.Build();
host.Run();