// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class RotatedTranslatedShapeSettings : DecoratedShapeSettings
{
    public RotatedTranslatedShapeSettings(in Vector3 position, in Quaternion rotation, ShapeSettings shapeSettings)
    {
        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_RotatedTranslatedShapeSettings_Create(positionPtr, rotationPtr, shapeSettings.Handle);
        }
    }

    public RotatedTranslatedShapeSettings(in Vector3 position, in Quaternion rotation, Shape shape)
    {
        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_RotatedTranslatedShapeSettings_Create2(positionPtr, rotationPtr, shape.Handle);
        }
    }

    public Vector3 Position
    {
        get
        {
            Vector3 position;
            JPH_RotatedTranslatedShape_GetPosition(Handle, &position);
            return position;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            Quaternion rotation;
            JPH_RotatedTranslatedShape_GetRotation(Handle, &rotation);
            return rotation;
        }
    }

    public override Shape Create() => new RotatedTranslatedShape(this);
}

public unsafe class RotatedTranslatedShape : DecoratedShape
{
    public RotatedTranslatedShape(in Vector3 position, in Quaternion rotation, Shape shape)
    {
        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_RotatedTranslatedShape_Create(positionPtr, rotationPtr, shape.Handle);
        }
    }

    public RotatedTranslatedShape(RotatedTranslatedShapeSettings settings)
        : base(JPH_RotatedTranslatedShapeSettings_CreateShape(settings.Handle))
    {
    }
}
