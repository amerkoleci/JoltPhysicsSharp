// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class WheelSettings : NativeObject
{
    public WheelSettings(
        in Vector3 position,
        in Vector3 suspensionForcePoint,
        in Vector3 suspensionDirection,
        in Vector3 steeringAxis,
        in Vector3 wheelUp,
        in Vector3 wheelForward,
        float suspensionMinLength,
        float suspensionMaxLength,
        float suspensionPreloadLength,
        in SpringSettings suspensionSpring,
        float radius,
        float width,
        bool enableSuspensionForcePoint)
        : base(
            JPH_WheelSettings_Create(
                position,
                suspensionForcePoint,
                suspensionDirection,
                steeringAxis,
                wheelUp,
                wheelForward,
                suspensionMinLength,
                suspensionMaxLength,
                suspensionPreloadLength,
                suspensionSpring,
                radius,
                width,
                enableSuspensionForcePoint))
    {
    }

    protected WheelSettings(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_WheelSettings_Destroy(Handle);
    }
}
