// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly unsafe struct MotionProperties(nint handle) : IEquatable<MotionProperties>
{
    public nint Handle { get; } = handle;
    public readonly bool IsNull => Handle == 0;
    public readonly bool IsNotNull => Handle != 0;
    public static MotionProperties Null => new(0);
    public static implicit operator MotionProperties(nint handle) => new(handle);
    public static bool operator ==(MotionProperties left, MotionProperties right) => left.Handle == right.Handle;
    public static bool operator !=(MotionProperties left, MotionProperties right) => left.Handle != right.Handle;
    public static bool operator ==(MotionProperties left, nint right) => left.Handle == right;
    public static bool operator !=(MotionProperties left, nint right) => left.Handle != right;
    public bool Equals(MotionProperties other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MotionProperties handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public AllowedDOFs AllowedDOFs
    {
        get => JPH_MotionProperties_GetAllowedDOFs(Handle);
    }

    public float LinearDamping
    {
        readonly get => JPH_MotionProperties_GetLinearDamping(Handle);
        set => JPH_MotionProperties_SetLinearDamping(Handle, value);
    }

    public float AngularDamping
    {
        readonly get => JPH_MotionProperties_GetAngularDamping(Handle);
        set => JPH_MotionProperties_SetAngularDamping(Handle, value);
    }

    public float InverseMassUnchecked
    {
        get => JPH_MotionProperties_GetInverseMassUnchecked(Handle);
    }

    public Vector3 InverseInertiaDiagonal
    {
        get
        {
            Vector3 result;
            JPH_MotionProperties_GetInverseInertiaDiagonal(Handle, &result);
            return result;
        }
    }

    public Quaternion InertiaRotation
    {
        get
        {
            Quaternion result;
            JPH_MotionProperties_GetInertiaRotation(Handle, &result);
            return result;
        }
    }

    public void SetMassProperties(AllowedDOFs allowedDOFs, in MassProperties massProperties)
    {
        JPH_MotionProperties_SetMassProperties(Handle, allowedDOFs, in massProperties);
    }

    public void SetInverseMass(float inverseMass)
    {
        JPH_MotionProperties_SetInverseMass(Handle, inverseMass);
    }

    public void SetInverseInertia(in Vector3 diagonal, in Quaternion rotation)
    {
        fixed (Vector3* diagonalPtr = &diagonal)
        fixed (Quaternion* rotationPtr = &rotation)
            JPH_MotionProperties_SetInverseInertia(Handle, diagonalPtr, rotationPtr);
    }

    public void ScaleToMass(float mass)
    {
        JPH_MotionProperties_ScaleToMass(Handle, mass);
    }
}
