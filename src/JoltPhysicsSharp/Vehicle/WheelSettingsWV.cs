// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class WheelSettingsWV : NativeObject
{
    public WheelSettingsWV(
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
        bool enableSuspensionForcePoint,
        float inertia,
        float angularDamping,
        float maxSteerAngle,
        //LinearCurve longitudinalFriction, // NOTE: BGE: just using default values for now.
        //LinearCurve lateralFriction,      // NOTE: BGE: just using default values for now.
        float maxBrakeTorque,
        float maxHandBrakeTorque)
        : base(
            JPH_WheelSettingsWV_Create(
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
                enableSuspensionForcePoint,
                inertia,
                angularDamping,
                maxSteerAngle,
                //longitudinalFriction, // NOTE: BGE: just using default values for now.
                //lateralFriction,      // NOTE: BGE: just using default values for now.
                maxBrakeTorque,
                maxHandBrakeTorque))
    {
    }

    protected WheelSettingsWV(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_WheelSettingsWV_Destroy(Handle);
    }
}
