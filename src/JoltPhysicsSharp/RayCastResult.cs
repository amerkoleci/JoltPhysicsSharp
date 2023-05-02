// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly struct RayCastResult
{
    public readonly BodyID BodyID;
    public readonly float Fraction;
    public readonly uint/*SubShapeID*/ subShapeID2;
}
