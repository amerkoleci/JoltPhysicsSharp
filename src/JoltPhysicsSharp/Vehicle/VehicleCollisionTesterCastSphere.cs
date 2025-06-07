// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleCollisionTesterCastSphere : VehicleCollisionTester
{
    public VehicleCollisionTesterCastSphere(ObjectLayer layer, float radius, in Vector3 up, float maxSlopeAngle)
        : this(JPH_VehicleCollisionTesterCastSphere_Create(layer, radius, up, maxSlopeAngle))
    {
    }

    protected VehicleCollisionTesterCastSphere(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleCollisionTesterCastSphere_Destroy(Handle);
    }
}
