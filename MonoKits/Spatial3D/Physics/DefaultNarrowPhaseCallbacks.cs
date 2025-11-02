using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using MonoKits.Spatial3D.Physics.Interfaces;

namespace MonoKits.Spatial3D.Physics;

public readonly struct DefaultNarrowPhaseCallbacks(PhysicsObjectMapper physicsObjectMapper) : INarrowPhaseCallbacks
{
    public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
    {
        if (!physicsObjectMapper.TryGetObject(a, out var objA)) return false;
        if (!physicsObjectMapper.TryGetObject(b, out var objB)) return false;

        if (objA is not ICollidable collidableA) return false;
        if (objB is not ICollidable collidableB) return false;

        if (!collidableA.IsCollidable) return false;
        if (!collidableB.IsCollidable) return false;

        bool aCanCollideWithB = (collidableA.CollisionMask & (1 << collidableB.CollisionLayer)) != 0;
        bool bCanCollideWithA = (collidableB.CollisionMask & (1 << collidableA.CollisionLayer)) != 0;

        if (!aCanCollideWithB || !bCanCollideWithA) return false;

        return true;
    }

    public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) => true;

    public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) 
        where TManifold : unmanaged, IContactManifold<TManifold>
    {
        pairMaterial = new();

        if (!physicsObjectMapper.TryGetObject(pair.A, out var objA)) return false;
        if (!physicsObjectMapper.TryGetObject(pair.B, out var objB)) return false;

        ISurfaceAware? surfaceA = objA as ISurfaceAware;
        ISurfaceAware? surfaceB = objB as ISurfaceAware;

        if (surfaceA != null && surfaceB != null)
        {
            pairMaterial.FrictionCoefficient = MathF.Sqrt(surfaceA.Friction * surfaceB.Friction);
            pairMaterial.MaximumRecoveryVelocity = 2f * (1f + MathF.Max(surfaceA.Restitution, surfaceB.Restitution));

            pairMaterial.SpringSettings = new(
                (surfaceA.SpringStiffness + surfaceB.SpringStiffness) * 0.5f,
                (surfaceA.SpringDamping + surfaceB.SpringDamping) * 0.5f);
        }
        else if (surfaceA != null)
        {
            pairMaterial.FrictionCoefficient = MathF.Sqrt(surfaceA.Friction * ISurfaceAware.DefaultFriction);
            pairMaterial.MaximumRecoveryVelocity = 2f * (1f + MathF.Max(surfaceA.Restitution, ISurfaceAware.DefaultRestitution));

            pairMaterial.SpringSettings = new(
                (surfaceA.SpringStiffness + ISurfaceAware.DefaultStiffness) * 0.5f,
                (surfaceA.SpringDamping + ISurfaceAware.DefaultDamping) * 0.5f);

        }
        else if (surfaceB != null)
        {
            pairMaterial.FrictionCoefficient = MathF.Sqrt(surfaceB.Friction * ISurfaceAware.DefaultFriction);
            pairMaterial.MaximumRecoveryVelocity = 2f * (1f + MathF.Max(surfaceB.Restitution, ISurfaceAware.DefaultRestitution));

            pairMaterial.SpringSettings = new(
                (surfaceB.SpringStiffness + ISurfaceAware.DefaultStiffness) * 0.5f,
                (surfaceB.SpringDamping + ISurfaceAware.DefaultDamping) * 0.5f);
        }
        else
        {
            pairMaterial.FrictionCoefficient = ISurfaceAware.DefaultFriction;
            pairMaterial.MaximumRecoveryVelocity = 2f * (1f + ISurfaceAware.DefaultRestitution);
            pairMaterial.SpringSettings = new(ISurfaceAware.DefaultStiffness, ISurfaceAware.DefaultDamping);
        }

        return true;
    }

    public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold) => true;

    public void Dispose() { }

    public void Initialize(Simulation simulation) { }
}
