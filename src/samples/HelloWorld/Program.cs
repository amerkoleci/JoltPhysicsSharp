// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Security.AccessControl;
using JoltPhysicsSharp;

namespace HelloWorld;

public static class Program
{
    private const uint MaxBodies = 1024;

    private const uint NumBodyMutexes = 0;
    private const uint MaxBodyPairs = 1024;
    private const uint MaxContactConstraints = 1024;

    static class Layers
    {
        public const byte NonMoving = 0;
        public const byte Moving = 1;
        public const int NumLayers = 2;
    };

    static class BroadPhaseLayers
    {
        public const byte NonMoving = 0;
        public const byte Moving = 1;
        public const int NumLayers = 2;
    };

    class BPLayerInterfaceImpl : BroadPhaseLayerInterface
    {
        private readonly BroadPhaseLayer[] _objectToBroadPhase = new BroadPhaseLayer[Layers.NumLayers];

        public BPLayerInterfaceImpl()
        {
            // Create a mapping table from object to broad phase layer
            _objectToBroadPhase[Layers.NonMoving] = BroadPhaseLayers.NonMoving;
            _objectToBroadPhase[Layers.Moving] = BroadPhaseLayers.Moving;
        }

        protected override int GetNumBroadPhaseLayers()
        {
            return BroadPhaseLayers.NumLayers;
        }

        protected override BroadPhaseLayer GetBroadPhaseLayer(ObjectLayer layer)
        {
            Debug.Assert(layer < Layers.NumLayers);
            return _objectToBroadPhase[layer];
        }

        protected override string GetBroadPhaseLayerName(BroadPhaseLayer layer)
        {
            switch ((byte)layer)
            {
                case BroadPhaseLayers.NonMoving: return "NON_MOVING";
                case BroadPhaseLayers.Moving: return "MOVING";
                default:
                    Debug.Assert(false);
                    return "INVALID";
            }
        }
    }

    private static bool BroadPhaseCanCollide(ObjectLayer layer1, BroadPhaseLayer layer2)
    {
        switch (layer1)
        {
            case Layers.NonMoving:
                return layer2 == BroadPhaseLayers.Moving;
            case Layers.Moving:
                return true;
            default:
                Debug.Assert(false);
                return false;
        }
    }

    private static bool ObjectCanCollide(ObjectLayer layer1, ObjectLayer layer2)
    {
        switch (layer1)
        {
            case Layers.NonMoving:
                return layer2 == Layers.Moving;
            case Layers.Moving:
                return true;
            default:
                Debug.Assert(false);
                return false;
        }
    }

    private static float WorldScale = 1.0f;

    private static Body CreateFloor(in BodyInterface bodyInterface, float size = 200.0f)
    {
        float scale = WorldScale;

        Body floor = bodyInterface.CreateBody(new BodyCreationSettings(
            new BoxShapeSettings(scale * new Vector3(0.5f * size, 1.0f, 0.5f * size), 0.0f),
            scale * new Vector3(0.0f, -1.0f, 0.0f),
            Quaternion.Identity,
            MotionType.Static,
            Layers.NonMoving)
            );
        bodyInterface.AddBody(floor.ID, ActivationMode.DontActivate);
        return floor;
    }

