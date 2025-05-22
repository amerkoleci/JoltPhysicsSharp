// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class PhysicsStepListener : NativeObject
{
    public PhysicsStepListener(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }
}
