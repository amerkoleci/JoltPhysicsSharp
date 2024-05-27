// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public abstract class DecoratedShapeSettings : ShapeSettings
{
    protected DecoratedShapeSettings()
    {
    }

    internal DecoratedShapeSettings(nint handle)
        : base(handle)
    {
    }
}

public abstract class DecoratedShape : Shape
{
    protected DecoratedShape()
    {
    }

    internal DecoratedShape(nint handle)
        : base(handle)
    {
    }
}
