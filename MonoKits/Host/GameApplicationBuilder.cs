using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

namespace MonoKits.Host;

public class GameApplicationBuilder<TGameApplication> where TGameApplication : GameApplication, new()
{
    private readonly TGameApplication _host;

    public GameServiceContainer Services => _host.Services;

    public GameComponentCollection Components => _host.Components;

    public GameApplicationBuilder()
    {
        _host = new();

        _host.Services.AddService<IHostApplicationLifetime>(
            new ApplicationLifetime(_host.LoggerFactory.CreateLogger<ApplicationLifetime>()));
    }

    public void ConfigureGame(Action<GameApplication> action) => action(_host);

    public void AddService(Func<GameApplication, object> func) => Services.AddService(func(_host));

    public void AddComponent(Func<GameApplication, IGameComponent> func) => Components.Add(func(_host));

    public IHost Build() => _host;
}
