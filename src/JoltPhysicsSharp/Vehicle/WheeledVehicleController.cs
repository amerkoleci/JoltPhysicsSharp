// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheeledVehicleController : NativeObject
{
    public WheeledVehicleController(Body body, WheeledVehicleControllerSettings controllerSettings, VehicleConstraintSettings constraintSettings)
        : base(JPH_WheeledVehicleController_Create(body.Handle, controllerSettings.Handle, constraintSettings.Handle))
    {
    }

    protected WheeledVehicleController(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_WheeledVehicleController_Destroy(Handle);
    }

    public void SetDriverInput(float forward, float right, float brake, float handBrake)
    {
        JPH_WheeledVehicleController_SetDriverInput(Handle, forward, right, brake, handBrake);
    }
}
