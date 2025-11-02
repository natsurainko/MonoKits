using System.Numerics;

namespace MonoKits.Extensions;

public static class NumericsExtensions
{
    public static Microsoft.Xna.Framework.Vector3 ToXna(this Vector3 v) => new(v.X, v.Y, v.Z);

    public static Microsoft.Xna.Framework.Quaternion ToXna(this Quaternion q) => new(q.X, q.Y, q.Z, q.W);
}
