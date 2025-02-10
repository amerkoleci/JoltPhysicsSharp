// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;

namespace JoltPhysicsSharp;

public readonly unsafe struct CollisionEstimationResult
{
    public readonly struct Impulse
    {
        public readonly float ContactImpulse;
        public readonly float FrictionImpulse1;
        public readonly float FrictionImpulse2;
    }

    public readonly Vector3 LinearVelocity1;
    public readonly Vector3 AngularVelocity1;
    public readonly Vector3 LinearVelocity2;
    public readonly Vector3 AngularVelocity2;

    public readonly Vector3 Tangent1;
    public readonly Vector3 Tangent2;

    internal readonly int ImpulseCount;
    internal readonly Impulse* ImpulsesPtr;
    public Span<Impulse> Impulses => new(ImpulsesPtr, ImpulseCount);
}
