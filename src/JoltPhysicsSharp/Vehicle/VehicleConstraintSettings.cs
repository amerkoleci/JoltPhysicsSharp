// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleConstraintSettings : NativeObject
{
    public VehicleConstraintSettings(
        in Vector3 up,
        in Vector3 forward,
        float maxPitchRollAngle,
        //Array<Ref<WheelSettings>> wheels,         // NOTE: BGE: just using default values for now.
        //VehicleAntiRollBars antiRollBars,         // NOTE: BGE: just using default values for now.
        WheeledVehicleControllerSettings settings)  // NOTE: BGE: making this too specific of a pointer type for now.
        : base(
            JPH_VehicleConstraintSettings_Create(
                up,
                forward,
                maxPitchRollAngle,
                //wheels,         // NOTE: BGE: just using default values for now.
                //antiRollBars,   // NOTE: BGE: just using default values for now.
                settings.Handle))
    {
    }

    protected VehicleConstraintSettings(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleConstraintSettings_Destroy(Handle);
    }
}
