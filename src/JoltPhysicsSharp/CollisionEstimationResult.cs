// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly unsafe struct CollisionEstimationResult
{
    public readonly Vector3 LinearVelocity1;
    public readonly Vector3 AngularVelocity1;
    public readonly Vector3 LinearVelocity2;
    public readonly Vector3 AngularVelocity2;

    public readonly Vector3 FrictionPoint;
    public readonly Vector3 FrictionImpulse;
    public readonly float AngularFrictionImpulse;

    public readonly int ContactImpulseCount;
    public readonly float* ContactImpulsesPtr;
    public Span<float> ContactImpulses => new(ContactImpulsesPtr, ContactImpulseCount);

    public void Free()
    {
        JPH_CollisionEstimationResult_FreeMembers(this);
    }
}
