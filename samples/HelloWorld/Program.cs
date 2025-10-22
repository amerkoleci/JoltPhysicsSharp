// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;

namespace HelloWorld;

public static class Program
{
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

        if (!Foundation.Init(false))
        {
            return;
        }

        {
            //using Sample sample = new HelloWorld();
            using Sample sample = new AlternativeCollissionFilteringSample();
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
