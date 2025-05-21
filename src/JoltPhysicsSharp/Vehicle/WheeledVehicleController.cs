// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheeledVehicleController : NativeObject
{
    public WheeledVehicleController(Body body, WheeledVehicleControllerSettings controllerSettings, VehicleConstraintSettings constraintSettings)
        : this(JPH_WheeledVehicleController_Create(body.Handle, controllerSettings.Handle, constraintSettings.Handle))
    {
        OwnedObjects.TryAdd(controllerSettings.Handle, controllerSettings);
        OwnedObjects.TryAdd(constraintSettings.Handle, constraintSettings);
    }

    protected WheeledVehicleController(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_WheeledVehicleController_Destroy(Handle);
    }

    public VehicleConstraint Constraint
    {
        // NOTE: BGE: we exposed and used the ctor overload to specify that the given handle isn't owned in this new object.
        get { return new VehicleConstraint(JPH_WheeledVehicleController_GetConstraint(Handle), false); }
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
