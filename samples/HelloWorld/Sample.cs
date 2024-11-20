// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using JoltPhysicsSharp;

namespace HelloWorld;

public abstract class Sample : IDisposable
{
    private const int MaxBodies = 65536;
    private const int MaxBodyPairs = 65536;
    private const int MaxContactConstraints = 65536;
    private const int NumBodyMutexes = 0;

    protected static class Layers
    {
        public static readonly ObjectLayer NonMoving = 0;
        public static readonly ObjectLayer Moving = 1;
    };

    protected static class BroadPhaseLayers
    {
        public static readonly BroadPhaseLayer NonMoving = 0;
        public static readonly BroadPhaseLayer Moving = 1;
    };

    protected const int NumLayers = 2;
    protected PhysicsSystemSettings _settings;
    protected readonly List<Body> _bodies = [];

    protected Sample()
    {
        _settings = new PhysicsSystemSettings()
        {
            MaxBodies = MaxBodies,
            MaxBodyPairs = MaxBodyPairs,
            MaxContactConstraints = MaxContactConstraints,
            NumBodyMutexes = NumBodyMutexes,
        };

        JobSystem = new JobSystemThreadPool();
    }

    public JobSystem JobSystem { get; set; }
    public PhysicsSystem? System { get; private set; }
    public BodyInterface BodyInterface => System!.BodyInterface;
    public BodyLockInterface BodyLockInterface => System!.BodyLockInterface;

    public virtual void Dispose()
    {
        foreach (Body body in _bodies)
        {
            BodyInterface.RemoveAndDestroyBody(body.ID);
        }
        _bodies.Clear();

        JobSystem.Dispose();
        System?.Dispose();
    }

    public virtual void Initialize()
    {
        SetupCollisionFiltering();
        System = new(_settings);

        // ContactListener
        System.OnContactValidate += OnContactValidate;
        System.OnContactAdded += OnContactAdded;
        System.OnContactPersisted += OnContactPersisted;
        System.OnContactRemoved += OnContactRemoved;
        // BodyActivationListener
        System.OnBodyActivated += OnBodyActivated;
        System.OnBodyDeactivated += OnBodyDeactivated;
    }

    protected virtual void SetupCollisionFiltering()
    {
        // We use only 2 layers: one for non-moving objects and one for moving objects
        ObjectLayerPairFilterTable objectLayerPairFilter = new(2);
        objectLayerPairFilter.EnableCollision(Layers.NonMoving, Layers.Moving);
        objectLayerPairFilter.EnableCollision(Layers.Moving, Layers.Moving);

        // We use a 1-to-1 mapping between object layers and broadphase layers
        BroadPhaseLayerInterfaceTable broadPhaseLayerInterface = new(2, 2);
        broadPhaseLayerInterface.MapObjectToBroadPhaseLayer(Layers.NonMoving, BroadPhaseLayers.NonMoving);
        broadPhaseLayerInterface.MapObjectToBroadPhaseLayer(Layers.Moving, BroadPhaseLayers.Moving);

        ObjectVsBroadPhaseLayerFilterTable objectVsBroadPhaseLayerFilter = new(broadPhaseLayerInterface, 2, objectLayerPairFilter, 2);

        _settings.ObjectLayerPairFilter = objectLayerPairFilter;
        _settings.BroadPhaseLayerInterface = broadPhaseLayerInterface;
        _settings.ObjectVsBroadPhaseLayerFilter = objectVsBroadPhaseLayerFilter;
    }

    public abstract void Run();

    protected Body CreateFloor(float size, ObjectLayer layer)
    {
        BoxShape shape = new(new Vector3(size, 0.5f, size));
        using BodyCreationSettings creationSettings = new(shape, new Vector3(0, -0.5f, 0.0f), Quaternion.Identity, MotionType.Static, layer);
        Body body = BodyInterface.CreateBody(creationSettings);
        BodyInterface.AddBody(body.ID, Activation.DontActivate);
        _bodies.Add(body);
        return body;
    }

    protected Body CreateBox(in Vector3 halfExtent,
        in Vector3 position,
        in Quaternion rotation,
        MotionType motionType,
        ObjectLayer layer,
        Activation activation = Activation.Activate)
    {
        BoxShape shape = new(halfExtent);
        using BodyCreationSettings creationSettings = new(shape, position, rotation, motionType, layer);
        Body body = BodyInterface.CreateBody(creationSettings);
        BodyInterface.AddBody(body.ID, activation);
        _bodies.Add(body);
        return body;
    }

    protected Body CreateSphere(float radius,
        in Vector3 position,
        in Quaternion rotation,
        MotionType motionType,
        ObjectLayer layer,
        Activation activation = Activation.Activate)
    {
        SphereShape shape = new(radius);
        using BodyCreationSettings creationSettings = new(shape, position, rotation, motionType, layer);
        Body body = BodyInterface.CreateBody(creationSettings);
        BodyInterface.AddBody(body.ID, activation);
        _bodies.Add(body);
        return body;
    }

    protected virtual ValidateResult OnContactValidate(PhysicsSystem system, in Body body1, in Body body2, Double3 baseOffset, nint collisionResult)
    {
        Console.WriteLine("Contact validate callback");

        // Allows you to ignore a contact before it is created (using layers to not make objects collide is cheaper!)
        return ValidateResult.AcceptAllContactsForThisBodyPair;
    }

    protected virtual void OnContactAdded(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, in ContactSettings settings)
    {
        Console.WriteLine("A contact was added");
    }

    protected virtual void OnContactPersisted(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, in ContactSettings settings)
    {
        Console.WriteLine("A contact was persisted");
    }

    protected virtual void OnContactRemoved(PhysicsSystem system, ref SubShapeIDPair subShapePair)
    {
        Console.WriteLine("A contact was removed");
    }

    protected virtual void OnBodyActivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        Console.WriteLine("A body got activated");
    }

    protected virtual void OnBodyDeactivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        Console.WriteLine("A body went to sleep");
    }
}
