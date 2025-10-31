using Microsoft.Xna.Framework;

namespace MonoKits.Spatial3D.Collision;

public interface ICollidable
{
    public BoundingBox BoundingBox { get; set; }
}
