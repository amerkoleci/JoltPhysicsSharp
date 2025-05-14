﻿// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class VehicleCollisionTester : NativeObject
{
    protected VehicleCollisionTester(nint handle)
        : base(handle)
    {
    }
}
