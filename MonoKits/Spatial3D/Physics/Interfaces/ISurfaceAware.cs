namespace MonoKits.Spatial3D.Physics.Interfaces;

public partial interface ISurfaceAware
{
    float Friction { get; }
    float Restitution { get; }
    float SpringStiffness { get; }
    float SpringDamping { get; }
}

public partial interface ISurfaceAware
{
    public const float DefaultFriction = 1f;
    public const float DefaultRestitution = 0f;
    public const float DefaultStiffness = 30f;
    public const float DefaultDamping = 1f;
}