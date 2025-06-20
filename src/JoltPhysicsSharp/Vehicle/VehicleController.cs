// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleController : NativeObject
{
    internal VehicleController(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public VehicleConstraint Constraint => VehicleConstraint.GetObject(JPH_VehicleController_GetConstraint(Handle))!;

    internal static T? GetObject<T>(nint handle) where T : VehicleController
    {
        return GetOrAddObject<T>(handle);
    }
}
