using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoKits.Extensions;

public static class Vector3Extensions
{
    /// <summary>
    /// 球面线性插值 - 保持恒定速度的圆弧插值
    /// </summary>
    /// <param name="a">起始向量</param>
    /// <param name="b">目标向量</param>
    /// <param name="t">插值系数 [0,1]</param>
    /// <returns>插值结果</returns>
    public static Vector3 Slerp(Vector3 a, Vector3 b, float t)
    {
        // 处理边界情况
        if (t <= 0f) return a;
        if (t >= 1f) return b;

        // 计算向量长度和归一化
        float magnitudeA = a.Length();
        float magnitudeB = b.Length();

        // 如果向量长度为零，返回线性插值
        if (magnitudeA < float.Epsilon || magnitudeB < float.Epsilon)
            return Vector3.Lerp(a, b, t);

        Vector3 normalizedA = a / magnitudeA;
        Vector3 normalizedB = b / magnitudeB;

        // 计算点积和夹角
        float dot = Vector3.Dot(normalizedA, normalizedB);

        // 限制点积在[-1,1]范围内，避免浮点误差
        dot = Math.Clamp(dot, -1f, 1f);

        // 计算夹角（弧度）
        float theta = MathF.Acos(dot);

        // 如果夹角很小，使用线性插值（更稳定）
        if (theta < float.Epsilon)
            return Vector3.Lerp(a, b, t);

        // 球面线性插值公式
        float sinTheta = MathF.Sin(theta);
        float factorA = MathF.Sin((1f - t) * theta) / sinTheta;
        float factorB = MathF.Sin(t * theta) / sinTheta;

        // 插值并恢复长度
        Vector3 result = factorA * normalizedA + factorB * normalizedB;
        float interpolatedMagnitude = MathHelper.Lerp(magnitudeA, magnitudeB, t);

        return result * interpolatedMagnitude;
    }

    /// <summary>
    /// 带缓动函数的球面线性插值
    /// </summary>
    public static Vector3 SlerpWithEasing(Vector3 a, Vector3 b, float t, Func<float, float>? easingFunction = null)
    {
        float easedT = easingFunction?.Invoke(t) ?? t;
        return Slerp(a, b, easedT);
    }

    /// <summary>
    /// 在球面上从A到B插值，保持恒定角速度
    /// </summary>
    public static Vector3 SlerpConstantSpeed(Vector3 a, Vector3 b, float t, float maxAngle)
    {
        float angle = MathHelper.ToDegrees(GetAngleBetween(a, b));
        float actualT = t;

        // 如果角度大于最大角度，限制插值速度
        if (angle > maxAngle)
        {
            actualT = (maxAngle / angle) * t;
        }

        return Slerp(a, b, actualT);
    }

    /// <summary>
    /// 计算两个向量之间的夹角（弧度）
    /// </summary>
    private static float GetAngleBetween(Vector3 a, Vector3 b)
    {
        float dot = Vector3.Dot(Vector3.Normalize(a), Vector3.Normalize(b));
        dot = Math.Clamp(dot, -1f, 1f);
        return MathF.Acos(dot);
    }
}
