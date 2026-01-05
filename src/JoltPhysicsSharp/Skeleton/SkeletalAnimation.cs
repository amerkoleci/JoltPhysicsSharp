// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class SkeletalAnimation : NativeObject
{
    internal SkeletalAnimation(nint handle, bool owns = true)
       : base(handle, owns)
    {
    }

    public SkeletalAnimation()
        : base(JPH_SkeletalAnimation_Create())
    {
    }

    protected override void DisposeNative()
    {
        JPH_SkeletalAnimation_Destroy(Handle);
    }

    public float Duration => JPH_SkeletalAnimation_GetDuration(Handle);
    public int AnimatedJointCount => JPH_SkeletalAnimation_GetAnimatedJointCount(Handle);

    public bool IsLooping
    {
        get => JPH_SkeletalAnimation_IsLooping(Handle);
        set => JPH_SkeletalAnimation_SetIsLooping(Handle, value);
    }

    public void ScaleJoints(float scale)
    {
        JPH_SkeletalAnimation_ScaleJoints(Handle, scale);
    }

    public void Sample(float time, SkeletonPose pose)
    {
        JPH_SkeletalAnimation_Sample(Handle, time, pose.Handle);
    }

    public void AddAnimatedJoint(string jointName)
    {
        JPH_SkeletalAnimation_AddAnimatedJoint(Handle, jointName);
    }

    public void AddKeyframe(int jointIndex, float time, in Vector3 translation, in Quaternion rotation)
    {
        JPH_SkeletalAnimation_AddKeyframe(Handle, jointIndex, time, in translation, in rotation);
    }


    internal static SkeletalAnimation? GetObject(nint handle) => GetOrAddObject(handle, h => new SkeletalAnimation(h, false));
}
