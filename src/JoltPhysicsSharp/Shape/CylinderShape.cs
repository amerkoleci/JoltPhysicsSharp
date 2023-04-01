// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CylinderShapeSettings : ConvexShapeSettings
{
    public unsafe CylinderShapeSettings(float halfHeight, float radius, float convexRadius = Foundation.DefaultConvexRadius)
        : base(JPH_CylinderShapeSettings_Create(halfHeight, radius, convexRadius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="CylinderShapeSettings" /> class.
    /// </summary>
    ~CylinderShapeSettings() => Dispose(isDisposing: false);
}

//public sealed class TriangleShape : ConvexShape
//{
//    public unsafe TriangleShape(float radius)
//        : base(JPH_SphereShape_Create(radius))
//    {
//    }

//    /// <summary>
//    /// Finalizes an instance of the <see cref="TriangleShape" /> class.
//    /// </summary>
//    ~TriangleShape() => Dispose(isDisposing: false);
//}
