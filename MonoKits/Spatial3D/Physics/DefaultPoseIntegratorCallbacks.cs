using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using MonoKits.Spatial3D.Physics.Interfaces;
using System.Numerics;

namespace MonoKits.Spatial3D.Physics;

public struct DefaultPoseIntegratorCallbacks(PhysicsObjectMapper physicsObjectMapper) : IPoseIntegratorCallbacks
{
    private Simulation? _simulation;

    public float Gravity { get; set; } = -10;

    public readonly AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;

    public readonly bool AllowSubstepsForUnconstrainedBodies => false;

    public readonly bool IntegrateVelocityForKinematics => false;

    public void Initialize(Simulation simulation) => _simulation = simulation;

    public void IntegrateVelocity(
        Vector<int> bodyIndices,
        Vector3Wide position,
        QuaternionWide orientation,
        BodyInertiaWide localInertia,
        Vector<int> integrationMask,
        int workerIndex,
        Vector<float> dt,
        ref BodyVelocityWide velocity)
    {
        Span<int> gravityMask = stackalloc int[Vector<int>.Count];
        gravityMask.Clear();

        for (int i = 0; i < Vector<int>.Count; i++)
        {
            if (integrationMask[i] == 0) continue;
            if (!physicsObjectMapper.TryGetBodyObject(new CollidableReference(CollidableMobility.Dynamic, new(bodyIndices[i])), out var physicsBody)) continue;
            if (physicsBody is not IAccelerationAware accelerationAware) continue;

            if (accelerationAware.AffectedByGravity) gravityMask[i] = 1;
        }

        var gravityY = new Vector<float>(Gravity);
        var shouldIntegrateGravity = Vector.GreaterThan(new(gravityMask), Vector<int>.Zero);
        velocity.Linear.Y = Vector.ConditionalSelect(shouldIntegrateGravity, velocity.Linear.Y + gravityY * dt, velocity.Linear.Y);
    }

    public void PrepareForIntegration(float dt) { }
}
