using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKits.Spatial3D;

namespace Minesweeper.Game3D;

internal partial class Player(Model model) : GameObject3D(model)
{
    /// <summary>
    /// Move the player in its relative orientation
    /// </summary>
    /// <param name="offset">offset == (forward, up, right)</param>
    public void Move(Vector3 movement)
    {
        Quaternion orientation = Quaternion.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);

        // Add translation
        Vector3 forward = Vector3.Transform(Vector3.Forward, orientation);
        Vector3 right = Vector3.Transform(Vector3.Right, orientation);

        Position += forward * movement.X;
        Position += Vector3.Up * movement.Y;
        Position += right * movement.Z;
    }
}
