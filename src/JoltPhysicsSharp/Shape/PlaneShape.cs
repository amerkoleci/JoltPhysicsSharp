// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class PlaneShapeSettings : ShapeSettings
{
    public const float DefaultHalfExtent = 1000.0f;

    public PlaneShapeSettings(in Plane plane, PhysicsMaterial? material = default, float halfExtent = DefaultHalfExtent)
        : base(JPH_PlaneShapeSettings_Create(in plane, material != null ? material.Handle : IntPtr.Zero, halfExtent))
    {
    }

    internal PlaneShapeSettings(nint handle)
        : base(handle)
    {
    }

    public override Shape Create() => new PlaneShape(this);
}

public sealed class PlaneShape : Shape
{
    internal PlaneShape(nint handle)
        : base(handle)
    {
    }

    public PlaneShape(in Plane plane, PhysicsMaterial? material = default, float halfExtent = PlaneShapeSettings.DefaultHalfExtent)
        : base(JPH_PlaneShape_Create(in plane, material != null ? material.Handle : IntPtr.Zero, halfExtent))
    {
    }

    public PlaneShape(PlaneShapeSettings settings)
        : base(JPH_PlaneShapeSettings_CreateShape(settings.Handle))
    {
    }

    public Plane Plane
    {
        get
        {
            JPH_PlaneShape_GetPlane(Handle, out Plane result);
            return result;
        }
    }

    public float HalfExtent
    {
        get => JPH_PlaneShape_GetHalfExtent(Handle);
    }
}
