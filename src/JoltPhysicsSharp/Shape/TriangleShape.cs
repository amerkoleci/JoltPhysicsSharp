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

    public override Shape Create() => new TriangleShape(this);
}

public sealed unsafe class TriangleShape : ConvexShape
{
    public TriangleShape(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius = 0.0f)
        : base(JPH_TriangleShape_Create(v1, v2, v3, convexRadius))
    {
    }

    public TriangleShape(TriangleShapeSettings settings)
        : base(JPH_TriangleShapeSettings_CreateShape(settings.Handle))
    {
    }

    public float ConvexRadius => JPH_TriangleShape_GetConvexRadius(Handle);

    public Vector3 Vertex1
    {
        get
        {
            Vector3 result;
            JPH_TriangleShape_GetVertex1(Handle, &result);
            return result;
        }
    }
    public Vector3 Vertex2
    {
        get
        {
            Vector3 result;
            JPH_TriangleShape_GetVertex2(Handle, &result);
            return result;
        }
    }

    public Vector3 Vertex3
    {
        get
        {
            Vector3 result;
            JPH_TriangleShape_GetVertex3(Handle, &result);
            return result;
        }
    }
}
