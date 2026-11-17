// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;
namespace JoltPhysicsSharp;

public abstract class DecoratedShapeSettings : ShapeSettings
{
    internal DecoratedShapeSettings(nint handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }
}

public abstract class DecoratedShape : Shape
{
    internal DecoratedShape(nint handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }

    public nint InnerShape => JPH_DecoratedShape_GetInnerShape(Handle);
}
