// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class CompoundShapeShapeSettings : ShapeSettings
{
    protected CompoundShapeShapeSettings(nint handle)
        : base(handle)
    {
    }

    public void AddShape(in Vector3 position, in Quaternion rotation, ShapeSettings shapeSettings, uint userData = 0u)
    {
        JPH_CompoundShapeSettings_AddShape(Handle, in position, in rotation, shapeSettings.Handle, userData);
    }

    public void AddShape(in Vector3 position, in Quaternion rotation, Shape shape, uint userData = 0u)
    {
        JPH_CompoundShapeSettings_AddShape2(Handle, in position, in rotation, shape.Handle, userData);
    }
}

public abstract class CompoundShape : Shape
{
    internal CompoundShape(nint handle)
        : base(handle)
    {
    }
}
