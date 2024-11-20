// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class TaperedCylinderShapeSettings : ConvexShapeSettings
{
    public unsafe TaperedCylinderShapeSettings(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius, float convexRadius = Foundation.DefaultConvexRadius, PhysicsMaterial? material = default)
        : base(JPH_TaperedCylinderShapeSettings_Create(halfHeightOfTaperedCylinder, topRadius, bottomRadius, convexRadius, material != null ? material.Handle : IntPtr.Zero))
    {
    }

    public override Shape Create() => new TaperedCylinderShape(this);
}


public sealed class TaperedCylinderShape : ConvexShape
{
    public TaperedCylinderShape(TaperedCylinderShapeSettings settings)
        : base(JPH_TaperedCylinderShapeSettings_CreateShape(settings.Handle))
    {
    }

    public float TopRadius => JPH_TaperedCylinderShape_GetTopRadius(Handle);
    public float BottomRadius => JPH_TaperedCylinderShape_GetBottomRadius(Handle);
    public float ConvexRadius => JPH_TaperedCylinderShape_GetConvexRadius(Handle);
    public float HalfHeight => JPH_TaperedCylinderShape_GetHalfHeight(Handle);
}
