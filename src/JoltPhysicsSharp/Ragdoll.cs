// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class RagdollSettings : NativeObject
{
    internal RagdollSettings(nint handle, bool owns = true)
       : base(handle, owns)
    {
    }

    public RagdollSettings()
        : base(JPH_RagdollSettings_Create())
    {
    }

    protected override void DisposeNative()
    {
        JPH_RagdollSettings_Destroy(Handle);
    }

    public Skeleton? Skeleton
    {
        get => Skeleton.GetObject(JPH_RagdollSettings_GetSkeleton(Handle));
        set => JPH_RagdollSettings_SetSkeleton(Handle, value?.Handle ?? nint.Zero);
    }

    public bool Stabilize() => JPH_RagdollSettings_Stabilize(Handle);

    public unsafe void DisableParentChildCollisions(in Matrix4x4? jointMatrices = default, float minSeparationDistance = 0.0f)
    {
        Mat4 callJointMatrices = jointMatrices.HasValue ? jointMatrices.Value.ToJolt() : default;
        JPH_RagdollSettings_DisableParentChildCollisions(Handle,
            jointMatrices.HasValue ? &callJointMatrices : default,
            minSeparationDistance);
    }

    public void CalculateBodyIndexToConstraintIndex()
    {
        JPH_RagdollSettings_CalculateBodyIndexToConstraintIndex(Handle);
    }

    public int GetConstraintIndexForBodyIndex(int bodyIndex)
    {
        return JPH_RagdollSettings_GetConstraintIndexForBodyIndex(Handle, bodyIndex);
    }

    public void CalculateConstraintIndexToBodyIdxPair()
    {
        JPH_RagdollSettings_CalculateConstraintIndexToBodyIdxPair(Handle);
    }
}

public class Ragdoll : NativeObject
{
    internal Ragdoll(nint handle, bool owns = true)
       : base(handle, owns)
    {
    }

    public Ragdoll(RagdollSettings settings, PhysicsSystem system)
        : base(JPH_RagdollSettings_CreateRagdoll(settings.Handle, system.Handle, 0u, 0u))
    {
    }

    public Ragdoll(RagdollSettings settings, PhysicsSystem system, CollisionGroupID collisionGroup)
        : base(JPH_RagdollSettings_CreateRagdoll(settings.Handle, system.Handle, collisionGroup.Value, 0u))
    {
    }

    public Ragdoll(RagdollSettings settings, PhysicsSystem system, CollisionGroupID collisionGroup, ulong userData)
        : base(JPH_RagdollSettings_CreateRagdoll(settings.Handle, system.Handle, collisionGroup.Value, userData))
    {
    }

    protected override void DisposeNative()
    {
        JPH_Ragdoll_Destroy(Handle);
    }

    public void AddToPhysicsSystem(Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        JPH_Ragdoll_AddToPhysicsSystem(Handle, activationMode, lockBodies);
    }

    public void RemoveFromPhysicsSystem(bool lockBodies = true)
    {
        JPH_Ragdoll_RemoveFromPhysicsSystem(Handle, lockBodies);
    }

    public void Activate(bool lockBodies = true) => JPH_Ragdoll_Activate(Handle, lockBodies);
    public bool IsActive(bool lockBodies = true) => JPH_Ragdoll_IsActive(Handle, lockBodies);
    public void ResetWarmStart() => JPH_Ragdoll_ResetWarmStart(Handle);

    internal static Ragdoll? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new Ragdoll(h, false));
}
