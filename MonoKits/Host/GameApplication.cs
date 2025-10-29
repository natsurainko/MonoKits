using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

namespace MonoKits.Host;

public class GameApplication : Game, IHost
{
    IServiceProvider IHost.Services => Services;

    public GraphicsDeviceManager GraphicsDeviceManager { get; }

    public LoggerFactory LoggerFactory { get; } = new();

    public SynchronizationContext? SynchronizationContext { get; private set; }

    public GameApplication()
    {
        GraphicsDeviceManager = new GraphicsDeviceManager(this);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        var taskCompletionSource = new TaskCompletionSource();
        var hostAppLifetime = Services.GetRequiredService<IHostApplicationLifetime>();

        try
        {
            SynchronizationContext = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(SynchronizationContext);

            this.Run();
            taskCompletionSource.SetResult();
            hostAppLifetime.StopApplication();
        }
        catch (Exception ex)
        {
            taskCompletionSource.SetException(ex);
        }

        return taskCompletionSource.Task;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
