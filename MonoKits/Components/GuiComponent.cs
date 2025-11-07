using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input.InputListeners;
using MonoKits.Gui.Controls;
using MonoKits.Gui.Input;
using MonoKits.Host;
using KeyboardListener = MonoKits.Overrides.KeyboardListener;
using MouseListener = MonoKits.Overrides.MouseListener;

namespace MonoKits.Components;

public partial class GuiComponent
{
    public static Texture2D? ControlDefaultTexture { get; private set; }

    private readonly MouseListener _mouseListener = new(new MouseListenerSettings { DoubleClickMilliseconds = 0 });
    private readonly KeyboardListener _keyboardListener = new();
    private readonly TouchListener _touchListener = new();

    public static MouseInputManager MouseInputManager { get; private set; } = null!;

    public static KeyboardInputManager KeyboardInputManager { get; private set; } = null!;

    public static TouchInputManager TouchInputManager { get; private set;  } = null!;

    public static SynchronizationContext? SynchronizationContext { get; private set; }
}

public partial class GuiComponent(GameApplication game, Action<CanvasPanel>? contentLoading = default) : GameComponent(game), IDrawable
{
    private bool _viewportInvalidated = true;

    public event EventHandler<EventArgs>? VisibleChanged;
    public event EventHandler<EventArgs>? DrawOrderChanged;

    public CanvasPanel? ContentRoot { get; private set; }

    public bool Visible
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                VisibleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    } = true;

    public int DrawOrder
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                DrawOrderChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    } = 0;

    public override void Initialize()
    {
        SynchronizationContext = game.SynchronizationContext;

        ControlDefaultTexture = new Texture2D(game.GraphicsDevice, 1, 1);
        ControlDefaultTexture.SetData([Color.White]);

        MouseInputManager = new(_mouseListener);
        KeyboardInputManager = new(_keyboardListener);
        TouchInputManager = new(_touchListener);

        ContentRoot = new CanvasPanel()
        {
            SpriteBatch = new(game.GraphicsDevice),
            SynchronizationContext = SynchronizationContext,
        };
        contentLoading?.Invoke(ContentRoot);

        game.Window.ClientSizeChanged += (s, e) => _viewportInvalidated = true;

        game.Window.TextInput += (s, e) =>
        {
            if (FocusManager.FocusedElement is ITextInputReceiver inputReceiver)
                inputReceiver.OnTextInput(e);
        };
    }

    public virtual void Draw(GameTime gameTime)
    {
        ContentRoot?.SpriteBatch?.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            ScissorTestEnable = true
        });
        ContentRoot?.Draw(gameTime);
        ContentRoot?.SpriteBatch?.End();
    }

    public override void Update(GameTime gameTime)
    {
        if (Game.IsActive)
        {
            _mouseListener.Update(gameTime);
            _keyboardListener.Update(gameTime);
            _touchListener.Update(gameTime);
        }

        if (_viewportInvalidated)
        {
            //Viewport viewport = game.GraphicsDevice.Viewport;
            game.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);

            ContentRoot?.UpdateLayout(new Gui.Size
            (
                Game.Window.ClientBounds.Width,
                Game.Window.ClientBounds.Height
            ));

            _viewportInvalidated = false;
        }

        ContentRoot?.Update(gameTime);
    }
}