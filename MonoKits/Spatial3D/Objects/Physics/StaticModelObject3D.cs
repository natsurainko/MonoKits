using BepuPhysics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Spatial3D.Physics.Interfaces;

namespace MonoKits.Spatial3D.Objects.Physics;

public partial class StaticModelObject3D(Model model) : ModelObject3D(model), IPhysicsStatic, ICollidable, ISurfaceAware
{
    public bool IsCollidable { get; set; } = true;

    public int CollisionLayer { get; set; } = 1;

    public int CollisionMask { get; set; } = ~0;

    public StaticDescription StaticDescription { get; set; }

    public float Friction { get; set; } = ISurfaceAware.DefaultFriction;

    public float Restitution { get; set; } = ISurfaceAware.DefaultRestitution;

    public float SpringStiffness { get; set; } = ISurfaceAware.DefaultStiffness;

    public float SpringDamping { get; set; } = ISurfaceAware.DefaultDamping;
}

public partial class StaticModelObject3D
{
    public static new StaticModelObject3D LoadFromContent(ContentManager content, string modelPath) => new(content.Load<Model>(modelPath));
}
