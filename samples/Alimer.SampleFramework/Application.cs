// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Alimer.SampleFramework;

public abstract class Application : DisposableObject
{
    private const int MaxBodies = 65536;
    private const int MaxBodyPairs = 65536;
    private const int MaxContactConstraints = 65536;
    private const int NumBodyMutexes = 0;

    protected static class Layers
    {
        public static readonly ObjectLayer NonMoving = 0;
        public static readonly ObjectLayer Moving = 1;
    }

    protected static class BroadPhaseLayers
    {
        public static readonly BroadPhaseLayer NonMoving = 0;
        public static readonly BroadPhaseLayer Moving = 1;
    }

    protected const int TargetFPS = 60;
    protected PhysicsSystemSettings _settings;
    protected readonly List<BodyID> _bodies = [];
    protected readonly HashSet<BodyID> _ignoreDrawBodies = [];

    protected Application(string title, int width = 1200, int height = 800)
    {
        if (!Foundation.Init(false))
        {
            return;
        }

        Foundation.SetTraceHandler((message) =>
        {
            Console.WriteLine(message);
        });

#if DEBUG
        Foundation.SetAssertFailureHandler((inExpression, inMessage, inFile, inLine) =>
        {
            string message = inMessage ?? inExpression;

            string outMessage = $"[JoltPhysics] Assertion failure at {inFile}:{inLine}: {message}";

            Debug.WriteLine(outMessage);

            throw new Exception(outMessage);
        });
#endif
        _settings = new PhysicsSystemSettings()
        {
            MaxBodies = MaxBodies,
            MaxBodyPairs = MaxBodyPairs,
            MaxContactConstraints = MaxContactConstraints,
            NumBodyMutexes = NumBodyMutexes,
        };

        JobSystem = new JobSystemThreadPool();
        SetupCollisionFiltering();
        PhysicsSystem = new(_settings);

        // ContactListener
        PhysicsSystem.OnContactValidate += OnContactValidate;
        PhysicsSystem.OnContactAdded += OnContactAdded;
        PhysicsSystem.OnContactPersisted += OnContactPersisted;
        PhysicsSystem.OnContactRemoved += OnContactRemoved;
        // BodyActivationListener
        PhysicsSystem.OnBodyActivated += OnBodyActivated;
        PhysicsSystem.OnBodyDeactivated += OnBodyDeactivated;

        // set a hint for anti-aliasing
        SetConfigFlags(ConfigFlags.Msaa4xHint);

        InitWindow(width, height, title);

        // 60 fps target
        SetTargetFPS(TargetFPS);

        // Create the main camera
        MainCamera = new()
        {
            Position = new Vector3(-20.0f, 8.0f, 10.0f),
            Target = new Vector3(0.0f, 4.0f, 0.0f),
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };

        // dynamically create a plane model
        Texture2D texture = GenCheckedTexture(10, 1, Color.LightGray, Color.Gray);
        Model planeModel = LoadModelFromMesh(GenMeshPlane(24, 24, 1, 1));
        SetMaterialTexture(ref planeModel, 0, MaterialMapIndex.Diffuse, ref texture);
        PlaneModel = planeModel;

        // dynamically create a box model
        var boxTexture = GenCheckedTexture(2, 1, Color.White, Color.Magenta);
        BoxMesh = GenMeshCube(1, 1, 1);
        Material boxMat = LoadMaterialDefault();
        SetMaterialTexture(ref boxMat, MaterialMapIndex.Diffuse, boxTexture);
        BoxMaterial = boxMat;
    }

    public JobSystem JobSystem { get; set; }
    public PhysicsSystem PhysicsSystem { get; private set; }
    public BodyInterface BodyInterface => PhysicsSystem.BodyInterface;
    public BodyLockInterface BodyLockInterface => PhysicsSystem.BodyLockInterface;

    public Camera3D MainCamera { get; }
    public Model PlaneModel { get; }
    public Mesh BoxMesh { get; }

    public Material BoxMaterial { get; }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (BodyID bodyID in _bodies)
            {
                BodyInterface.RemoveAndDestroyBody(bodyID);
            }
            _bodies.Clear();

            JobSystem.Dispose();
            PhysicsSystem.Dispose();

