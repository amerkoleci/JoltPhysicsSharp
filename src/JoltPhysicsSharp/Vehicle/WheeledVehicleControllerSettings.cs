// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheeledVehicleControllerSettings : NativeObject
{
    public WheeledVehicleControllerSettings(
	    VehicleEngineSettings engine,
	    VehicleTransmissionSettings transmission,
	    //Array<VehicleDifferentialSettings> differentials,	// NOTE: BGE: just using default values for now.
	    float differentialLimitedSlipRatio)
        : base(
            JPH_WheeledVehicleControllerSettings_Create(
                engine.Handle,
                transmission.Handle,
                //differentials, // NOTE: BGE: just using default values for now.
                differentialLimitedSlipRatio))
        {
        }

    protected WheeledVehicleControllerSettings(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_WheeledVehicleControllerSettings_Destroy(Handle);
    }
}
