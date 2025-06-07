// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleCollisionTesterCastCylinder : VehicleCollisionTester
{
    public VehicleCollisionTesterCastCylinder(ObjectLayer layer, float convexRadiusFraction)
        : this(JPH_VehicleCollisionTesterCastCylinder_Create(layer, convexRadiusFraction))
    {
    }

    protected VehicleCollisionTesterCastCylinder(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleCollisionTesterCastCylinder_Destroy(Handle);
    }
}
