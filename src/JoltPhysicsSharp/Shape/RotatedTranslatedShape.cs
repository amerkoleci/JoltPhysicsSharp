// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class RotatedTranslatedShapeSettings : DecoratedShapeSettings
{
    public RotatedTranslatedShapeSettings(in Vector3 position, in Quaternion rotation, ShapeSettings shapeSettings)
    {
        Handle = JPH_RotatedTranslatedShapeSettings_Create(in position, in rotation, shapeSettings.Handle);
    }

    public RotatedTranslatedShapeSettings(in Vector3 position, in Quaternion rotation, Shape shape)
    {
        Handle = JPH_RotatedTranslatedShapeSettings_Create2(in position, in rotation, shape.Handle);
    }

    public Vector3 Position
    {
        get
        {
            JPH_RotatedTranslatedShape_GetPosition(Handle, out Vector3 position);
            return position;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            JPH_RotatedTranslatedShape_GetRotation(Handle, out Quaternion rotation);
            return rotation;
        }
    }

    public override Shape Create() => new RotatedTranslatedShape(this);
}

public class RotatedTranslatedShape : DecoratedShape
{
    public RotatedTranslatedShape(in Vector3 position, in Quaternion rotation, Shape shape)
    {
        Handle = JPH_RotatedTranslatedShape_Create(in position, in rotation, shape.Handle);
    }

    public RotatedTranslatedShape(RotatedTranslatedShapeSettings settings)
        : base(JPH_RotatedTranslatedShapeSettings_CreateShape(settings.Handle))
    {
    }
}
