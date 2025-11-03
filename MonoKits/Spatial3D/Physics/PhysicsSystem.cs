using BepuPhysics;
using BepuUtilities;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using MonoKits.Spatial3D.Physics.Interfaces;
using System.Collections.Concurrent;

namespace MonoKits.Spatial3D.Physics;

public class PhysicsSystem : IDisposable
{
    private readonly ThreadDispatcher _threadDispatcher;
    private readonly PhysicsObjectMapper _physicsObjectMapper = new();

    private readonly ConcurrentDictionary<IPhysicsBody, BodyHandle> _physicsBodies = [];
    private readonly ConcurrentDictionary<IPhysicsStatic, StaticHandle> _physicsStatics = [];

    public Simulation Simulation { get; }

    public BufferPool BufferPool { get; }

    public PhysicsSystem(BufferPool? bufferPool = null)
    {
        var targetThreadCount = Math.Max(1,
            Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
        _threadDispatcher = new ThreadDispatcher(targetThreadCount);

        BufferPool = bufferPool ?? new();
        Simulation = Simulation.Create(BufferPool,
            new DefaultNarrowPhaseCallbacks(_physicsObjectMapper),
            new DefaultPoseIntegratorCallbacks(_physicsObjectMapper),
            new SolveDescription(8, 1));
    }

    public void Add(IPhysicsBody physicsBody)
    {
        BodyHandle handle = Simulation.Bodies.Add(physicsBody.BodyDescription);
        _physicsBodies.TryAdd(physicsBody, handle);
        _physicsObjectMapper.AddObject(handle, physicsBody);
        physicsBody.OnLoaded(Simulation.Bodies.GetBodyReference(handle));
    }

    public void Add(IPhysicsStatic physicsStatic)
    {
        StaticHandle handle = Simulation.Statics.Add(physicsStatic.StaticDescription);
        _physicsStatics.TryAdd(physicsStatic, handle);
        _physicsObjectMapper.AddObject(handle, physicsStatic);
    }

    public void Remove(IPhysicsBody physicsBody)
    {
        if (!_physicsBodies.TryRemove(physicsBody, out var bodyHandle)) return;
        Simulation.Bodies.Remove(bodyHandle);
        _physicsObjectMapper.TryRemove(bodyHandle, physicsBody.IsKinematic, out _);
    }

    public void Remove(IPhysicsStatic physicsStatic)
    {
        if (!_physicsStatics.TryRemove(physicsStatic, out var staticHandle)) return;
        Simulation.Statics.Remove(staticHandle);
        _physicsObjectMapper.TryRemove(staticHandle, out _);
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (dt <= 0f) return;

        Simulation.Timestep(dt, _threadDispatcher);

        foreach (var item in _physicsBodies)
        {
            var bodyReference = Simulation.Bodies.GetBodyReference(item.Value);
            item.Key.OnUpdate(bodyReference.Pose);
        }
    }

    public void Dispose() => Simulation.Dispose();
}
