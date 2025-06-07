// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleCollisionTesterRay : VehicleCollisionTester
{
    public VehicleCollisionTesterRay(ObjectLayer layer, Vector3 up, float maxSlopeAngle)
        : this(JPH_VehicleCollisionTesterRay_Create(layer, up, maxSlopeAngle))
    {
    }

    protected VehicleCollisionTesterRay(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleCollisionTesterRay_Destroy(Handle);
    }
}
