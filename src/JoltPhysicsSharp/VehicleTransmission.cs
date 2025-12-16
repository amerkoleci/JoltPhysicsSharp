// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class VehicleTransmission : NativeObject
{
    internal VehicleTransmission(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    internal static VehicleTransmission? GetObject(nint handle) => GetOrAddObject(handle, h => new VehicleTransmission(h, false));

    public void Set(int currentGear, float clutchFriction)
    {
        JPH_VehicleTransmission_Set(Handle, currentGear, clutchFriction);
    }
    public void Update(float deltaTime, float currentRPM, float forwardInput, bool canShiftUp)
    {
        JPH_VehicleTransmission_Update(Handle, deltaTime, currentRPM, forwardInput, canShiftUp);
    }

    public int CurrentGear => JPH_VehicleTransmission_GetCurrentGear(Handle);
    public float ClutchFriction => JPH_VehicleTransmission_GetClutchFriction(Handle);
    public bool IsSwitchingGear => JPH_VehicleTransmission_IsSwitchingGear(Handle);
    public float CurrentRatio => JPH_VehicleTransmission_GetCurrentRatio(Handle);

    public bool AllowSleep => JPH_VehicleTransmission_AllowSleep(Handle);
}
