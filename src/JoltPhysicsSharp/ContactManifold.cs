// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly unsafe struct ContactManifold(nint handle) : IEquatable<ContactManifold>
{
    public nint Handle { get; } = handle; public bool IsNull => Handle == 0;
    public static ContactManifold Null => new(0);
    public static implicit operator ContactManifold(nint handle) => new(handle);
    public static bool operator ==(ContactManifold left, ContactManifold right) => left.Handle == right.Handle;
    public static bool operator !=(ContactManifold left, ContactManifold right) => left.Handle != right.Handle;
    public static bool operator ==(ContactManifold left, nint right) => left.Handle == right;
    public static bool operator !=(ContactManifold left, nint right) => left.Handle != right;
    public bool Equals(ContactManifold other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ContactManifold handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public readonly Vector3 WorldSpaceNormal
    {
        get
        {
            JPH_ContactManifold_GetWorldSpaceNormal(Handle, out Vector3 result);
            return result;
        }
    }

    public readonly float PenetrationDepth
    {
        get
        {
            return JPH_ContactManifold_GetPenetrationDepth(Handle);
        }
    }

    public readonly SubShapeID SubShapeID1
    {
        get
        {
            return JPH_ContactManifold_GetSubShapeID1(Handle);
        }
    }

    public readonly SubShapeID SubShapeID2
    {
        get
        {
            return JPH_ContactManifold_GetSubShapeID2(Handle);
        }
    }

    public readonly uint PointCount
    {
        get
        {
            return JPH_ContactManifold_GetPointCount(Handle);
        }
    }

    public Vector3 GetWorldSpaceContactPointOn1(uint index)
    {
        Vector3 result;
        JPH_ContactManifold_GetWorldSpaceContactPointOn1(Handle, index, &result);
        return result;
    }

    public void GetWorldSpaceContactPointOn1(uint index, out Vector3 result)
    {
        Unsafe.SkipInit(out result);
        fixed (Vector3* resultPtr = &result)
            JPH_ContactManifold_GetWorldSpaceContactPointOn1(Handle, index, resultPtr);
    }

    public void GetWorldSpaceContactPointOn1(uint index, Vector3* result)
    {
        JPH_ContactManifold_GetWorldSpaceContactPointOn1(Handle, index, result);
    }

    public Vector3 GetWorldSpaceContactPointOn2(uint index)
    {
        Vector3 result;
        JPH_ContactManifold_GetWorldSpaceContactPointOn2(Handle, index, &result);
        return result;
    }

    public void GetWorldSpaceContactPointOn2(uint index, out Vector3 result)
    {
        Unsafe.SkipInit(out result);
        fixed (Vector3* resultPtr = &result)
            JPH_ContactManifold_GetWorldSpaceContactPointOn2(Handle, index, resultPtr);
    }

    public void GetWorldSpaceContactPointOn2(uint index, Vector3* result)
    {
        JPH_ContactManifold_GetWorldSpaceContactPointOn2(Handle, index, result);
    }
}
