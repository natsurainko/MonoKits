using System.Numerics;

namespace MonoKits.Spatial3D.Physics.Interfaces;

public interface IAccelerationAware
{
    bool AffectedByGravity { get; }

    Vector3 CustomAcceleration { get; }
}