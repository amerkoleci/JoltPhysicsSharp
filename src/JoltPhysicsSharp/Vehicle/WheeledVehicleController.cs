// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheeledVehicleController : NativeObject
{
    public WheeledVehicleController(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public void SetForwardInput(float forward)
    {
        JPH_WheeledVehicleController_SetForwardInput(Handle, forward);
    }

    public void SetRightInput(float right)
    {
        JPH_WheeledVehicleController_SetRightInput(Handle, right);
    }

    public void SetBrakeInput(float brake)
    {
        JPH_WheeledVehicleController_SetBrakeInput(Handle, brake);
    }

    public void SetHandBrakeInput(float handBrake)
    {
        JPH_WheeledVehicleController_SetHandBrakeInput(Handle, handBrake);
    }
}
