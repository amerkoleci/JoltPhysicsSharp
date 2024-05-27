// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
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
        public static readonly ObjectLayer NonMoving = 0;
        public static readonly ObjectLayer Moving = 1;
    };

    static class BroadPhaseLayers
    {
        public static readonly BroadPhaseLayer NonMoving = 0;
        public static readonly BroadPhaseLayer Moving = 1;
    };

    public const int NumLayers = 2;

    private static float WorldScale = 1.0f;

    private static Body CreateFloor(in BodyInterface bodyInterface, float size = 200.0f)
    {
        Body floor = bodyInterface.CreateBody(new BodyCreationSettings(
            new BoxShapeSettings(WorldScale * new Vector3(0.5f * size, 1.0f, 0.5f * size), 0.0f),
            WorldScale * new Vector3(0.0f, -1.0f, 0.0f),
            Quaternion.Identity,
            MotionType.Static,
            Layers.NonMoving)
            );
        bodyInterface.AddBody(floor.ID, Activation.DontActivate);
        return floor;
    }

    private static void StackTest(in BodyInterface bodyInterface)
    {
        // Floor
        CreateFloor(bodyInterface);

        ReadOnlySpan<Vector3> box = [
            new(5, 6, 7),
            new(5, 6, 14),
            new(5, 12, 7),
            new(5, 12, 14),
            new(10, 6, 7),
            new(10, 6, 14),
            new(10, 12, 7),
            new(10, 12, 14)
        ];

        const float cDensity = 1.5f;
        ConvexHullShapeSettings settings = new(box)
        {
            Density = cDensity
        };

        using ConvexHullShape shape = new(settings);
        Vector3 com = shape.CenterOfMass;
        CHECK_APPROX_EQUAL(new Vector3(7.5f, 9.0f, 10.5f), com, 1.0e-5f);

        // Calculate reference value of mass and inertia of a box
        MassProperties reference = default;
        reference.SetMassAndInertiaOfSolidBox(new Vector3(5, 6, 7), cDensity);

        // Mass is easy to calculate, double check if SetMassAndInertiaOfSolidBox calculated it correctly
        CHECK_APPROX_EQUAL(5.0f * 6.0f * 7.0f * cDensity, reference.Mass, 1.0e-6f);

        /// Get calculated inertia tensor
        MassProperties m = shape.MassProperties;
        CHECK_APPROX_EQUAL(reference.Mass, m.Mass, 1.0e-6f);
        //CHECK_APPROX_EQUAL(reference.Inertia, m.Inertia, 1.0e-4f);
        //
        // Check inner radius
        CHECK_APPROX_EQUAL(shape.InnerRadius, 2.5f);

        Shape boxShape = new BoxShape(new Vector3(0.5f, 1.0f, 2.0f));

        // Dynamic body stack
        for (int i = 0; i < 10; ++i)
        {
            Quaternion rotation;
            if ((i & 1) != 0)
                rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.5f * (float)Math.PI);
            else
                rotation = Quaternion.Identity;
            Body stack = bodyInterface.CreateBody(new BodyCreationSettings(boxShape, new Vector3(10, 1.0f + i * 2.1f, 0), rotation, MotionType.Dynamic, Layers.Moving));
            bodyInterface.AddBody(stack.ID, Activation.Activate);
        }
    }

    private static void CHECK_APPROX_EQUAL(float inLHS, float inRHS, float inTolerance = 1.0e-6f)
    {
        Debug.Assert(MathF.Abs(inRHS - inLHS) <= inTolerance);
    }

    private static void CHECK_APPROX_EQUAL(in Vector3 inLHS, in Vector3 inRHS, float inTolerance = 1.0e-6f)
    {
        Debug.Assert(IsClose(inLHS, inRHS, inTolerance * inTolerance));
    }

    private static bool IsClose(in Vector3 inV1, in Vector3 inV2, float inMaxDistSq = 1.0e-12f)
    {
        return (inV2 - inV1).LengthSquared() <= inMaxDistSq;
    }

    private static MeshShapeSettings CreateTorusMesh(float inTorusRadius, float inTubeRadius, int inTorusSegments = 16, int inTubeSegments = 16)
    {
        int cNumVertices = inTorusSegments * inTubeSegments;

        // Create torus
        int triangleIndex = 0;
        Span<Vector3> triangleVertices = stackalloc Vector3[cNumVertices];
        Span<IndexedTriangle> indexedTriangles = stackalloc IndexedTriangle[cNumVertices * 2];

        for (int torus_segment = 0; torus_segment < inTorusSegments; ++torus_segment)
        {
            Matrix4x4 rotation = Matrix4x4.CreateFromAxisAngle(Vector3.UnitY, (float)torus_segment * 2.0f * (float)Math.PI / inTorusSegments);
            for (int tube_segment = 0; tube_segment < inTubeSegments; ++tube_segment)
            {
                // Create vertices
                float tube_angle = (float)tube_segment * 2.0f * (float)Math.PI / inTubeSegments;
                Vector3 pos = Vector3.Transform(
                    new Vector3(inTorusRadius + inTubeRadius * (float)Math.Sin(tube_angle), inTubeRadius * (float)Math.Cos(tube_angle), 0),
                    rotation);
                triangleVertices[triangleIndex] = pos;

                // Create indices
                int start_idx = torus_segment * inTubeSegments + tube_segment;
                indexedTriangles[triangleIndex] = new(start_idx, (start_idx + 1) % cNumVertices, (start_idx + inTubeSegments) % cNumVertices);
                indexedTriangles[triangleIndex + 1] = new((start_idx + 1) % cNumVertices, (start_idx + inTubeSegments + 1) % cNumVertices, (start_idx + inTubeSegments) % cNumVertices);

                triangleIndex++;
            }
        }

        return new MeshShapeSettings(triangleVertices, indexedTriangles);
    }

    public static unsafe void Main()
    {
        if (!Foundation.Init(0u, false))
        {
            return;
        }

        {
            ObjectLayerPairFilter objectLayerPairFilter;
            BroadPhaseLayerInterface broadPhaseLayerInterface;
            ObjectVsBroadPhaseLayerFilter objectVsBroadPhaseLayerFilter;

            bool useTable = true;
            if (useTable)
            {
                // We use only 2 layers: one for non-moving objects and one for moving objects
                ObjectLayerPairFilterTable objectLayerPairFilterTable = new(2);
                objectLayerPairFilterTable.EnableCollision(Layers.NonMoving, Layers.Moving);
                objectLayerPairFilterTable.EnableCollision(Layers.Moving, Layers.Moving);

                // We use a 1-to-1 mapping between object layers and broadphase layers
                BroadPhaseLayerInterfaceTable broadPhaseLayerInterfaceTable = new(2, 2);
                broadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.NonMoving, BroadPhaseLayers.NonMoving);
                broadPhaseLayerInterfaceTable.MapObjectToBroadPhaseLayer(Layers.Moving, BroadPhaseLayers.Moving);

                objectLayerPairFilter = objectLayerPairFilterTable;
                broadPhaseLayerInterface = broadPhaseLayerInterfaceTable;
                objectVsBroadPhaseLayerFilter = new ObjectVsBroadPhaseLayerFilterTable(broadPhaseLayerInterfaceTable, 2, objectLayerPairFilterTable, 2);
            }
            else
            {
                objectLayerPairFilter = new ObjectLayerPairFilterMask();

                // Layer that objects can be in, determines which other objects it can collide with
                // Typically you at least want to have 1 layer for moving bodies and 1 layer for static bodies, but you can have more
                // layers if you want. E.g. you could have a layer for high detail collision (which is not used by the physics simulation
                // but only if you do collision testing).
                const uint NUM_BROAD_PHASE_LAYERS = 2;

                BroadPhaseLayerInterfaceMask bpInterface = new(NUM_BROAD_PHASE_LAYERS);
                //bpInterface.ConfigureLayer(BroadPhaseLayers.NonMoving, GROUP_STATIC, 0); // Anything that has the static bit set goes into the static broadphase layer
                //bpInterface.ConfigureLayer(BroadPhaseLayers.Moving, GROUP_FLOOR1 | GROUP_FLOOR2 | GROUP_FLOOR3, 0); // Anything that has one of the floor bits set goes into the dynamic broadphase layer

                broadPhaseLayerInterface = bpInterface;
                objectVsBroadPhaseLayerFilter = new ObjectVsBroadPhaseLayerFilterMask(bpInterface);
            }

            PhysicsSystemSettings settings = new()
            {
                ObjectLayerPairFilter = objectLayerPairFilter,
                BroadPhaseLayerInterface = broadPhaseLayerInterface,
                ObjectVsBroadPhaseLayerFilter = objectVsBroadPhaseLayerFilter
            };

            using PhysicsSystem physicsSystem = new(settings);

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
            bodyInterface.AddBody(floor, Activation.DontActivate);

            SphereShape sphereShape = new(0.5f);
            BodyCreationSettings spherSettings = new(sphereShape, new Vector3(0.0f, 2.0f, 0.0f), Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
            BodyID sphereID = bodyInterface.CreateAndAddBody(spherSettings, Activation.Activate);

            // Now you can interact with the dynamic body, in this case we're going to give it a velocity.
            // (note that if we had used CreateBody then we could have set the velocity straight on the body before adding it to the physics system)
            bodyInterface.SetLinearVelocity(sphereID, new Vector3(0.0f, -5.0f, 0.0f));

            //StackTest(bodyInterface);

            MeshShapeSettings meshShape = CreateTorusMesh(3.0f, 1.0f);
            BodyCreationSettings bodySettings = new(meshShape, new Vector3(0, 10, 0), Quaternion.Identity, MotionType.Dynamic, Layers.Moving);

            // Create capsule
            float mHeightStanding = 1.35f;
            float mRadiusStanding = 0.3f;

            CapsuleShape capsule = new(0.5f * mHeightStanding, mRadiusStanding);

            CharacterVirtualSettings characterSettings = new();
            characterSettings.Shape = new RotatedTranslatedShapeSettings(new Vector3(0, 0.5f * mHeightStanding + mRadiusStanding, 0), Quaternion.Identity, capsule).Create();

            // Configure supporting volume
            characterSettings.SupportingVolume = new Plane(Vector3.UnitY, -mHeightStanding); // Accept contacts that touch the lower sphere of the capsule


            CharacterVirtual character = new(characterSettings, new Vector3(0, 0, 0), Quaternion.Identity, 0, physicsSystem);

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
                //Matrix4x4 transform = bodyInterface.GetWorldTransform(sphereID);
                //Vector3 translation = bodyInterface.GetWorldTransform(sphereID).Translation;
                //Matrix4x4 centerOfMassTransform = bodyInterface.GetCenterOfMassTransform(sphereID);
                Console.WriteLine($"Step {step} : Position = ({position}), Velocity = ({velocity})");

                // If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
                const int collisionSteps = 1;

                // Step the world
                PhysicsUpdateError error = physicsSystem.Step(deltaTime, collisionSteps);
                Debug.Assert(error == PhysicsUpdateError.None);
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


    private static ValidateResult OnContactValidate(PhysicsSystem system, in Body body1, in Body body2, Double3 baseOffset, nint collisionResult)
    {
        Console.WriteLine("Contact validate callback");

        // Allows you to ignore a contact before it is created (using layers to not make objects collide is cheaper!)
        return ValidateResult.AcceptAllContactsForThisBodyPair;
    }

    private static void OnContactAdded(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, in ContactSettings settings)
    {
        Console.WriteLine("A contact was added");
    }

    private static void OnContactPersisted(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, in ContactSettings settings)
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
