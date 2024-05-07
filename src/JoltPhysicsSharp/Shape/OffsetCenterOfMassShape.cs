// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class OffsetCenterOfMassShapeSettings : DecoratedShapeSettings
{
    public OffsetCenterOfMassShapeSettings(in Vector3 offset, ShapeSettings shapeSettings)
        : base(JPH_OffsetCenterOfMassShapeSettings_Create(in offset, shapeSettings.Handle))
    {
    }

    public OffsetCenterOfMassShapeSettings(in Vector3 offset, Shape shape)
        : base(JPH_OffsetCenterOfMassShapeSettings_Create2(in offset, shape.Handle))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="OffsetCenterOfMassShapeSettings" /> class.
    /// </summary>
    ~OffsetCenterOfMassShapeSettings() => Dispose(disposing: false);

    //public override Shape Create()
    //{
    //    return new OffsetCenterOfMassShape(JPH_OffsetCenterOfMassShapeSettings_CreateShape(Handle));
    //}
}

public sealed class OffsetCenterOfMassShape : ConvexShape
{
    public OffsetCenterOfMassShape(in Vector3 offset, Shape shape)
        : base(JPH_OffsetCenterOfMassShape_Create(offset, shape.Handle))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="OffsetCenterOfMassShape" /> class.
    /// </summary>
    ~OffsetCenterOfMassShape() => Dispose(isDisposing: false);

    public Vector3 Offset
    {
        get
        {
            JPH_OffsetCenterOfMassShape_GetOffset(Handle, out Vector3 value);
            return value;
        }
    }
}
