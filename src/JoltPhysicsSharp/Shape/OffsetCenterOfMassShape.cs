// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class OffsetCenterOfMassShapeSettings : DecoratedShapeSettings
{
    public OffsetCenterOfMassShapeSettings(in Vector3 offset, ShapeSettings shapeSettings)
        : base(JPH_OffsetCenterOfMassShapeSettings_Create(in offset, shapeSettings.Handle))
    {
        GC.KeepAlive(shapeSettings);
    }

    public OffsetCenterOfMassShapeSettings(in Vector3 offset, Shape shape)
        : base(JPH_OffsetCenterOfMassShapeSettings_Create2(in offset, shape.Handle))
    {
        GC.KeepAlive(shape);
    }

    public override Shape Create() => new OffsetCenterOfMassShape(this);
}

public sealed class OffsetCenterOfMassShape : ConvexShape
{
    public OffsetCenterOfMassShape(in Vector3 offset, Shape shape)
        : base(JPH_OffsetCenterOfMassShape_Create(offset, shape.Handle))
    {
        GC.KeepAlive(shape);
    }

    public OffsetCenterOfMassShape(OffsetCenterOfMassShapeSettings settings)
        : base(JPH_OffsetCenterOfMassShapeSettings_CreateShape(settings.Handle))
    {
        GC.KeepAlive(settings);
    }

    public Vector3 Offset
    {
        get
        {
            JPH_OffsetCenterOfMassShape_GetOffset(Handle, out Vector3 value);
            return value;
        }
    }
}
