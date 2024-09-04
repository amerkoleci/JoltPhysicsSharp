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

    /// <summary>
    /// Finalizes an instance of the <see cref="TaperedCylinderShapeSettings" /> class.
    /// </summary>
    ~TaperedCylinderShapeSettings() => Dispose(disposing: false);

    public override Shape Create() => new TaperedCylinderShape(this);
}


public sealed class TaperedCylinderShape : ConvexShape
{
    public TaperedCylinderShape(TaperedCylinderShapeSettings settings)
        : base(JPH_TaperedCylinderShapeSettings_CreateShape(settings.Handle))
    {
    }
}
