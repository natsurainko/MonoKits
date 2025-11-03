using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;
using MonoKits.Spatial3D;
using MonoKits.Spatial3D.Physics;

namespace MonoKits.Content;

public abstract class Scene3D<TScopedContent>(TScopedContent scopedContent, ContentManager contentManager, PhysicsSystem? physicsSystem = null)
    where TScopedContent : ScopedContent
{
    protected readonly ContentManager _contentManager = contentManager;
    protected readonly PhysicsSystem? _physicsSystem = physicsSystem;
    protected readonly GraphicsDevice _graphicsDevice = contentManager.GetGraphicsDevice();

    protected TScopedContent ScopedContent { get; } = scopedContent;

    public virtual void Load(SceneManager sceneManager)
    {
        LoadObjects();
        Loading();
        LoadProperties();
        LoadPhysicsObjects(_physicsSystem);
        LoadObjectsIntoScene(sceneManager);
        LoadObjectsIntoPhysicsSystem(_physicsSystem);
        OnLoaded();
    }

    public virtual void Unload() { }

    protected virtual void OnLoaded() { }

    protected virtual void Loading() { }

    protected virtual void LoadObjects() { }

    protected virtual void LoadPhysicsObjects(PhysicsSystem? physicsSystem) { }

    protected virtual void LoadProperties() { }

    protected virtual void LoadObjectsIntoScene(SceneManager sceneManager) { }

    protected virtual void LoadObjectsIntoPhysicsSystem(PhysicsSystem? physicsSystem) { }
}