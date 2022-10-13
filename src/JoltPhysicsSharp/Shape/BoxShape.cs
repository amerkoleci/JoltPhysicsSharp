// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class BoxShapeSettings : ConvexShapeSettings
{
    public BoxShapeSettings(in Vector3 halfExent, float convexRadius = Foundation.DefaultConvexRadius)
        : base(JPH_BoxShapeSettings_Create(halfExent, convexRadius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BoxShapeSettings" /> class.
    /// </summary>
    ~BoxShapeSettings() => Dispose(isDisposing: false);
}

public sealed class BoxShape : ConvexShape
{
    public BoxShape(in Vector3 halfExent, float convexRadius = Foundation.DefaultConvexRadius)
        : base(JPH_BoxShape_Create(halfExent, convexRadius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BoxShape" /> class.
    /// </summary>
    ~BoxShape() => Dispose(isDisposing: false);
}
