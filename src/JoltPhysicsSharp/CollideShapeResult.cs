// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;

namespace JoltPhysicsSharp;

public readonly unsafe struct CollideShapeResult
{
    public readonly Vector3 ContactPointOn1;
    public readonly Vector3 ContactPointOn2;
    public readonly Vector3 PenetrationAxis;
    public readonly float PenetrationDepth;
    public readonly SubShapeID SubShapeID1;
    public readonly SubShapeID SubShapeID2;
    public readonly BodyID BodyID2;
    internal readonly int Shape1FaceCount;
    internal readonly Vector3* Shape1Faces;
    internal readonly int Shape2FaceCount;
    internal readonly Vector3* Shape2Faces;
    public Span<Vector3> Shape1Face => new(Shape1Faces, Shape1FaceCount);
    public Span<Vector3> Shape2Face => new(Shape2Faces, Shape2FaceCount);
}
