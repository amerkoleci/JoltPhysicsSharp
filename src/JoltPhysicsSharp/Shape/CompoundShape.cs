// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract unsafe class CompoundShapeShapeSettings : ShapeSettings
{
    protected CompoundShapeShapeSettings(nint handle)
        : base(handle)
    {
    }

    public uint NumSubShapes => JPH_CompoundShape_GetNumSubShapes(Handle);

    public void AddShape(in Vector3 position, in Quaternion rotation, ShapeSettings shapeSettings, uint userData = 0u)
    {
        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            JPH_CompoundShapeSettings_AddShape(Handle, positionPtr, rotationPtr, shapeSettings.Handle, userData);
        }
    }

    public void AddShape(in Vector3 position, in Quaternion rotation, Shape shape, uint userData = 0u)
    {
        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            JPH_CompoundShapeSettings_AddShape2(Handle, positionPtr, rotationPtr, shape.Handle, userData);
        }
    }
}

public abstract class CompoundShape : Shape
{
    internal CompoundShape(nint handle)
        : base(handle)
    {
    }
}
