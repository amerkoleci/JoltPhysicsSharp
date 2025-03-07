// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ConvexShapeSettings : ShapeSettings
{
    protected ConvexShapeSettings()
    {
    }

    internal ConvexShapeSettings(nint handle)
        : base(handle)
    {
    }

    public float Density
    {
        get => JPH_ConvexShapeSettings_GetDensity(Handle);
        set => JPH_ConvexShapeSettings_SetDensity(Handle, value);
    }
}

public abstract class ConvexShape : Shape
{
    internal ConvexShape(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    public float Density
    {
        get => JPH_ConvexShape_GetDensity(Handle);
        set => JPH_ConvexShape_SetDensity(Handle, value);
    }
}
