// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class ScaledShapeSettings : DecoratedShapeSettings
{
    public ScaledShapeSettings(ShapeSettings shapeSettings, in Vector3 scale)
    {
        Handle = JPH_ScaledShapeSettings_Create(shapeSettings.Handle, in scale);
    }

    public ScaledShapeSettings(Shape shape, in Vector3 scale)
    {
        Handle = JPH_ScaledShapeSettings_Create2(shape.Handle, in scale);
    }

    public Vector3 Scale
    {
        get
        {
            JPH_ScaledShape_GetScale(Handle, out Vector3 result);
            return result;
        }
    }

    public override Shape Create() => new ScaledShape(this);
}

public  class ScaledShape : DecoratedShape
{
    public ScaledShape(Shape shape, in Vector3 scale)
        : base(JPH_ScaledShape_Create(shape.Handle, in scale))
    {
    }

    public ScaledShape(ScaledShapeSettings settings)
        : base(JPH_ScaledShapeSettings_CreateShape(settings.Handle))
    {
    }
}
