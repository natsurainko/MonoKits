using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Extensions;
using MonoKits.Spatial3D.Collision;
using MonoKits.Spatial3D.Objects;

namespace Minesweeper.Game3D;

internal partial class Player(Model model) : ModelObject3D(model), ICollidable
{
    public BoundingBox BoundingBox { get; set; } = model.CreateBoundingBoxFromVertices();
}
