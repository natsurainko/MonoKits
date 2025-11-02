using Microsoft.Xna.Framework;

namespace MonoKits.Extensions;

public static class QuaternionExtensions
{
    public static Quaternion CreateFromRotationVector3(Vector3 angles)
        => Quaternion.CreateFromYawPitchRoll(angles.Y, angles.X, angles.Z);

    public static System.Numerics.Quaternion ToNumerics(this Quaternion d) => new(d.X, d.Y, d.Z, d.W);
}
