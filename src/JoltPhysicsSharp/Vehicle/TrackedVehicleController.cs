// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheelSettingsTV : WheelSettings
{
    public WheelSettingsTV()
        : base(JPH_WheelSettingsTV_Create(), true)
    {
    }

    internal WheelSettingsTV(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public float LongitudinalFriction
    {
        get => JPH_WheelSettingsTV_GetLongitudinalFriction(Handle);
        set => JPH_WheelSettingsTV_SetLongitudinalFriction(Handle, value);
    }

    public float LateralFriction
    {
        get => JPH_WheelSettingsTV_GetLateralFriction(Handle);
        set => JPH_WheelSettingsTV_SetLateralFriction(Handle, value);
    }

    internal static new WheelSettingsTV? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new WheelSettingsTV(h, false));
}

public sealed class WheelTV : Wheel
{
    public WheelTV(WheelSettingsTV settings)
        : base(JPH_WheelTV_Create(settings.Handle), true)
    {
    }

    internal WheelTV(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public new WheelSettingsTV Settings => WheelSettingsTV.GetObject(JPH_WheelTV_GetSettings(Handle))!;
}


public unsafe class TrackedVehicleControllerSettings : VehicleControllerSettings
{
    public TrackedVehicleControllerSettings()
        : base(JPH_TrackedVehicleControllerSettings_Create(), true)
    {
    }

    internal TrackedVehicleControllerSettings(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public VehicleEngineSettings Engine
    {
        get
        {
            JPH_TrackedVehicleControllerSettings_GetEngine(Handle, out JPH_VehicleEngineSettings native);
            VehicleEngineSettings result = new();
            result.FromNative(native);
            return result;
        }
        set
        {
            JPH_VehicleEngineSettings native = new();
            value.ToNative(&native);
            JPH_TrackedVehicleControllerSettings_SetEngine(Handle, in native);
        }
    }

    public VehicleTransmissionSettings Transmission
    {
        get
        {
            return VehicleTransmissionSettings.GetObject(JPH_TrackedVehicleControllerSettings_GetTransmission(Handle))!;
        }
        set
        {
            JPH_TrackedVehicleControllerSettings_SetTransmission(Handle, value.Handle);
        }
    }

    // TODO: VehicleTrackSettings
}

public class TrackedVehicleController : VehicleController
{
    internal TrackedVehicleController(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public void SetDriverInput(float forward, float leftRatio, float rightRatio, float brake)
    {
        JPH_TrackedVehicleController_SetDriverInput(Handle, forward, leftRatio, rightRatio, brake);
    }

    public float ForwardInput
    {
        get => JPH_TrackedVehicleController_GetForwardInput(Handle);
        set => JPH_TrackedVehicleController_SetForwardInput(Handle, value);
    }

    public float LeftRatio
    {
        get => JPH_TrackedVehicleController_GetLeftRatio(Handle);
        set => JPH_TrackedVehicleController_SetLeftRatio(Handle, value);
    }

    public float RightRatio
    {
        get => JPH_TrackedVehicleController_GetRightRatio(Handle);
        set => JPH_TrackedVehicleController_SetRightRatio(Handle, value);
    }

    public float BrakeInput
    {
        get => JPH_TrackedVehicleController_GetBrakeInput(Handle);
        set => JPH_TrackedVehicleController_SetBrakeInput(Handle, value);
    }
}
