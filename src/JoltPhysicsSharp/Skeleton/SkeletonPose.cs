// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class SkeletonPose : NativeObject
{
    internal SkeletonPose(nint handle, bool owns = true)
       : base(handle, owns)
    {
    }

    public SkeletonPose()
        : base(JPH_SkeletonPose_Create())
    {
    }

    protected override void DisposeNative()
    {
        JPH_SkeletonPose_Destroy(Handle);
    }

    public Skeleton Skeleton
    {
        get => Skeleton.GetObject(JPH_SkeletonPose_GetSkeleton(Handle));
        set => JPH_SkeletonPose_SetSkeleton(Handle, value.Handle);
    }

    public Vector3 RootOffset
    {
        get
        {
            JPH_SkeletonPose_GetRootOffset(Handle, out Vector3 result);
            return result;
        }
        set => JPH_SkeletonPose_SetRootOffset(Handle, in value);
    }

    public int JointCount => JPH_SkeletonPose_GetJointCount(Handle);

    public void GetJointState(int index, out Vector3 translation, out Quaternion rotation)
    {
        JPH_SkeletonPose_GetJointState(Handle, index, out translation, out rotation);
    }

    public void SetJointState(int index, in Vector3 translation, in Quaternion rotation)
    {
        JPH_SkeletonPose_SetJointState(Handle, index, in translation, in rotation);
    }

    public void GetJointMatrix(int index, out Matrix4x4 result)
    {
        JPH_SkeletonPose_GetJointMatrix(Handle, index, out result);
    }

    public void SetJointMatrix(int index, in Matrix4x4 matrix)
    {
        JPH_SkeletonPose_SetJointMatrix(Handle, index, in matrix);
    }

    public unsafe void GetJointMatrices(Matrix4x4* outMatrices, int count)
    {
        JPH_SkeletonPose_GetJointMatrices(Handle, outMatrices, count);
    }

    public unsafe void GetJointMatrices(Span<Matrix4x4> outMatrices)
    {
        fixed (Matrix4x4* outMatricesPtr = outMatrices)
        {
            JPH_SkeletonPose_GetJointMatrices(Handle, outMatricesPtr, outMatrices.Length);
        }
    }

    public unsafe void SetJointMatrices(Span<Matrix4x4> matrices)
    {
        fixed (Matrix4x4* matricesPtr = matrices)
        {
            JPH_SkeletonPose_SetJointMatrices(Handle, matricesPtr, matrices.Length);
        }
    }

    public unsafe void SetJointMatrices(Matrix4x4* matrices, int count)
    {
        JPH_SkeletonPose_SetJointMatrices(Handle, matrices, count);
    }

    public void CalculateJointMatrices()
    {
        JPH_SkeletonPose_CalculateJointMatrices(Handle);
    }

    public void CalculateJointStates()
    {
        JPH_SkeletonPose_CalculateJointStates(Handle);
    }

    public unsafe void CalculateLocalSpaceJointMatrices(Span<Matrix4x4> outMatrices)
    {
        fixed (Matrix4x4* outMatricesPtr = outMatrices)
        {
            JPH_SkeletonPose_CalculateLocalSpaceJointMatrices(Handle, outMatricesPtr);
        }
    }

    public unsafe void CalculateLocalSpaceJointMatrices(Matrix4x4* outMatrices)
    {
        JPH_SkeletonPose_CalculateLocalSpaceJointMatrices(Handle, outMatrices);
    }

    internal static SkeletonPose? GetObject(nint handle) => GetOrAddObject(handle, h => new SkeletonPose(h, false));
}
