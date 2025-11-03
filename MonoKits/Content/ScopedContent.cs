using Microsoft.Xna.Framework.Content;

namespace MonoKits.Content;

public abstract class ScopedContent(ContentManager contentManager) : IDisposable
{
    protected readonly ContentManager _contentManager = contentManager;

    public abstract void Load();

    public abstract void Dispose();

    protected virtual void OnLoaded() { }
}