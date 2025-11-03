namespace MonoKits.Content.Attributes.ContentObjects;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ContentObjectAttribute<T> : Attribute
{
    public required string ContentPath { get; set; }

    public string? PropertyName { get; set; }
}