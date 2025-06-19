// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleCollisionTester : NativeObject
{
    internal VehicleCollisionTester(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleCollisionTester_Destroy(Handle);
    }

    public ObjectLayer objectLayer
    {
        get => JPH_VehicleCollisionTester_GetObjectLayer(Handle);
        set => JPH_VehicleCollisionTester_SetObjectLayer(Handle, value);
    }

    internal static VehicleCollisionTester? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new VehicleCollisionTester(h, false));
}
