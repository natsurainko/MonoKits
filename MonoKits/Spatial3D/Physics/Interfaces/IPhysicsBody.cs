using BepuPhysics;
using System.Numerics;

namespace MonoKits.Spatial3D.Physics.Interfaces;

public interface IPhysicsBody
{
    BodyDescription BodyDescription { get; set; }

    Quaternion Orientation { get; set; }

    bool IsKinematic { get; }

    float Mass { get; }

    public void OnUpdate(RigidPose pose) { }

    public void OnLoaded(BodyReference bodyReference) { }
}
