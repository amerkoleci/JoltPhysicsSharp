// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;


public unsafe class MotorcycleControllerSettings : WheeledVehicleControllerSettings
{
    public MotorcycleControllerSettings()
        : base(JPH_MotorcycleControllerSettings_Create(), true)
    {
    }

    internal MotorcycleControllerSettings(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public float MaxLeanAngle
    {
        get => JPH_MotorcycleControllerSettings_GetMaxLeanAngle(Handle);
        set => JPH_MotorcycleControllerSettings_SetMaxLeanAngle(Handle, value);
    }

    public float LeanSpringConstant
    {
        get => JPH_MotorcycleControllerSettings_GetLeanSpringConstant(Handle);
        set => JPH_MotorcycleControllerSettings_SetLeanSpringConstant(Handle, value);
    }

    public float LeanSpringDamping
    {
        get => JPH_MotorcycleControllerSettings_GetLeanSpringDamping(Handle);
        set => JPH_MotorcycleControllerSettings_SetLeanSpringDamping(Handle, value);
    }

    public float LeanSpringIntegrationCoefficient
    {
        get => JPH_MotorcycleControllerSettings_GetLeanSpringIntegrationCoefficient(Handle);
        set => JPH_MotorcycleControllerSettings_SetLeanSpringIntegrationCoefficient(Handle, value);
    }

    public float LeanSpringIntegrationCoefficientDecay
    {
        get => JPH_MotorcycleControllerSettings_GetLeanSpringIntegrationCoefficientDecay(Handle);
        set => JPH_MotorcycleControllerSettings_SetLeanSpringIntegrationCoefficientDecay(Handle, value);
    }

    public float LeanSmoothingFactor
    {
        get => JPH_MotorcycleControllerSettings_GetLeanSmoothingFactor(Handle);
        set => JPH_MotorcycleControllerSettings_SetLeanSmoothingFactor(Handle, value);
    }
}

public class MotorcycleController : WheeledVehicleController
{
    internal MotorcycleController(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    public float WheelBase
    {
        get => JPH_MotorcycleController_GetWheelBase(Handle);
    }

    public bool IsLeanControllerEnabled
    {
        get => JPH_MotorcycleController_IsLeanControllerEnabled(Handle);
        set => JPH_MotorcycleController_EnableLeanController(Handle, value);
    }

    public bool IsLeanSteeringLimitEnabled
    {
        get => JPH_MotorcycleController_IsLeanSteeringLimitEnabled(Handle);
        set => JPH_MotorcycleController_EnableLeanSteeringLimit(Handle, value);
    }

    public float LeanSpringConstant
    {
        get => JPH_MotorcycleController_GetLeanSpringConstant(Handle);
        set => JPH_MotorcycleController_SetLeanSpringConstant(Handle, value);
    }

    public float LeanSpringDamping
    {
        get => JPH_MotorcycleController_GetLeanSpringDamping(Handle);
        set => JPH_MotorcycleController_SetLeanSpringDamping(Handle, value);
    }

    public float LeanSpringIntegrationCoefficient
    {
        get => JPH_MotorcycleController_GetLeanSpringIntegrationCoefficient(Handle);
        set => JPH_MotorcycleController_SetLeanSpringIntegrationCoefficient(Handle, value);
    }

    public float LeanSpringIntegrationCoefficientDecay
    {
        get => JPH_MotorcycleController_GetLeanSpringIntegrationCoefficientDecay(Handle);
        set => JPH_MotorcycleController_SetLeanSpringIntegrationCoefficientDecay(Handle, value);
    }

    public float LeanSmoothingFactor
    {
        get => JPH_MotorcycleController_GetLeanSmoothingFactor(Handle);
        set => JPH_MotorcycleController_SetLeanSmoothingFactor(Handle, value);
    }
}
