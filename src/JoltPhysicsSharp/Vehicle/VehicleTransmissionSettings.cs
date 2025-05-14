// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleTransmissionSettings : NativeObject
{
    public VehicleTransmissionSettings(
        TransmissionMode mode,
        //Array<float> gearRatios,		    // NOTE: BGE: just using default values for now.
        //Array<float> reverseGearRatios,   // NOTE: BGE: just using default values for now.
        float switchTime,
        float clutchReleaseTime,
        float switchLatency,
        float shiftUpRPM,
        float shiftDownRPM,
        float clutchStrength)
        : this(
            JPH_VehicleTransmissionSettings_Create(
                mode,
                //gearRatios,		    // NOTE: BGE: just using default values for now.
                //reverseGearRatios,   // NOTE: BGE: just using default values for now.
                switchTime,
                clutchReleaseTime,
                switchLatency,
                shiftUpRPM,
                shiftDownRPM,
                clutchStrength))
    {
    }

    protected VehicleTransmissionSettings(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleTransmissionSettings_Destroy(Handle);
    }
}
