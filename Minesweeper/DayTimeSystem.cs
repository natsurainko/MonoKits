using Microsoft.Xna.Framework;
using MonoKits.Spatial3D.Rendering;
using System;

namespace Minesweeper;

public class DayTimeSystem(RenderContext renderContext)
{
    public float TimeOfDay { get; private set; } = 15.0f;

    public float DayDuration { get; set; } = 120f;

    public bool IsPaused { get; set; } = false;

    public Vector3 SceneCenter { get; set; } = Vector3.Zero;
    public float SceneRadius { get; set; } = 200f;


    private static readonly Color[] Colors =
    [
        new (180, 200, 255), // 冷蓝白色 00:00
        new (255, 120, 60), // 深橙红 5:00
        new (255, 230, 180), // 亮黄
        new (255, 255, 250), // 纯白
        new (255, 200, 120), // 金色
        new (255, 80, 40), // 深橙红
    ];

    public void Update(GameTime gameTime)
    {
        if (IsPaused) return;

        float deltaHours = (float)gameTime.ElapsedGameTime.TotalSeconds / DayDuration * 24f;
        TimeOfDay = (TimeOfDay + deltaHours) % 24f;

        UpdateGlobalLight();
    }

    private void UpdateGlobalLight()
    {
        if (renderContext.GlobalLight == null) return;

        float angle = -(TimeOfDay - 6) / 12 * MathF.PI;
        float pitch = MathHelper.PiOver2 * MathF.Sin(angle);
        float yaw = - MathHelper.PiOver2 + angle;

        renderContext.GlobalLight.Rotation = new(pitch, yaw, 0);

        if (TimeOfDay > 18 || TimeOfDay <= 6)
            renderContext.GlobalLight.Rotation = -renderContext.GlobalLight.Rotation;

        renderContext.GlobalLight.FocusOnSceneCenter(SceneCenter, SceneRadius);

        if (TimeOfDay < 4 || TimeOfDay >= 20)
        {
            renderContext.GlobalLight.Color = Colors[0] * 0.2f;
            renderContext.GlobalLight.ShadowIntensity = 0.5f;
        }
        else if (TimeOfDay >= 4 && TimeOfDay < 6)
        {
            renderContext.GlobalLight.Color = Color.Lerp(Colors[0], Colors[1], (TimeOfDay - 4) / 2) * float.Lerp(0.2f, 0, (TimeOfDay - 4) / 2);
            renderContext.GlobalLight.ShadowIntensity = TimeOfDay > 5.5f ? 0 : float.Lerp(0.5f, 0, (TimeOfDay - 4) / 1.5f);
        }
        else if (TimeOfDay >= 6 && TimeOfDay < 8)
        {
            renderContext.GlobalLight.Color = Color.Lerp(Colors[1], Colors[2], (TimeOfDay - 6) / 2) * float.Lerp(0, 0.8f, (TimeOfDay - 6) / 2);
            renderContext.GlobalLight.ShadowIntensity = TimeOfDay < 6.5f ? 0 : float.Lerp(0, 1.0f, (TimeOfDay - 6.5f) / 1.5f);
        }
        else if (TimeOfDay >= 8 && TimeOfDay < 10)
        {
            renderContext.GlobalLight.Color = Color.Lerp(Colors[2], Colors[3], (TimeOfDay - 8) / 2) * 0.8f;
            renderContext.GlobalLight.ShadowIntensity = 1.0f;
        }
        else if (TimeOfDay >= 10 && TimeOfDay < 14)
        {
            renderContext.GlobalLight.Color = Colors[3] * 0.8f;
            renderContext.GlobalLight.ShadowIntensity = 1.0f;
        }
        else if (TimeOfDay >= 14 && TimeOfDay < 16)
        {
            renderContext.GlobalLight.Color = Color.Lerp(Colors[3], Colors[4], (TimeOfDay - 14) / 2) * 0.8f;
            renderContext.GlobalLight.ShadowIntensity = 1.0f;
        }
        else if (TimeOfDay >= 16 && TimeOfDay < 18)
        {
            renderContext.GlobalLight.Color = Color.Lerp(Colors[4], Colors[5], (TimeOfDay - 16) / 2) * float.Lerp(0.8f, 0, (TimeOfDay - 16) / 2);
            renderContext.GlobalLight.ShadowIntensity = TimeOfDay > 17.5f ? 0 : float.Lerp(1.0f, 0, (TimeOfDay - 16) / 1.5f);
        }
        else if (TimeOfDay >= 18 && TimeOfDay < 20)
        {
            renderContext.GlobalLight.Color = Color.Lerp(Colors[5], Colors[0], (TimeOfDay - 18) / 2) * float.Lerp(0, 0.2f, (TimeOfDay - 18) / 2);
            renderContext.GlobalLight.ShadowIntensity = TimeOfDay < 18.5f ? 0 : float.Lerp(0, 0.5f, (TimeOfDay - 18.5f) / 1.5f);
        }
    }
}
