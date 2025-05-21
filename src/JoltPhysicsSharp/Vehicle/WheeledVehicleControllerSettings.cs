// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheeledVehicleControllerSettings : VehicleControllerSettings
{
    public WheeledVehicleControllerSettings(
	    VehicleEngineSettings engine,
	    VehicleTransmissionSettings transmission,
	    //Array<VehicleDifferentialSettings> differentials,	// NOTE: BGE: just using default values for now.
	    float differentialLimitedSlipRatio)
        : this(
            JPH_WheeledVehicleControllerSettings_Create(
                engine.Handle,
                transmission.Handle,
                //differentials, // NOTE: BGE: just using default values for now.
                differentialLimitedSlipRatio))
        {
            OwnedObjects.TryAdd(engine.Handle, engine);
            OwnedObjects.TryAdd(transmission.Handle, transmission);
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
