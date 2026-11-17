// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleControllerSettings : NativeObject
{
    internal VehicleControllerSettings(nint handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleControllerSettings_Destroy(Handle);
    }

    internal static VehicleControllerSettings? GetObject(nint handle) => GetOrAddObject(handle, h => new VehicleControllerSettings(h, false));
}
