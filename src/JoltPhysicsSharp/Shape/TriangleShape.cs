// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class TriangleShapeSettings : ConvexShapeSettings
{
    public TriangleShapeSettings(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius = 0.0f)
        : base(JPH_TriangleShapeSettings_Create(v1, v2, v3, convexRadius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TriangleShapeSettings" /> class.
    /// </summary>
    ~TriangleShapeSettings() => Dispose(disposing: false);

    public override Shape Create() => new TriangleShape(this);
}

public sealed class TriangleShape : ConvexShape
{
    public TriangleShape(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius = 0.0f)
        : base(JPH_TriangleShape_Create(v1, v2, v3, convexRadius))
    {
    }

    public TriangleShape(TriangleShapeSettings settings)
        : base(JPH_TriangleShapeSettings_CreateShape(settings.Handle))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TriangleShape" /> class.
    /// </summary>
    ~TriangleShape() => Dispose(disposing: false);
}
