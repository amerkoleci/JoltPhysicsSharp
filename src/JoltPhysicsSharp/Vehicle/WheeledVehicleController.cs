// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheelSettingsWV : WheelSettings
{
    public WheelSettingsWV()
        : base(JPH_WheelSettingsWV_Create(), true)
    {
    }

    internal WheelSettingsWV(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public float Inertia
    {
        get => JPH_WheelSettingsWV_GetInertia(Handle);
        set => JPH_WheelSettingsWV_SetInertia(Handle, value);
    }

    public float MaxSteerAngle
    {
        get => JPH_WheelSettingsWV_GetMaxSteerAngle(Handle);
        set => JPH_WheelSettingsWV_SetMaxSteerAngle(Handle, value);
    }

    public float MaxBrakeTorque
    {
        get => JPH_WheelSettingsWV_GetMaxBrakeTorque(Handle);
        set => JPH_WheelSettingsWV_SetMaxBrakeTorque(Handle, value);
    }

    public float MaxHandBrakeTorque
    {
        get => JPH_WheelSettingsWV_GetMaxHandBrakeTorque(Handle);
        set => JPH_WheelSettingsWV_SetMaxHandBrakeTorque(Handle, value);
    }

    internal static new WheelSettingsWV? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new WheelSettingsWV(h, false));
}

public sealed class WheelWV : Wheel
{
    public WheelWV(WheelSettingsWV settings)
        : base(JPH_WheelWV_Create(settings.Handle), true)
    {
    }

    internal WheelWV(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public new WheelSettingsWV Settings => WheelSettingsWV.GetObject(JPH_WheelWV_GetSettings(Handle))!;

    public void ApplyTorque(float torque, float deltaTime)
    {
        JPH_WheelWV_ApplyTorque(Handle, torque, deltaTime);
    }
}


public unsafe class WheeledVehicleControllerSettings : VehicleControllerSettings
{
    public WheeledVehicleControllerSettings()
        : base(JPH_WheeledVehicleControllerSettings_Create(), true)
    {
    }

    internal WheeledVehicleControllerSettings(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public VehicleEngineSettings Engine
    {
        get
        {
            JPH_WheeledVehicleControllerSettings_GetEngine(Handle, out JPH_VehicleEngineSettings native);
            VehicleEngineSettings result = new();
            result.FromNative(native);
            return result;
        }
        set
        {
            JPH_VehicleEngineSettings native = new();
            value.ToNative(&native);
            JPH_WheeledVehicleControllerSettings_SetEngine(Handle, in native);
        }
    }

    public VehicleTransmissionSettings Transmission
    {
        get
        {
            return VehicleTransmissionSettings.GetObject(JPH_WheeledVehicleControllerSettings_GetTransmission(Handle))!;
        }
        set
        {
            JPH_WheeledVehicleControllerSettings_SetTransmission(Handle, value.Handle);
        }
    }

    public int DifferentialsCount
    {
        get => JPH_WheeledVehicleControllerSettings_GetDifferentialsCount(Handle);
        set => JPH_WheeledVehicleControllerSettings_SetDifferentialsCount(Handle, value);
    }

    public float DifferentialLimitedSlipRatio
    {
        get => JPH_WheeledVehicleControllerSettings_GetDifferentialLimitedSlipRatio(Handle);
        set => JPH_WheeledVehicleControllerSettings_SetDifferentialLimitedSlipRatio(Handle, value);
    }

    public VehicleDifferentialSettings GetDifferential(int index)
    {
        if (index < 0 || index >= DifferentialsCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        JPH_VehicleDifferentialSettings native;
        JPH_WheeledVehicleControllerSettings_GetDifferential(Handle, index, &native);
        VehicleDifferentialSettings result = new();
        result.FromNative(native);
        return result;
    }

    public void SetDifferential(int index, VehicleDifferentialSettings settings)
    {
        if (index < 0 || index >= DifferentialsCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        JPH_VehicleDifferentialSettings native = new();
        settings.ToNative(&native);
        JPH_WheeledVehicleControllerSettings_SetDifferential(Handle, index, &native);
    }
}

public class WheeledVehicleController : VehicleController
{
    internal WheeledVehicleController(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public void SetDriverInput(float forward, float right, float brake, float handBrake)
    {
        JPH_WheeledVehicleController_SetDriverInput(Handle, forward, right, brake, handBrake);
    }

    public float ForwardInput
    {
        get => JPH_WheeledVehicleController_GetForwardInput(Handle);
        set => JPH_WheeledVehicleController_SetForwardInput(Handle, value);
    }

    public float RightInput
    {
        get => JPH_WheeledVehicleController_GetRightInput(Handle);
        set => JPH_WheeledVehicleController_SetRightInput(Handle, value);
    }

    public float BrakeInput
    {
        get => JPH_WheeledVehicleController_GetBrakeInput(Handle);
        set => JPH_WheeledVehicleController_SetBrakeInput(Handle, value);
    }

    public float HandBrakeInput
    {
        get => JPH_WheeledVehicleController_GetHandBrakeInput(Handle);
        set => JPH_WheeledVehicleController_SetHandBrakeInput(Handle, value);
    }

    public float WheelSpeedAtClutch
    {
        get => JPH_WheeledVehicleController_GetWheelSpeedAtClutch(Handle);
    }
}
