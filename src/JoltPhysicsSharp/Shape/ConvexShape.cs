// Copyright © Amer Koleci and Contributors.
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
}

public abstract class ConvexShape : Shape
{
    internal ConvexShape(nint handle)
        : base(handle)
    {
    }
}
