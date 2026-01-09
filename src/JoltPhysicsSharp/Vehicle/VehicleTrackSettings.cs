// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public struct VehicleTrackSettings
{
    public uint DrivenWheel { get; set; }
    public uint[]? Wheels { get; set; }
    public float Inertia { get; set; }
    public float AngularDamping { get; set; }
    public float MaxBrakeTorque { get; set; }
    public float DifferentialRatio { get; set; }

    public unsafe VehicleTrackSettings()
    {
        JPH_VehicleTrackSettings native;
        JPH_VehicleTrackSettings_Init(&native);

        FromNative(native);
    }

    internal unsafe void FromNative(in JPH_VehicleTrackSettings native)
    {
        DrivenWheel = native.drivenWheel;
        Inertia = native.inertia;
        AngularDamping = native.angularDamping;
        MaxBrakeTorque = native.maxBrakeTorque;
        DifferentialRatio = native.differentialRatio;

        if (native.wheelsCount > 0 && native.wheels != null)
        {
            Wheels = new uint[native.wheelsCount];
            for (int i = 0; i < native.wheelsCount; i++)
            {
                Wheels[i] = native.wheels[i];
            }
        }
    }

    internal unsafe void ToNative(JPH_VehicleTrackSettings* native, uint* wheelsBuffer)
    {
        native->drivenWheel = DrivenWheel;
        native->inertia = Inertia;
        native->angularDamping = AngularDamping;
        native->maxBrakeTorque = MaxBrakeTorque;
        native->differentialRatio = DifferentialRatio;

        if (Wheels is { Length: > 0 })
        {
            for (int i = 0; i < Wheels.Length; i++)
            {
                wheelsBuffer[i] = Wheels[i];
            }
            native->wheels = wheelsBuffer;
            native->wheelsCount = (uint)Wheels.Length;
        }
        else
        {
            native->wheels = null;
            native->wheelsCount = 0;
        }
    }
}
