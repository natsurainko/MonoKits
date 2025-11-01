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
        Vector3 angles = Vector3.Zero;

        // 绕X轴旋转的角度 (Pitch)
        float sinPitch = 2.0f * (q.W * q.X + q.Y * q.Z);
        float cosPitch = 1.0f - 2.0f * (q.X * q.X + q.Y * q.Y);
        angles.X = MathF.Atan2(sinPitch, cosPitch);

        // 绕Y轴旋转的角度 (Yaw)
        float sinYaw = 2.0f * (q.W * q.Y - q.Z * q.X);
        if (MathF.Abs(sinYaw) >= 1.0f)
            angles.Y = (MathF.PI / 2.0f * MathF.Sign(sinYaw)); // 90度处理
        else
            angles.Y = MathF.Asin(sinYaw);

        // 绕Z轴旋转的角度 (Roll)
        float sinRoll = 2.0f * (q.W * q.Z + q.X * q.Y);
        float cosRoll = 1.0f - 2.0f * (q.Y * q.Y + q.Z * q.Z);
        angles.Z = MathF.Atan2(sinRoll, cosRoll);

        return angles;
    }
}
