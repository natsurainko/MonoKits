using BepuPhysics;
using BepuPhysics.Collidables;
using MonoKits.Spatial3D.Physics.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace MonoKits.Spatial3D.Physics;

public class PhysicsObjectMapper
{
    private readonly ConcurrentDictionary<CollidableReference, IPhysicsStatic> _collidableStaticObjects = [];
    private readonly ConcurrentDictionary<CollidableReference, IPhysicsBody> _collidableBodyObjects = [];

    public void AddObject(StaticHandle handle, IPhysicsStatic physicsStatic) => _collidableStaticObjects.TryAdd(new(handle), physicsStatic);

    public void AddObject(BodyHandle handle, IPhysicsBody physicsBody) 
        => _collidableBodyObjects.TryAdd(new(physicsBody.IsKinematic ? CollidableMobility.Kinematic : CollidableMobility.Dynamic, handle), physicsBody);

    public bool TryGetObject(CollidableReference collidable, [NotNullWhen(true)] out object? obj)
    {
        obj = null;

        if (collidable.Mobility == CollidableMobility.Static)
        {
            if (_collidableStaticObjects.TryGetValue(collidable, out var physicsStatic))
            {
                obj = physicsStatic;
                return true;
            }

            return false;
        }
        else
        {
            if (_collidableBodyObjects.TryGetValue(collidable, out var physicsStatic))
            {
                obj = physicsStatic;
                return true;
            }

            return false;
        }
    }

    public bool TryGetStaticObject(CollidableReference collidable, [NotNullWhen(true)] out IPhysicsStatic? obj)
    {
        obj = null;

        if (TryGetObject(collidable, out var _obj) && _obj is IPhysicsStatic physicsStatic)
        {
            obj = physicsStatic;
            return true;
        }

        return false;
    }

    public bool TryGetBodyObject(CollidableReference collidable, [NotNullWhen(true)] out IPhysicsBody? obj)
    {
        obj = null;

        if (TryGetObject(collidable, out var _obj) && _obj is IPhysicsBody physicsBody)
        {
            obj = physicsBody;
            return true;
        }

        return false;
    }

    public bool TryRemove(StaticHandle handle, out IPhysicsStatic? physicsStatic)
    {
        CollidableReference collidable = new(handle);
        return _collidableStaticObjects.TryRemove(collidable, out physicsStatic);
    }

    public bool TryRemove(BodyHandle handle, bool isKinematic, out IPhysicsBody? physicsBody)
    {
        CollidableReference collidable = new(isKinematic ? CollidableMobility.Kinematic : CollidableMobility.Dynamic, handle);
        return _collidableBodyObjects.TryRemove(collidable, out physicsBody);
    }

    public void Clear()
    {
        _collidableStaticObjects.Clear();
        _collidableBodyObjects.Clear();
    }
}
