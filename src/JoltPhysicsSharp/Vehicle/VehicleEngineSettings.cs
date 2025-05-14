// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleEngineSettings : NativeObject
{
    public VehicleEngineSettings(
        float maxTorque,
        float minRPM,
        float maxRPM,
        //LinearCurve normalizedTorque, // NOTE: BGE: just using default values for now.
        float inertia,
        float angularDamping)
        : this(
            JPH_VehicleEngineSettings_Create(
                maxTorque,
                minRPM,
                maxRPM,
                //normalizedTorque, // NOTE: BGE: just using default values for now.
                inertia,
                angularDamping))
    {
    }

    protected VehicleEngineSettings(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleEngineSettings_Destroy(Handle);
    }
}
