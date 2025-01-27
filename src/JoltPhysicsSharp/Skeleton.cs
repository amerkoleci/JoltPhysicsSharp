// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class Skeleton : NativeObject
{
    public struct Joint
    {
        public string Name;
        public string? ParentName;
        public int ParentJointIndex;
    }

    internal Skeleton(nint handle, bool owns = true)
       : base(handle, owns)
    {
    }

    public Skeleton()
        : base(JPH_Skeleton_Create())
    {
    }

    protected override void DisposeNative()
    {
        JPH_Skeleton_Destroy(Handle);
    }

    public int JointCount => JPH_Skeleton_GetJointCount(Handle);

    public uint AddJoint(string name) => JPH_Skeleton_AddJoint(Handle, name);
    public uint AddJoint(string name, int parentIndex) => JPH_Skeleton_AddJoint2(Handle, name, parentIndex);
    public uint AddJoint(string name, string parentName) => JPH_Skeleton_AddJoint3(Handle, name, parentName);

    public unsafe Joint GetJoint(int index)
    {
        JPH_Skeleton_GetJoint(Handle, index, out SkeletonJoint nativeJoint);
        return new Joint
        {
            Name = ConvertToManaged(nativeJoint.name)!,
            ParentName = ConvertToManaged(nativeJoint.parentName),
            ParentJointIndex = nativeJoint.parentJointIndex
        };
    }

    public unsafe void GetJoint(int index, out Joint joint)
    {
        JPH_Skeleton_GetJoint(Handle, index, out SkeletonJoint nativeJoint);
        joint = new()
        {
            Name = ConvertToManaged(nativeJoint.name)!,
            ParentName = ConvertToManaged(nativeJoint.parentName),
            ParentJointIndex = nativeJoint.parentJointIndex
        };
    }

    public int GetJointIndex(string name) => JPH_Skeleton_GetJointIndex(Handle, name);

    public void CalculateParentJointIndices() => JPH_Skeleton_CalculateParentJointIndices(Handle);
    public bool AreJointsCorrectlyOrdered() => JPH_Skeleton_AreJointsCorrectlyOrdered(Handle);

    internal static Skeleton? GetObject(nint handle) => GetOrAddObject(handle, (nint h) => new Skeleton(h, false));
}
