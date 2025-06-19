// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleTransmissionSettings : NativeObject
{
    public VehicleTransmissionSettings()
        : base(JPH_VehicleTransmissionSettings_Create(), true)
    {
    }

    internal VehicleTransmissionSettings(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleTransmissionSettings_Destroy(Handle);
    }

    public TransmissionMode Mode
    {
        get => JPH_VehicleTransmissionSettings_GetMode(Handle);
        set => JPH_VehicleTransmissionSettings_SetMode(Handle, value);
    }

    public int GearRatioCount => JPH_VehicleTransmissionSettings_GetGearRatioCount(Handle);

    public unsafe Span<float> GearRatios
    {
        get
        {
            int count = GearRatioCount;
            return new Span<float>(JPH_VehicleTransmissionSettings_GetGearRatios(Handle), count);
        }
        set
        {
            fixed (float* ptr = value)
            {
                JPH_VehicleTransmissionSettings_SetGearRatios(Handle, ptr, value.Length);
            }
        }
    }

    public float GetGearRatio(int index)
    {
        if (index < 0 || index >= GearRatioCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        return JPH_VehicleTransmissionSettings_GetGearRatio(Handle, index);
    }

    public void SetGearRatio(int index, float ratio)
    {
        if (index < 0 || index >= GearRatioCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }
        JPH_VehicleTransmissionSettings_SetGearRatio(Handle, index, ratio);
    }

    public int ReverseGearRatioCount => JPH_VehicleTransmissionSettings_GetReverseGearRatioCount(Handle);

    public unsafe Span<float> ReverseGearRatios
    {
        get
        {
            int count = ReverseGearRatioCount;
            return new Span<float>(JPH_VehicleTransmissionSettings_GetReverseGearRatios(Handle), count);
        }
        set
        {
            fixed (float* ptr = value)
            {
                JPH_VehicleTransmissionSettings_SetReverseGearRatios(Handle, ptr, value.Length);
            }
        }
    }

    public float GetReverseGearRatio(int index)
    {
        if (index < 0 || index >= ReverseGearRatioCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }

        return JPH_VehicleTransmissionSettings_GetReverseGearRatio(Handle, index);
    }

    public void SetReverseGearRatio(int index, float ratio)
    {
        if (index < 0 || index >= ReverseGearRatioCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
        }
        JPH_VehicleTransmissionSettings_SetReverseGearRatio(Handle, index, ratio);
    }

    public float SwitchTime
    {
        get => JPH_VehicleTransmissionSettings_GetSwitchTime(Handle);
        set => JPH_VehicleTransmissionSettings_SetSwitchTime(Handle, value);
    }

    public float ClutchReleaseTime
    {
        get => JPH_VehicleTransmissionSettings_GetClutchReleaseTime(Handle);
        set => JPH_VehicleTransmissionSettings_SetClutchReleaseTime(Handle, value);
    }

    public float SwitchLatency
    {
        get => JPH_VehicleTransmissionSettings_GetSwitchLatency(Handle);
        set => JPH_VehicleTransmissionSettings_SetSwitchLatency(Handle, value);
    }

    public float ShiftUpRPM
    {
        get => JPH_VehicleTransmissionSettings_GetShiftUpRPM(Handle);
        set => JPH_VehicleTransmissionSettings_SetShiftUpRPM(Handle, value);
    }

    public float ShiftDownRPM
    {
        get => JPH_VehicleTransmissionSettings_GetShiftDownRPM(Handle);
        set => JPH_VehicleTransmissionSettings_SetShiftDownRPM(Handle, value);
    }

    public float ClutchStrength
    {
        get => JPH_VehicleTransmissionSettings_GetClutchStrength(Handle);
        set => JPH_VehicleTransmissionSettings_SetClutchStrength(Handle, value);
    }

    internal static VehicleTransmissionSettings? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new VehicleTransmissionSettings(h, false));
}
