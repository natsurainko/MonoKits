namespace MonoKits.Content.Attributes.Scenes;

[AttributeUsage(AttributeTargets.Property)]
public class StaticModelAttribute(string modelProperty) : Attribute
{
    public string ModelProperty { get; init; } = modelProperty;
}