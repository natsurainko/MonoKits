using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MonoKits.Gui.Controls;

public abstract partial class Panel : UIElement
{
    protected readonly ObservableCollection<UIElement> _children;

    public IList<UIElement> Children => _children;

    public Panel()
    {
        _children = [];
        _children.CollectionChanged += OnChildrenCollectionChanged;
    }

    protected virtual void OnChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems?.Count > 0)
        {
            for (int i = 0; i < e.NewItems.Count; i++)
            {
                if (e.NewItems[i] is UIElement uIElement)
                    uIElement.Parent = this;
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems?.Count > 0)
        {
            for (int i = 0; i < e.OldItems.Count; i++)
            {
                if (e.OldItems[i] is UIElement uIElement)
                    uIElement.Parent = null;
            }
        }

        InvalidateVisual();
    }

    protected override void DrawWithClipping(GameTime gameTime)
    {
        Rectangle clipRect = Bounds + Padding;

        if (clipRect.Width <= 0 || clipRect.Height <= 0) return;

        var device = SpriteBatch!.GraphicsDevice;
        var originalScissorRect = device.ScissorRectangle;

        device.ScissorRectangle = Rectangle.Intersect(Bounds + Padding, originalScissorRect);

        DrawOverride(gameTime);

        device.ScissorRectangle = originalScissorRect;
    }

    protected override void DrawOverride(GameTime gameTime)
    {
        base.DrawOverride(gameTime);

        for (int i = 0; i < Children.Count; i++)
            Children[i]?.Draw(gameTime);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        for (int i = 0; i < Children.Count; i++)
            Children[i]?.Update(gameTime);
    }
}