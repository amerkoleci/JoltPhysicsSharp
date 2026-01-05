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

    public void ResizeParts(int count)
    {
        JPH_RagdollSettings_ResizeParts(Handle, count);
    }

    public int PartCount => JPH_RagdollSettings_GetPartCount(Handle);

    public void SetPartShape(int partIndex, Shape shape)
    {
        JPH_RagdollSettings_SetPartShape(Handle, partIndex, shape.Handle);
    }
    public void SetPartPosition(int partIndex, in Vector3 position)
    {
        JPH_RagdollSettings_SetPartPosition(Handle, partIndex, position);
    }
    public void SetPartRotation(int partIndex, in Quaternion rotation)
    {
        JPH_RagdollSettings_SetPartRotation(Handle, partIndex, rotation);
    }

    public void SetPartMotionType(int partIndex, MotionType motionType)
    {
        JPH_RagdollSettings_SetPartMotionType(Handle, partIndex, motionType);
    }
    public void SetPartObjectLayer(int partIndex, ObjectLayer layer)
    {
        JPH_RagdollSettings_SetPartObjectLayer(Handle, partIndex, layer.Value);
    }
    public void SetPartMassProperties(int partIndex, float mass)
    {
        JPH_RagdollSettings_SetPartMassProperties(Handle, partIndex, mass);
    }

    public unsafe void SetPartToParent(int partIndex, SwingTwistConstraintSettings constraintSettings)
    {
        JPH_SwingTwistConstraintSettings nativeSettings;
        constraintSettings.ToNative(&nativeSettings);

        JPH_RagdollSettings_SetPartToParent(Handle, partIndex, &nativeSettings);
    }

    internal static RagdollSettings? GetObject(nint handle) => GetOrAddObject(handle, h => new RagdollSettings(h, false));
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

    public void SetPose(SkeletonPose pose, bool lockBodies = true)
    {
        JPH_Ragdoll_SetPose(Handle, pose.Handle, lockBodies);
    }

    public void SetPose(in Vector3 rootOffset, in Matrix4x4 jointMatrices, bool lockBodies = true)
    {
        JPH_Ragdoll_SetPose2(Handle, rootOffset, jointMatrices, lockBodies);
    }

    public SkeletonPose? GetPose(bool lockBodies = true)
    {
        JPH_Ragdoll_GetPose(Handle, out nint handle, lockBodies);
        if (handle == 0)
            return default;

        return SkeletonPose.GetObject(handle);
    }

    public void GetPose2(out Vector3 rootOffset, out Matrix4x4 jointMatrices, bool lockBodies = true)
    {
        JPH_Ragdoll_GetPose2(Handle, out rootOffset, out jointMatrices, lockBodies);
    }

    public void DriveToPoseUsingMotors(SkeletonPose pose)
    {
        JPH_Ragdoll_DriveToPoseUsingMotors(Handle, pose.Handle);
    }

    public void DriveToPoseUsingKinematics(SkeletonPose pose, float deltaTime, bool lockBodies = true)
    {
        JPH_Ragdoll_DriveToPoseUsingKinematics(Handle, pose.Handle, deltaTime, lockBodies);
    }

    public int BodyCount => JPH_Ragdoll_GetBodyCount(Handle);
    public int ConstraintCount => JPH_Ragdoll_GetConstraintCount(Handle);

    public BodyID GetBodyID(int bodyIndex) => JPH_Ragdoll_GetBodyID(Handle, bodyIndex);
    public TwoBodyConstraint? GetConstraint(int bodyIndex) => TwoBodyConstraint.GetObject(JPH_Ragdoll_GetConstraint(Handle, bodyIndex));

    public void GetRootTransform(out Vector3 position, out Quaternion rotation, bool lockBodies = true)
    {
        JPH_Ragdoll_GetRootTransform(Handle, out position, out rotation, lockBodies);
    }

    public RagdollSettings? RagdollSettings
    {
        get => RagdollSettings.GetObject(JPH_Ragdoll_GetRagdollSettings(Handle));
    }

    internal static Ragdoll? GetObject(nint handle) => GetOrAddObject(handle, h => new Ragdoll(h, false));
}
