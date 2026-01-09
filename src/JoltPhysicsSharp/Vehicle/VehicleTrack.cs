// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class VehicleTrack : NativeObject
{
    internal VehicleTrack(nint handle, bool owns = false)
        : base(handle, owns)
    {
    }

    internal static VehicleTrack? GetObject(nint handle) => GetOrAddObject(handle, h => new VehicleTrack(h, false));

    public float AngularVelocity
    {
        get => JPH_VehicleTrack_GetAngularVelocity(Handle);
        set => JPH_VehicleTrack_SetAngularVelocity(Handle, value);
    }

    public uint DrivenWheel => JPH_VehicleTrack_GetDrivenWheel(Handle);
    public float Inertia => JPH_VehicleTrack_GetInertia(Handle);
    public float AngularDamping => JPH_VehicleTrack_GetAngularDamping(Handle);
    public float MaxBrakeTorque => JPH_VehicleTrack_GetMaxBrakeTorque(Handle);
    public float DifferentialRatio => JPH_VehicleTrack_GetDifferentialRatio(Handle);
}
