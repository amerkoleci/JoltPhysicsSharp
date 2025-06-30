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

        TestPhysicsStepListener testListener = new();
        PhysicsSystem.AddStepListener(testListener);

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

    class TestPhysicsStepListener : PhysicsStepListener
    {
        protected override void OnStep(in PhysicsStepListenerContext context)
        {
            TraceLog(TraceLogLevel.Debug, $"Test step listener: {context.DeltaTime}");
        }
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

    public struct VehicleSettings
    {
        public Vector3 Position = new Vector3(0, 2, 0);
        public bool UseCastSphere = true;
        public float WheelRadius = 0.3f;
        public float WheelWidth = 0.1f;
        public float HalfVehicleLength = 2.0f;
        public float HalfVehicleWidth = 0.9f;
        public float HalfVehicleHeight = 0.2f;
        public float WheelOffsetHorizontal = 1.4f;
        public float WheelOffsetVertical = 0.18f;
        public float SuspensionMinLength = 0.3f;
        public float SuspensionMaxLength = 0.5f;
        public float MaxSteeringAngle = MathUtil.DegreesToRadians(30);
        public bool FourWheelDrive = false;
        public float FrontBackLimitedSlipRatio = 1.4f;
        public float LeftRightLimitedSlipRatio = 1.4f;
        public bool AntiRollbar = true;

        public VehicleSettings()
        {

        }
    }

    protected VehicleConstraint AddVehicle(in VehicleSettings settings)
    {
        const int FL_WHEEL = 0;
        const int FR_WHEEL = 1;
        const int BL_WHEEL = 2;
        const int BR_WHEEL = 3;

        // Create vehicle body
        Shape car_shape = new OffsetCenterOfMassShapeSettings(new Vector3(0, -settings.HalfVehicleHeight, 0), new BoxShape(new Vector3(settings.HalfVehicleWidth, settings.HalfVehicleHeight, settings.HalfVehicleLength))).Create();
        using BodyCreationSettings car_body_settings = new(car_shape, settings.Position, Quaternion.Identity, MotionType.Dynamic, Layers.Moving);
        car_body_settings.OverrideMassProperties = OverrideMassProperties.CalculateInertia;
        var massProperties = car_body_settings.MassPropertiesOverride;
        massProperties.Mass = 1500.0f;
        car_body_settings.MassPropertiesOverride = massProperties;
        Body car_body = BodyInterface.CreateBody(car_body_settings);
        BodyInterface.AddBody(car_body, Activation.Activate);

        // Create vehicle constraint
        VehicleConstraintSettings vehicle = new()
        {
            DrawConstraintSize = 0.1f,
            MaxPitchRollAngle = MathUtil.DegreesToRadians(60.0f)
        };

        // Wheels
        WheelSettingsWV fl = new()
        {
            Position = new Vector3(settings.HalfVehicleWidth, -settings.WheelOffsetVertical, settings.WheelOffsetHorizontal),
            MaxSteerAngle = settings.MaxSteeringAngle,
            MaxHandBrakeTorque = 0.0f // Front wheel doesn't have hand brake
        };


        WheelSettingsWV fr = new()
        {
            Position = new Vector3(-settings.HalfVehicleWidth, -settings.WheelOffsetVertical, settings.WheelOffsetHorizontal),
            MaxSteerAngle = settings.MaxSteeringAngle,
            MaxHandBrakeTorque = 0.0f // Front wheel doesn't have hand brake
        };

        WheelSettingsWV bl = new();
        bl.Position = new Vector3(settings.HalfVehicleWidth, -settings.WheelOffsetVertical, -settings.WheelOffsetHorizontal);
        bl.MaxSteerAngle = 0.0f;

        WheelSettingsWV br = new()
        {
            Position = new Vector3(-settings.HalfVehicleWidth, -settings.WheelOffsetVertical, -settings.WheelOffsetHorizontal),
            MaxSteerAngle = 0.0f
        };

        vehicle.Wheels = new WheelSettings[4];
        vehicle.Wheels[FL_WHEEL] = fl;
        vehicle.Wheels[FR_WHEEL] = fr;
        vehicle.Wheels[BL_WHEEL] = bl;
        vehicle.Wheels[BR_WHEEL] = br;

        foreach (WheelSettings w in vehicle.Wheels)
        {
            w.Radius = settings.WheelRadius;
            w.Width = settings.WheelWidth;
            w.SuspensionMinLength = settings.SuspensionMinLength;
            w.SuspensionMaxLength = settings.SuspensionMaxLength;
        }

        WheeledVehicleControllerSettings controller = new();
        vehicle.Controller = controller;

        // Differential
        controller.DifferentialsCount = settings.FourWheelDrive ? 2 : 1;

        controller.SetDifferential(0, new VehicleDifferentialSettings()
        {
            LeftWheel = FL_WHEEL,
            RightWheel = FR_WHEEL,
            LimitedSlipRatio = settings.LeftRightLimitedSlipRatio,
            EngineTorqueRatio = settings.FourWheelDrive ? 0.5f : 1.0f
        });

        controller.DifferentialLimitedSlipRatio = settings.FrontBackLimitedSlipRatio;
        if (settings.FourWheelDrive)
        {
            controller.SetDifferential(1, new VehicleDifferentialSettings()
            {
                LeftWheel = BL_WHEEL,
                RightWheel = BR_WHEEL,
                LimitedSlipRatio = settings.LeftRightLimitedSlipRatio,
                EngineTorqueRatio = 0.5f
            });
        }

        // Anti rollbars
        if (settings.AntiRollbar)
        {
            vehicle.AntiRollBars = new VehicleAntiRollBar[2];
            vehicle.AntiRollBars[0].LeftWheel = FL_WHEEL;
            vehicle.AntiRollBars[0].RightWheel = FR_WHEEL;
            vehicle.AntiRollBars[1].LeftWheel = BL_WHEEL;
            vehicle.AntiRollBars[1].RightWheel = BR_WHEEL;
        }

        // Create the constraint
        VehicleConstraint constraint = new(car_body, vehicle);

        // Create collision tester
        VehicleCollisionTester tester;
        if (settings.UseCastSphere)
            tester = new VehicleCollisionTesterCastSphere(Layers.Moving, 0.5f * settings.WheelWidth);
        else
            tester = new VehicleCollisionTesterRay(Layers.Moving);
        constraint.SetVehicleCollisionTester(tester);

        // Add to the world
        PhysicsSystem.AddConstraint(constraint);
        PhysicsSystem.AddStepListener(constraint);

        return constraint;
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
