// Copyright (c) Amer Koleci and Contributors.
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

    public override Shape Create() => new BoxShape(this);
}

public sealed class BoxShape : ConvexShape
{
    public BoxShape(in Vector3 halfExent, float convexRadius = Foundation.DefaultConvexRadius)
        : base(JPH_BoxShape_Create(halfExent, convexRadius))
    {
    }

    public BoxShape(BoxShapeSettings settings)
        : base(JPH_BoxShapeSettings_CreateShape(settings.Handle))
    {
    }

    public Vector3 HalfExtent
    {
        get
        {
            JPH_BoxShape_GetHalfExtent(Handle, out Vector3 value);
            return value;
        }
    }

    public void GetHalfExtent(out Vector3 halfExtent) => JPH_BoxShape_GetHalfExtent(Handle, out halfExtent);

    public float ConvexRadius => JPH_BoxShape_GetConvexRadius(Handle);
}
