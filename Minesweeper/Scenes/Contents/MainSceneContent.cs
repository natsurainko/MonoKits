using Microsoft.Xna.Framework.Graphics;
using MonoKits.Content.Attributes.ContentObjects;

namespace Minesweeper.Scenes.Contents;

[ContentObject<Model>(ContentPath = "Models/Starvalley")]
[ContentObject<Model>(ContentPath = "Models/QuickPlane")]
[ContentObject<Model>(ContentPath = "Models/Board")]
[ContentObject<Model>(ContentPath = "Models/House")]
[ContentObject<Model>(ContentPath = "Models/Block")]
[Texture2D(ContentPath = "Content/Images/title.png", PropertyName = "Title")]
[Texture2D(ContentPath = "Content/Images/bait.png", PropertyName = "Bait")]
internal partial class MainSceneContent;