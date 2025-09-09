// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public struct ContactSettings
{
    /// <summary>
    /// Combined friction for the body pair (<see cref="PhysicsSystem.SetCombineFriction"/>)
    /// </summary>
    public float CombinedFriction;
    /// <summary>
    /// Combined restitution for the body pair (<see cref="PhysicsSystem.SetCombineFriction"/>)
    /// </summary>
	public float CombinedRestitution;
    /// <summary>
    /// Scale factor for the inverse mass of body 1 (0 = infinite mass, 1 = use original mass, 2 = body has half the mass). For the same contact pair, you should strive to keep the value the same over time.
    /// </summary>
	public float InverseMassScale1; // 1.0f
    /// <summary>
    /// Scale factor for the inverse inertia of body 1 (usually same as mInvMassScale1)
    /// </summary>
	public float InverseInertiaScale1; // 1.0f
    /// <summary>
    /// Scale factor for the inverse mass of body 2 (0 = infinite mass, 1 = use original mass, 2 = body has half the mass). For the same contact pair, you should strive to keep the value the same over time.
    /// </summary>
    public float InverseMassScale2; // 1.0f
    /// <summary>
    /// Scale factor for the inverse inertia of body 2 (usually same as <see cref="InverseMassScale2"/>)
    /// </summary>
	public float InverseInertiaScale2; // 1.0f
    /// <summary>
    /// If the contact should be treated as a sensor vs body contact (no collision response)
    /// </summary>
    public Bool32 IsSensor;
    /// <summary>
    /// Relative linear surface velocity between the bodies (world space surface velocity of body 2 - world space surface velocity of body 1), can be used to create a conveyor belt effect
    /// </summary>
	public Vector3 RelativeLinearSurfaceVelocity;
    /// <summary>
    /// Relative angular surface velocity between the bodies (world space angular surface velocity of body 2 - world space angular surface velocity of body 1). Note that this angular velocity is relative to the center of mass of body 1, so if you want it relative to body 2's center of mass you need to add body 2 angular velocity x (body 1 world space center of mass - body 2 world space center of mass) to mRelativeLinearSurfaceVelocity.
    /// </summary>
	public Vector3 RelativeAngularSurfaceVelocity;
}
