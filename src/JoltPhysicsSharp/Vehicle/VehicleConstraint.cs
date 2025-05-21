// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleConstraint : Constraint
{
    public VehicleConstraint(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public void SetVehicleCollisionTester(VehicleCollisionTester tester)
    {
        JPH_VehicleConstraint_SetVehicleCollisionTester(Handle, tester.Handle);
    }
}