            Foundation.Shutdown();
        }
    }

    public void Run()
    {
        // Optional step: Before starting the physics simulation you can optimize the broad phase. This improves collision detection performance (it's pointless here because we only have 2 bodies).
        // You should definitely not call this every frame or when e.g. streaming in a new level section as it is an expensive operation.
        // Instead insert all new objects in batches instead of 1 at a time to keep the broad phase efficient.
        PhysicsSystem.OptimizeBroadPhase();

        // If you take larger steps than 1 / 60th of a second you need to do multiple collision steps in order to keep the simulation stable. Do 1 collision step per 1 / 60th of a second (round up).
        const int collisionSteps = 1;

        float deltaTime = 1.0f / TargetFPS;

        while (!WindowShouldClose())
        {
            // Step the world
            PhysicsUpdateError error = PhysicsSystem.Update(deltaTime, collisionSteps, JobSystem);
            Debug.Assert(error == PhysicsUpdateError.None);

            BeginDrawing();
            ClearBackground(Color.Blue);

            BeginMode3D(MainCamera);
            DrawModel(PlaneModel, Vector3.Zero, 1.0f, Color.White);

            foreach (BodyID bodyID in _bodies)
            {
                if (_ignoreDrawBodies.Contains(bodyID))
                    continue;

                //Vector3 pos = BodyInterface.GetPosition(bodyID);
                //Quaternion rot = BodyInterface.GetRotation(bodyID);
                //Matrix4x4 ori = Matrix4x4.CreateFromQuaternion(rot);
                //Matrix4x4 matrix = new(
                //    ori.M11, ori.M12, ori.M13, pos.X,
                //    ori.M21, ori.M22, ori.M23, pos.Y,
                //    ori.M31, ori.M32, ori.M33, pos.Z,
                //    0, 0, 0, 1.0f);

                // Raylib uses column major matrix
                Matrix4x4 worldTransform = BodyInterface.GetWorldTransform(bodyID);
                Matrix4x4 drawTransform = Matrix4x4.Transpose(worldTransform);
                DrawMesh(BoxMesh, BoxMaterial, drawTransform);
            }

            EndMode3D();

            DrawText($"{GetFPS()} fps", 10, 10, 20, Color.White);

            EndDrawing();
        }

        CloseWindow();
    }

    #region Raylib
    protected static Texture2D GenCheckedTexture(int size, int checks, Color colorA, Color colorB)
    {
        Image imageMag = GenImageChecked(size, size, checks, checks, colorA, colorB);
        Texture2D textureMag = LoadTextureFromImage(imageMag);
        UnloadImage(imageMag);
        return textureMag;
    }
    #endregion

    #region Physics
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

    protected BodyID CreateFloor(float size, ObjectLayer layer)
    {
        BoxShape shape = new(new Vector3(size, 5.0f, size));
        using BodyCreationSettings creationSettings = new(shape, new Vector3(0, -5.0f, 0.0f), Quaternion.Identity, MotionType.Static, layer);
        BodyID body = BodyInterface.CreateAndAddBody(creationSettings, Activation.DontActivate);
        _bodies.Add(body);
        _ignoreDrawBodies.Add(body);
        return body;
    }

    protected BodyID CreateBox(in Vector3 halfExtent,
        in Vector3 position,
        in Quaternion rotation,
        MotionType motionType,
        ObjectLayer layer,
        Activation activation = Activation.Activate)
    {
        BoxShape shape = new(halfExtent);
        using BodyCreationSettings creationSettings = new(shape, position, rotation, motionType, layer);
        BodyID body = BodyInterface.CreateAndAddBody(creationSettings, activation);
        _bodies.Add(body);
        return body;
    }

    protected BodyID CreateSphere(float radius,
        in Vector3 position,
        in Quaternion rotation,
        MotionType motionType,
        ObjectLayer layer,
        Activation activation = Activation.Activate)
    {
        SphereShape shape = new(radius);
        using BodyCreationSettings creationSettings = new(shape, position, rotation, motionType, layer);
        BodyID body = BodyInterface.CreateAndAddBody(creationSettings, activation);
        _bodies.Add(body);
        return body;
    }

    protected virtual ValidateResult OnContactValidate(PhysicsSystem system, in Body body1, in Body body2, Double3 baseOffset, in CollideShapeResult collisionResult)
    {
        TraceLog(TraceLogLevel.Debug, "Contact validate callback");

        // Allows you to ignore a contact before it is created (using layers to not make objects collide is cheaper!)
        return ValidateResult.AcceptAllContactsForThisBodyPair;
    }

    protected virtual void OnContactAdded(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, in ContactSettings settings)
    {
        TraceLog(TraceLogLevel.Debug, "A contact was added");
    }

    protected virtual void OnContactPersisted(PhysicsSystem system, in Body body1, in Body body2, in ContactManifold manifold, in ContactSettings settings)
    {
        TraceLog(TraceLogLevel.Debug, "A contact was persisted");
    }

    protected virtual void OnContactRemoved(PhysicsSystem system, ref SubShapeIDPair subShapePair)
    {
        TraceLog(TraceLogLevel.Debug, "A contact was removed");
    }

    protected virtual void OnBodyActivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        TraceLog(TraceLogLevel.Debug, "A body got activated");
    }

    protected virtual void OnBodyDeactivated(PhysicsSystem system, in BodyID bodyID, ulong bodyUserData)
    {
        TraceLog(TraceLogLevel.Debug, "A body went to sleep");
    } 
    #endregion
}
