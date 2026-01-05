// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

internal static unsafe partial class JoltApi
{

    #region Skeleton
    public readonly struct SkeletonJoint
    {
        public readonly byte* name;
        public readonly byte* parentName;
        public readonly int parentJointIndex;
    }

    [LibraryImport(LibName)]
    public static partial nint JPH_Skeleton_Create();
    [LibraryImport(LibName)]
    public static partial void JPH_Skeleton_Destroy(nint skeleton);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial uint JPH_Skeleton_AddJoint(nint skeleton, string name);
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial uint JPH_Skeleton_AddJoint2(nint skeleton, string name, int parentIndex);
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial uint JPH_Skeleton_AddJoint3(nint skeleton, string name, string parentName);
    [LibraryImport(LibName)]
    public static partial int JPH_Skeleton_GetJointCount(nint skeleton);
    [LibraryImport(LibName)]
    public static partial void JPH_Skeleton_GetJoint(nint skeleton, int index, out SkeletonJoint joint);
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int JPH_Skeleton_GetJointIndex(nint skeleton, string name);
    [LibraryImport(LibName)]
    public static partial void JPH_Skeleton_CalculateParentJointIndices(nint skeleton);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Skeleton_AreJointsCorrectlyOrdered(nint skeleton);
    #endregion

    #region SkeletonPose
    [LibraryImport(LibName)]
    public static partial nint JPH_SkeletonPose_Create();
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_Destroy(nint pose);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_SetSkeleton(nint pose, nint skeleton);
    [LibraryImport(LibName)]
    public static partial nint JPH_SkeletonPose_GetSkeleton(nint pose);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_SetRootOffset(nint pose, in Vector3 offset); // RVec3
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_GetRootOffset(nint pose, out Vector3 result);  // RVec3
    [LibraryImport(LibName)]
    public static partial int JPH_SkeletonPose_GetJointCount(nint pose);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_GetJointState(nint pose, int index, out Vector3 translation, out Quaternion rotation);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_SetJointState(nint pose, int index, in Vector3 translation, in Quaternion rotation);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_GetJointMatrix(nint pose, int index, out Matrix4x4 result);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_SetJointMatrix(nint pose, int index, in Matrix4x4 matrix);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_GetJointMatrices(nint pose, Matrix4x4* outMatrices, int count);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_SetJointMatrices(nint pose, Matrix4x4* matrices, int count);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_CalculateJointMatrices(nint pose);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_CalculateJointStates(nint pose);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonPose_CalculateLocalSpaceJointMatrices(nint pose, Matrix4x4* outMatrices);
    #endregion

    #region SkeletalAnimation
    [LibraryImport(LibName)]
    public static partial nint JPH_SkeletalAnimation_Create();
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletalAnimation_Destroy(nint animation);
    [LibraryImport(LibName)]
    public static partial float JPH_SkeletalAnimation_GetDuration(nint animation);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_SkeletalAnimation_IsLooping(nint animation);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletalAnimation_SetIsLooping(nint animation, [MarshalAs(UnmanagedType.U1)] bool looping);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletalAnimation_ScaleJoints(nint animation, float scale);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletalAnimation_Sample(nint animation, float time, /*JPH_SkeletonPose**/nint pose);
    [LibraryImport(LibName)]
    public static partial int JPH_SkeletalAnimation_GetAnimatedJointCount(nint animation);
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial void JPH_SkeletalAnimation_AddAnimatedJoint(nint animation, string jointName);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletalAnimation_AddKeyframe(nint animation, int jointIndex, float time, in Vector3 translation, in Quaternion rotation);
    #endregion

    #region SkeletonMapper
    [LibraryImport(LibName)]
    public static partial nint JPH_SkeletonMapper_Create();
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonMapper_Destroy(nint mapper);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonMapper_Initialize(nint mapper, nint skeleton1, in Matrix4x4 neutralPose1, nint skeleton2, in Matrix4x4 neutralPose2);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonMapper_LockAllTranslations(nint mapper, nint skeleton2, in Matrix4x4 neutralPose2);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonMapper_LockTranslations(nint mapper, nint skeleton2, Bool8* lockedTranslations, in Matrix4x4 neutralPose2);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonMapper_Map(nint mapper, in Matrix4x4 pose1ModelSpace, in Matrix4x4 pose2LocalSpace, out Matrix4x4 outPose2ModelSpace);
    [LibraryImport(LibName)]
    public static partial void JPH_SkeletonMapper_MapReverse(nint mapper, in Matrix4x4 pose2ModelSpace, out Matrix4x4 outPose1ModelSpace);
    [LibraryImport(LibName)]
    public static partial int JPH_SkeletonMapper_GetMappedJointIndex(nint mapper, int joint1Index);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_SkeletonMapper_IsJointTranslationLocked(nint mapper, int joint2Index);
    #endregion
}
