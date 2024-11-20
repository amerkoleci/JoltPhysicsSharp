// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class TaperedCapsuleShapeSettings : ConvexShapeSettings
{
    public TaperedCapsuleShapeSettings(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius)
        : base(JPH_TaperedCapsuleShapeSettings_Create(halfHeightOfTaperedCylinder, topRadius, bottomRadius))
    {
    }
    public override Shape Create() => new TaperedCapsuleShape(this);
}

public sealed class TaperedCapsuleShape : ConvexShape
{
    public TaperedCapsuleShape(TaperedCapsuleShapeSettings settings)
        : base(JPH_TaperedCapsuleShapeSettings_CreateShape(settings.Handle))
    {
    }

    public float TopRadius => JPH_TaperedCapsuleShape_GetTopRadius(Handle);
    public float BottomRadius => JPH_TaperedCapsuleShape_GetBottomRadius(Handle);
    public float HalfHeight => JPH_TaperedCapsuleShape_GetHalfHeight(Handle);
}
