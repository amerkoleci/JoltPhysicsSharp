// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class VehicleControllerSettings : NativeObject
{
    protected VehicleControllerSettings()
    {
    }

    internal VehicleControllerSettings(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_VehicleControllerSettings_Destroy(Handle);
    }

    internal static VehicleControllerSettings? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new VehicleControllerSettings(h, false));
}