    private static void StackTest(in BodyInterface bodyInterface)
    {
        // Floor
        CreateFloor(bodyInterface);

        Shape boxShape = new BoxShape(new Vector3(0.5f, 1.0f, 2.0f));

        // Dynamic body stack
        for (int i = 0; i < 10; ++i)
        {
            Quaternion rotation;
            if ((i & 1) != 0)
                rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f * MathF.PI);
            else
                rotation = Quaternion.Identity;
            Body stack = bodyInterface.CreateBody(new BodyCreationSettings(boxShape, new Vector3(10, 1.0f + i * 2.1f, 0), rotation, MotionType.Dynamic, Layers.Moving));
            bodyInterface.AddBody(stack.ID, ActivationMode.Activate);
        }
    }

    public static unsafe void Main()
    {
        if (!Foundation.Init())
            return;

        {
            // Malloc
            //using TempAllocator tempAllocator = new();
            using TempAllocator tempAllocator = new(10 * 1024 * 1024);
            using JobSystemThreadPool jobSystem = new(Foundation.MaxPhysicsJobs, Foundation.MaxPhysicsBarriers);
            using BPLayerInterfaceImpl broadPhaseLayer = new();
            using PhysicsSystem physicsSystem = new();
            physicsSystem.Init(MaxBodies, NumBodyMutexes, MaxBodyPairs, MaxContactConstraints,
                broadPhaseLayer,
                BroadPhaseCanCollide,
                ObjectCanCollide);

            // ContactListener
            physicsSystem.OnContactValidate += OnContactValidate;
            physicsSystem.OnContactAdded += OnContactAdded;
            physicsSystem.OnContactPersisted += OnContactPersisted;
            physicsSystem.OnContactRemoved += OnContactRemoved;
            // BodyActivationListener
            physicsSystem.OnBodyActivated += OnBodyActivated;
            physicsSystem.OnBodyDeactivated += OnBodyDeactivated;

            BodyInterface bodyInterface = physicsSystem.BodyInterface;

            // Next we can create a rigid body to serve as the floor, we make a large box
            // Create the settings for the collision volume (the shape). 
            // Note that for simple shapes (like boxes) you can also directly construct a BoxShape.
            BoxShapeSettings floorShapeSettings = new(new Vector3(100.0f, 1.0f, 100.0f));

            BodyCreationSettings floorSettings = new(floorShapeSettings, new Vector3(0.0f, -1.0f, 0.0f), Quaternion.Identity, MotionType.Static, Layers.NonMoving);

            // Create the actual rigid body
            Body floor = bodyInterface.CreateBody(floorSettings);

            // Add it to the world
            bodyInterface.AddBody(floor, ActivationMode.DontActivate);

            BodyCreationSettings spherSettings = new(new SphereShape(0.5f), new Vector3(0.0f, 2.0f, 0.0f), Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
            BodyID sphereID = bodyInterface.CreateAndAddBody(spherSettings, ActivationMode.Activate);

            // Now you can interact with the dynamic body, in this case we're going to give it a velocity.
            // (note that if we had used CreateBody then we could have set the velocity straight on the body before adding it to the physics system)
            bodyInterface.SetLinearVelocity(sphereID, new Vector3(0.0f, -5.0f, 0.0f));

            StackTest(bodyInterface);

            // We simulate the physics world in discrete time steps. 60 Hz is a good rate to update the physics system.
            const float deltaTime = 1.0f / 60.0f;

            // Optional step: Before starting the physics simulation you can optimize the broad phase. This improves collision detection performance (it's pointless here because we only have 2 bodies).
            // You should definitely not call this every frame or when e.g. streaming in a new level section as it is an expensive operation.
            // Instead insert all new objects in batches instead of 1 at a time to keep the broad phase efficient.
            physicsSystem.OptimizeBroadPhase();

            uint step = 0;
            while (bodyInterface.IsActive(sphereID))
            {
                // Next step
                ++step;

                // Output current position and velocity of the sphere
                Vector3 position = bodyInterface.GetCenterOfMassPosition(sphereID);
                Vector3 velocity = bodyInterface.GetLinearVelocity(sphereID);
                Console.WriteLine($"Step {step} : Position = ({position}), Velocity = ({velocity})");

                // If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
                const int collisionSteps = 1;

                // If you want more accurate step results you can do multiple sub steps within a collision step. Usually you would set this to 1.
                const int integrationSubSteps = 1;

                // Step the world
                physicsSystem.Update(deltaTime, collisionSteps, integrationSubSteps, tempAllocator, jobSystem);
            }

            // Remove the sphere from the physics system. Note that the sphere itself keeps all of its state and can be re-added at any time.
            bodyInterface.RemoveBody(sphereID);

            // Destroy the sphere. After this the sphere ID is no longer valid.
            bodyInterface.DestroyBody(sphereID);

            // Remove and destroy the floor
            bodyInterface.RemoveBody(floor.ID);
            bodyInterface.DestroyBody(floor.ID);
        }

        Foundation.Shutdown();
    }


    private static ValidateResult OnContactValidate(PhysicsSystem system, in Body body1, in Body body2, Vector3 baseOffset, IntPtr collisionResult)
    {
        Console.WriteLine("Contact validate callback");

        // Allows you to ignore a contact before it is created (using layers to not make objects collide is cheaper!)
        return ValidateResult.AcceptAllContactsForThisBodyPair;
    }

    private static void OnContactAdded(PhysicsSystem system, in Body body1, in Body body2)
    {
        Console.WriteLine("A contact was added");
    }

    private static void OnContactPersisted(PhysicsSystem system, in Body body1, in Body body2)
    {
        Console.WriteLine("A contact was persisted");
    }

    private static void OnContactRemoved(PhysicsSystem system, ref SubShapeIDPair subShapePair)
    {
        Console.WriteLine("A contact was removed");
    }

    private static void OnBodyActivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        Console.WriteLine("A body got activated");
    }

    private static void OnBodyDeactivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        Console.WriteLine("A body went to sleep");
    }
}
