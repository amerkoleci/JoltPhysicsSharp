// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleCollisionTesterRay : VehicleCollisionTester
{
    public VehicleCollisionTesterRay(ObjectLayer layer, Vector3 up, float maxSlopeAngle)
        : base(JPH_VehicleCollisionTesterRay_Create(layer, up, maxSlopeAngle), true)
    {
    }

    public VehicleCollisionTesterRay(ObjectLayer layer, Vector3 up)
        : this(layer, up, MathUtil.DegreesToRadians(80.0f))
    {
    }

    public VehicleCollisionTesterRay(ObjectLayer layer)
        : this(layer, Vector3.UnitY, MathUtil.DegreesToRadians(80.0f))
    {
    }

    internal VehicleCollisionTesterRay(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }
}
