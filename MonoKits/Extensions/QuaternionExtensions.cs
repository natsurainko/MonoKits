using Microsoft.Xna.Framework;

namespace MonoKits.Extensions;

public static class QuaternionExtensions
{
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
            MathF.Atan2(2.0f * (yy * ww + xx * zz), 1.0f - 2.0f * (ysq + zsq)), //
            MathF.Asin(2.0f * (xx * yy + zz * ww)) //Yaw
        );
    }

    public static void GetEulerAngles(this Quaternion rotation, out float yaw, out float pitch, out float roll)
    {
        Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);

        // 计算 Yaw
        yaw = MathF.Atan2(forward.X, forward.Z);

        // 计算 Pitch
        float horizontalLength = MathF.Sqrt(forward.X * forward.X + forward.Z * forward.Z);
        pitch = MathF.Atan2(forward.Y, horizontalLength);

        // Roll 可以从 Up 向量提取
        Vector3 up = Vector3.Transform(Vector3.Up, rotation);
        Vector3 right = Vector3.Cross(forward, up);
        Vector3 expectedRight = Vector3.Cross(forward, Vector3.Up);

        if (expectedRight.LengthSquared() > 0.0001f && right.LengthSquared() > 0.0001f)
        {
            expectedRight.Normalize();
            right.Normalize();
            float rollDot = Vector3.Dot(right, expectedRight);
            roll = MathF.Acos(MathHelper.Clamp(rollDot, -1f, 1f));

            // 判断 Roll 的正负
            if (Vector3.Dot(Vector3.Cross(right, expectedRight), forward) < 0)
            {
                roll = -roll;
            }
        }
        else
        {
            roll = 0;
        }
    }
}
