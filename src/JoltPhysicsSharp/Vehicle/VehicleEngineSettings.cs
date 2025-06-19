// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public struct VehicleEngineSettings
{
    public float MaxTorque { get; set; }
    public float MinRPM { get; set; }
    public float MaxRPM { get; set; }
    //public LinearCurve			normalizedTorque;
    public float Inertia { get; set; }
    public float AngularDamping { get; set; }

    public unsafe VehicleEngineSettings()
    {
        JPH_VehicleEngineSettings native;
        JPH_VehicleEngineSettings_Init(&native);

        FromNative(native);
    }

    internal void FromNative(in JPH_VehicleEngineSettings native)
    {
        MaxTorque = native.maxTorque;
        MinRPM = native.minRPM;
        MaxRPM = native.maxRPM;
        Inertia = native.inertia;
        AngularDamping = native.angularDamping;
    }

    internal unsafe void ToNative(JPH_VehicleEngineSettings* native)
    {
        native->maxTorque = MaxTorque;
        native->minRPM = MinRPM;
        native->maxRPM = MaxRPM;
        native->inertia = Inertia;
        native->angularDamping = AngularDamping;
    }
}
