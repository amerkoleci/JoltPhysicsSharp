// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class VehicleEngine : NativeObject
{
    internal VehicleEngine(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    internal static VehicleEngine? GetObject(nint handle) => GetOrAddObject(handle, h => new VehicleEngine(h, false));

    public float CurrentRPM
    {
        get => JPH_VehicleEngine_GetCurrentRPM(Handle);
        set => JPH_VehicleEngine_SetCurrentRPM(Handle, value);
    }


    public float AngularVelocity => JPH_VehicleEngine_GetAngularVelocity(Handle);
    public bool AllowSleep => JPH_VehicleEngine_AllowSleep(Handle);

    public void ClampRPM()
    {
        JPH_VehicleEngine_ClampRPM(Handle);
    }

    public float GetTorque(float acceleration) => JPH_VehicleEngine_GetTorque(Handle, acceleration);

    public void ApplyTorque(float torque, float deltaTime) => JPH_VehicleEngine_ApplyTorque(Handle, torque, deltaTime);
    public void ApplyDamping(float deltaTime) => JPH_VehicleEngine_ApplyDamping(Handle, deltaTime);
}
