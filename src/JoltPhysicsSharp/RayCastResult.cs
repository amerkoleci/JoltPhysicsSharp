// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public struct RayCastResult
{
    /// <summary>
    /// C Float Epsilon.
    /// C# Float Epsilon has a different value than C Float Epsilon, which we need for default values
    /// </summary>
    const float CEpsilon = 1.192092896e-07F;

    public BodyID BodyID;
    public float Fraction;
    public uint/*SubShapeID*/ subShapeID2;

    /// <summary>
    /// Default values for raycasting.
    /// Required for raycasting successfully, as it expects these values to do it correctly.
    /// </summary>
    public static RayCastResult Default => new()
    {
        BodyID = BodyID.Invalid,
        Fraction = 1.0f + CEpsilon,
    };
}
