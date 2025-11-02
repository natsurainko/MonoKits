namespace MonoKits.Spatial3D.Physics.Interfaces;

public interface ICollidable
{
    bool IsCollidable { get; }

    int CollisionLayer { get; }

    int CollisionMask { get; }
}