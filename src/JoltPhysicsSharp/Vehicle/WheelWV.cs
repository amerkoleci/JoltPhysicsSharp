// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheelWV : NativeObject
{
    public WheelWV(WheelSettingsWV settingsWV)
        : base(JPH_WheelWV_Create(settingsWV.Handle))
    {
    }

    protected WheelWV(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_WheelWV_Destroy(Handle);
    }
}
