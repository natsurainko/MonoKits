namespace MonoKits.Content.Attributes.Scenes;

[AttributeUsage(AttributeTargets.Property)]
public class SceneObjectRotationAttribute(float x, float y, float z) : Attribute
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Z { get; } = z;
}