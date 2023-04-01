// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class ConvexHullShapeSettings : ConvexShapeSettings
{
    public unsafe ConvexHullShapeSettings(Vector3* points, int pointsCount, float maxConvexRadius = Foundation.DefaultConvexRadius)
       : base(JPH_ConvexHullShapeSettings_Create(points, pointsCount, maxConvexRadius))
    {
    }

    public ConvexHullShapeSettings(Vector3[] points, float maxConvexRadius = Foundation.DefaultConvexRadius)
        : this(points.AsSpan(), maxConvexRadius)
    {
    }

    public unsafe ConvexHullShapeSettings(ReadOnlySpan<Vector3> points, float maxConvexRadius = Foundation.DefaultConvexRadius)
    {
        fixed (Vector3* pointsPtr = points)
        {
            Handle = JPH_ConvexHullShapeSettings_Create(pointsPtr, points.Length, maxConvexRadius);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ConvexHullShapeSettings" /> class.
    /// </summary>
    ~ConvexHullShapeSettings() => Dispose(isDisposing: false);
}
