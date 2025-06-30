// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class VehicleConstraintSettings : ConstraintSettings
{
    public VehicleConstraintSettings()
    {
        JPH_VehicleConstraintSettings native;
        JPH_VehicleConstraintSettings_Init(&native);

        FromNative(native);
    }

    public Vector3 Up { get; set; }
    public Vector3 Forward { get; set; }
    public float MaxPitchRollAngle { get; set; }
    public WheelSettings[]? Wheels { get; set; }
    public VehicleAntiRollBar[]? AntiRollBars { get; set; }
    public VehicleControllerSettings? Controller { get; set; }

    private void FromNative(in JPH_VehicleConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Up = native.up;
        Forward = native.forward;
        MaxPitchRollAngle = native.maxPitchRollAngle;
        if (native.wheelsCount > 0)
        {
            Wheels = new WheelSettings[native.wheelsCount];
            for (uint i = 0; i < native.wheelsCount; i++)
            {
                Wheels[i] = WheelSettings.GetObject(native.wheels[i])!;
            }
        }

        if (native.antiRollBarsCount > 0)
        {
            AntiRollBars = new VehicleAntiRollBar[native.antiRollBarsCount];
            for (uint i = 0; i < native.antiRollBarsCount; i++)
            {
                AntiRollBars[i] = new VehicleAntiRollBar();
                AntiRollBars[i].FromNative(native.antiRollBars[i]);
            }
        }

        Controller = VehicleControllerSettings.GetObject(native.controller);
    }

    internal void ToNative(JPH_VehicleConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->up = Up;
        native->forward = Forward;
        native->maxPitchRollAngle = MaxPitchRollAngle;
        native->wheelsCount = Wheels != null ? Wheels.Length : 0;
        if (native->wheelsCount > 0)
        {
            native->wheels = (nint*)NativeMemory.Alloc((nuint)(sizeof(nint) * native->wheelsCount));
            for (uint i = 0; i < native->wheelsCount; i++)
            {
                native->wheels[i] = Wheels[i]!.Handle;
            }
        }
        else
        {
            native->wheels = null;
        }

        native->antiRollBarsCount = AntiRollBars != null ? AntiRollBars.Length : 0;
        if (native->antiRollBarsCount > 0)
        {
            native->antiRollBars = (JPH_VehicleAntiRollBar*)NativeMemory.Alloc((nuint)(sizeof(JPH_VehicleAntiRollBar) * native->antiRollBarsCount));
            for (uint i = 0; i < native->antiRollBarsCount; i++)
            {
                AntiRollBars[i]!.ToNative(&native->antiRollBars[i]);
            }
        }
        else
        {
            native->antiRollBars = null;
        }

        native->controller = Controller?.Handle ?? nint.Zero;
    }

    internal unsafe nint CreateConstraintNative(Body body)
    {
        JPH_VehicleConstraintSettings nativeSettings = default;
        try
        {
            ToNative(&nativeSettings);
            return JPH_VehicleConstraint_Create(body.Handle, &nativeSettings);
        }
        finally
        {
            if (nativeSettings.wheels != null)
            {
                NativeMemory.Free(nativeSettings.wheels);
            }
            if (nativeSettings.antiRollBars != null)
            {
                NativeMemory.Free(nativeSettings.antiRollBars);
            }
        }
    }
}
