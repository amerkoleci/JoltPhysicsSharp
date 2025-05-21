// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleConstraintSettings : NativeObject
{
    protected static unsafe nint Ctor(
        Vector3 up,
        Vector3 forward,
        float maxPitchRollAngle,
        Span<WheelSettingsWV> wheels,       // NOTE: BGE: just using an overly-specific type for now.
        //VehicleAntiRollBars antiRollBars, // NOTE: BGE: just using default values for now.
        VehicleControllerSettings settings)
    {
        int count = wheels.Length;
        nint* wheelsArray = stackalloc nint[count];
        for (int i = 0; i < count; i++) wheelsArray[i] = wheels[i].Handle;
        return JPH_VehicleConstraintSettings_Create(
            up,
            forward,
            maxPitchRollAngle,
            wheelsArray,
            wheels.Length,
            //antiRollBars, // NOTE: BGE: just using default values for now.
            settings.Handle);
    }

    public VehicleConstraintSettings(
        Vector3 up,
        Vector3 forward,
        float maxPitchRollAngle,
        Span<WheelSettingsWV> wheels,       // NOTE: BGE: just using an overly-specific type for now.
        //VehicleAntiRollBars antiRollBars, // NOTE: BGE: just using default values for now.
        VehicleControllerSettings settings)
        : this(
            Ctor(
                up,
                forward,
                maxPitchRollAngle,
                wheels,
                //antiRollBars, // NOTE: BGE: just using default values for now.
                settings))
    {
        foreach (var wheel in wheels) OwnedObjects.TryAdd(wheel.Handle, wheel);
        OwnedObjects.TryAdd(settings.Handle, settings);
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
