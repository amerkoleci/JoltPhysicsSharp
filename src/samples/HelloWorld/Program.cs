// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;

namespace HelloWorld;

public static class Program
{
    private const int MaxBodies = 65536;
    private const int MaxBodyPairs = 65536;
    private const int MaxContactConstraints = 65536;
    private const int NumBodyMutexes = 0;

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

    private const int NumLayers = 2;

    private static void StackTest(in BodyInterface bodyInterface)
    {
        // Floor
        //CreateFloor(bodyInterface);

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

        {
            using Sample sample = new HelloWorld();
            //using Sample sample = new AlternativeCollissionFilteringSample();
            sample.Initialize();
            sample.Run();

            // Create capsule
            float mHeightStanding = 1.35f;
            float mRadiusStanding = 0.3f;

            CapsuleShape capsule = new(0.5f * mHeightStanding, mRadiusStanding);

            CharacterVirtualSettings characterSettings = new();
            characterSettings.Shape = new RotatedTranslatedShapeSettings(new Vector3(0, 0.5f * mHeightStanding + mRadiusStanding, 0), Quaternion.Identity, capsule).Create();

            // Configure supporting volume
            characterSettings.SupportingVolume = new Plane(Vector3.UnitY, -mHeightStanding); // Accept contacts that touch the lower sphere of the capsule

            //CharacterVirtual character = new(characterSettings, new Vector3(0, 0, 0), Quaternion.Identity, 0, physicsSystem);
        }

        Foundation.Shutdown();
    }
}
