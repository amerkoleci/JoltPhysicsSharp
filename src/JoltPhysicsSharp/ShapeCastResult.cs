// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;

namespace JoltPhysicsSharp;

public readonly struct ShapeCastResult
{
    public readonly Vector3 ContactPointOn1;
    public readonly Vector3 ContactPointOn2;
    public readonly Vector3 PenetrationAxis;
    public readonly float PenetrationDepth;
    public readonly SubShapeID SubShapeID1;
    public readonly SubShapeID SubShapeID2;
    public readonly BodyID BodyID2;
    public readonly float Fraction;
    public readonly Bool8 IsBackFaceHit;
}
