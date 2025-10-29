using Microsoft.Xna.Framework;

namespace MonoKits.Extensions;

public static class QuaternionExtensions
{
    public static Quaternion CreateFromRotationVector3(Vector3 angles)
        => Quaternion.CreateFromYawPitchRoll(angles.Y, angles.X, angles.Z);

    public static Quaternion SafeNormalize(this Quaternion q)
    {
        float norm = q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;

        if (norm > 0.0f)
        {
            float invNorm = 1.0f / MathF.Sqrt(norm);
            return new Quaternion(
                q.X * invNorm,
                q.Y * invNorm,
                q.Z * invNorm,
                q.W * invNorm
            );
        }

        return Quaternion.Identity;
    }

    public static Vector3 Multiply(Vector3 v, Quaternion q)
    {
        Quaternion tmp0 = Quaternion.Conjugate(q);
        Quaternion qv = new(v, 0f);
        Quaternion tmp1 = Quaternion.Multiply(tmp0, qv);
        Quaternion result = Quaternion.Multiply(tmp1, q);
        return new(result.X, result.Y, result.Z);
    }

    public static Vector3 ToEuler(this Quaternion q)
    {
        float xx = q.X;
        float yy = q.Y;
        float zz = q.Z;
        float ww = q.W;
        float xsq = xx * xx;
        float ysq = yy * yy;
        float zsq = zz * zz;

        return new(
            MathF.Atan2(2.0f * (xx * ww - yy * zz), 1.0f - 2.0f * (xsq + zsq)),
            MathF.Atan2(2.0f * (yy * ww + xx * zz), 1.0f - 2.0f * (ysq + zsq)),
            MathF.Asin(2.0f * (xx * yy + zz * ww))
        );
    }
}
