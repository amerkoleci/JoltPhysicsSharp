// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleCollisionTesterCastSphere : VehicleCollisionTester
{
    public VehicleCollisionTesterCastSphere(ObjectLayer layer, float radius, in Vector3 up, float maxSlopeAngle)
        : base(JPH_VehicleCollisionTesterCastSphere_Create(layer, radius, up, maxSlopeAngle), true)
    {
    }

    public VehicleCollisionTesterCastSphere(ObjectLayer layer, float radius, in Vector3 up)
        : this(layer, radius, up, MathUtil.DegreesToRadians(80.0f))
    {
    }

    public VehicleCollisionTesterCastSphere(ObjectLayer layer, float radius)
        : this(layer, radius, Vector3.UnitY, MathUtil.DegreesToRadians(80.0f))
    {
    }

    internal VehicleCollisionTesterCastSphere(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }
}
