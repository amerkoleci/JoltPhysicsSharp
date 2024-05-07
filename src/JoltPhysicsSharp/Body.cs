// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly unsafe struct Body(nint handle) : IEquatable<Body>
{
    public nint Handle { get; } = handle; public bool IsNull => Handle == 0;
    public static Body Null => new(0);
    public static implicit operator Body(nint handle) => new(handle);
    public static bool operator ==(Body left, Body right) => left.Handle == right.Handle;
    public static bool operator !=(Body left, Body right) => left.Handle != right.Handle;
    public static bool operator ==(Body left, nint right) => left.Handle == right;
    public static bool operator !=(Body left, nint right) => left.Handle != right;
    public bool Equals(Body other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Body handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public readonly BodyID ID => JPH_Body_GetID(Handle);

    /// <summary>
    /// Get the type of body (rigid or soft)
    /// </summary>
    public readonly BodyType BodyType => JPH_Body_GetBodyType(Handle);

    /// <summary>
    /// Gets where this body is a rigid body.
    /// </summary>
    public readonly bool IsRigidBody => JPH_Body_GetBodyType(Handle) == BodyType.Rigid;

    /// <summary>
    /// Gets where this body is a soft body.
    /// </summary>
    public readonly bool IsSoftBody => JPH_Body_GetBodyType(Handle) == BodyType.Soft;
    public readonly bool IsActive => JPH_Body_IsActive(Handle);
    public readonly bool IsStatic => JPH_Body_IsStatic(Handle);
    public readonly bool IsKinematic => JPH_Body_IsKinematic(Handle);
    public readonly bool IsDynamic => JPH_Body_IsDynamic(Handle);

    public bool IsSensor
    {
        readonly get => JPH_Body_IsSensor(Handle);
        set => JPH_Body_SetIsSensor(Handle, value);
    }

    public bool CollideKinematicVsNonDynamic
    {
        readonly get => JPH_Body_GetCollideKinematicVsNonDynamic(Handle);
        set => JPH_Body_SetCollideKinematicVsNonDynamic(Handle, value);
    }

    public bool UseManifoldReduction
    {
        readonly get => JPH_Body_GetUseManifoldReduction(Handle);
        set => JPH_Body_SetUseManifoldReduction(Handle, value);
    }

    public bool ApplyGyroscopicForce
    {
        readonly get => JPH_Body_GetApplyGyroscopicForce(Handle);
        set => JPH_Body_SetApplyGyroscopicForce(Handle, value);
    }

    public unsafe BoundingBox WorldSpaceBounds
    {
        get
        {
            BoundingBox result = default;
            JPH_Body_GetWorldSpaceBounds(Handle, &result);
            return result;
        }
    }

    public MotionProperties MotionProperties => JPH_Body_GetMotionProperties(Handle);

    public MotionType MotionType
    {
        readonly get => JPH_Body_GetMotionType(Handle);
        set => JPH_Body_SetMotionType(Handle, value);
    }

    public bool AllowSleeping
    {
        readonly get => JPH_Body_GetAllowSleeping(Handle);
        set => JPH_Body_SetAllowSleeping(Handle, value);
    }

    public float Friction
    {
        readonly get => JPH_Body_GetFriction(Handle);
        set => JPH_Body_SetFriction(Handle, value);
    }

    public float Restitution
    {
        readonly get => JPH_Body_GetRestitution(Handle);
        set => JPH_Body_SetRestitution(Handle, value);
    }

    public Vector3 Position
    {
        get
        {
            if (DoublePrecision)
                throw new InvalidOperationException($"Double precision is enabled: use {nameof(RPosition)}");

            Vector3 result;
            JPH_Body_GetPosition(Handle, &result);
            return result;
        }
    }

    public Double3 RPosition
    {
        get
        {
            if (!DoublePrecision)
                throw new InvalidOperationException($"Double precision is disabled: use {nameof(Position)}");

            Double3 result;
            JPH_Body_GetPositionDouble(Handle, &result);
            return result;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            Quaternion value;
            JPH_Body_GetRotation(Handle, &value);
            return value;
        }
    }

    public Vector3 CenterOfMassPosition
    {
        get
        {
            if (DoublePrecision)
                throw new InvalidOperationException($"Double precision is enabled: use {nameof(RCenterOfMassPosition)}");

            Vector3 value;
            JPH_Body_GetCenterOfMassPosition(Handle, &value);
            return value;
        }
    }

    public Double3 RCenterOfMassPosition
    {
        get
        {
            if (!DoublePrecision)
                throw new InvalidOperationException($"Double precision is disabled: use {nameof(CenterOfMassPosition)}");

            Double3 value;
            JPH_Body_GetCenterOfMassPositionDouble(Handle, &value);
            return value;
        }
    }

    public bool GetUseManifoldReductionWithBody(in Body other)
    {
        return JPH_Body_GetUseManifoldReductionWithBody(Handle, other.Handle);
    }

    public void GetPosition(out Vector3 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPosition)}");

        Unsafe.SkipInit(out result);
        fixed (Vector3* resultPtr = &result)
            JPH_Body_GetPosition(Handle, resultPtr);
    }

    public void GetRPosition(out Double3 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPosition)}");

        Unsafe.SkipInit(out result);
        fixed (Double3* resultPtr = &result)
        {
            JPH_Body_GetPositionDouble(Handle, resultPtr);
        }
    }

    public void GetRotation(out Quaternion result)
    {
        Unsafe.SkipInit(out result);
        fixed (Quaternion* resultPtr = &result)
            JPH_Body_GetRotation(Handle, resultPtr);
    }

    public void GetCenterOfMassPosition(out Vector3 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassPosition)}");

        Unsafe.SkipInit(out result);
        fixed (Vector3* resultPtr = &result)
        {
            JPH_Body_GetCenterOfMassPosition(Handle, resultPtr);
        }
    }

    public void GetRCenterOfMassPosition(out Double3 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassPosition)}");

        Unsafe.SkipInit(out result);
        fixed (Double3* resultPtr = &result)
        {
            JPH_Body_GetCenterOfMassPositionDouble(Handle, resultPtr);
        }
    }

    #region GetWorldTransform
    public unsafe Matrix4x4 GetWorldTransform()
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Matrix4x4 result;
        JPH_Body_GetWorldTransform(Handle, &result);
        return result;
    }

    public unsafe void GetWorldTransform(out Matrix4x4 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Unsafe.SkipInit(out result);
        fixed (Matrix4x4* resultPtr = &result)
        {
            JPH_Body_GetWorldTransform(Handle, resultPtr);
        }
    }

    public unsafe RMatrix4x4 GetRWorldTransform()
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        RMatrix4x4 result;
        JPH_Body_GetWorldTransformDouble(Handle, &result);
        return result;
    }

    public unsafe void GetRWorldTransform(out RMatrix4x4 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        Unsafe.SkipInit(out result);
        fixed (RMatrix4x4* resultPtr = &result)
        {
            JPH_Body_GetWorldTransformDouble(Handle, resultPtr);
        }
    }
    #endregion

    #region GetCenterOfMassTransform
    public unsafe Matrix4x4 GetCenterOfMassTransform()
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassTransform)}");

        Matrix4x4 result;
        JPH_Body_GetCenterOfMassTransform(Handle, &result);
        return result;
    }

    public unsafe void GetCenterOfMassTransform(out Matrix4x4 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassTransform)}");

        Unsafe.SkipInit(out result);
        fixed (Matrix4x4* resultPtr = &result)
        {
            JPH_Body_GetCenterOfMassTransform(Handle, resultPtr);
        }
    }

    public unsafe RMatrix4x4 GetRCenterOfMassTransform()
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassTransform)}");

        RMatrix4x4 result;
        JPH_Body_GetCenterOfMassTransformDouble(Handle, &result);
        return result;
    }

    public unsafe void GetRCenterOfMassTransform(out RMatrix4x4 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassTransform)}");

        Unsafe.SkipInit(out result);
        fixed (RMatrix4x4* resultPtr = &result)
        {
            JPH_Body_GetCenterOfMassTransformDouble(Handle, resultPtr);
        }
    }
    #endregion

    public Vector3 GetLinearVelocity()
    {
        Vector3 velocity;
        JPH_Body_GetLinearVelocity(Handle, &velocity);
        return velocity;
    }

    public void GetLinearVelocity(out Vector3 velocity)
    {
        Unsafe.SkipInit(out velocity);

        fixed (Vector3* velocityPtr = &velocity)
            JPH_Body_GetLinearVelocity(Handle, velocityPtr);
    }

    public void SetLinearVelocity(in Vector3 velocity)
    {
        fixed (Vector3* velocityPtr = &velocity)
            JPH_Body_SetLinearVelocity(Handle, velocityPtr);
    }

    public Vector3 GetAngularVelocity()
    {
        Vector3 velocity;
        JPH_Body_GetAngularVelocity(Handle, &velocity);
        return velocity;
    }

    public void GetAngularVelocity(out Vector3 velocity)
    {
        Unsafe.SkipInit(out velocity);

        fixed (Vector3* velocityPtr = &velocity)
            JPH_Body_GetAngularVelocity(Handle, velocityPtr);
    }

    public void SetAngularVelocity(in Vector3 velocity)
    {
        fixed (Vector3* velocityPtr = &velocity)
            JPH_Body_SetAngularVelocity(Handle, velocityPtr);
    }

    public Vector3 GetAccumulatedForce()
    {
        Vector3 force;
        JPH_Body_GetAccumulatedForce(Handle, &force);
        return force;
    }

    public void GetAccumulatedForce(out Vector3 force)
    {
        Unsafe.SkipInit(out force);

        fixed (Vector3* forcePtr = &force)
            JPH_Body_GetAccumulatedForce(Handle, forcePtr);
    }

    public Vector3 GetAccumulatedTorque()
    {
        Vector3 torque;
        JPH_Body_GetAccumulatedTorque(Handle, &torque);
        return torque;
    }

    public void GetAccumulatedTorque(out Vector3 torque)
    {
        Unsafe.SkipInit(out torque);

        fixed (Vector3* torquePtr = &torque)
            JPH_Body_GetAccumulatedTorque(Handle, torquePtr);
    }

    public void GetWorldSpaceSurfaceNormal(SubShapeID subShapeID, in Vector3 position, out Vector3 normal)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldSpaceSurfaceNormal)}");

        Unsafe.SkipInit(out normal);

        fixed (Vector3* positionPtr = &position)
        fixed (Vector3* normalPtr = &normal)
        {
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID, positionPtr, normalPtr);
        }
    }

    public void GetRWorldSpaceSurfaceNormal(SubShapeID subShapeID, in Double3 position, out Vector3 normal)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldSpaceSurfaceNormal)}");

        Unsafe.SkipInit(out normal);
        fixed (Double3* positionPtr = &position)
        fixed (Vector3* normalPtr = &normal)
        {
            JPH_Body_GetWorldSpaceSurfaceNormalDouble(Handle, subShapeID, positionPtr, normalPtr);
        }
    }

    public void AddForce(in Vector3 force)
    {
        fixed (Vector3* forcePtr = &force)
            JPH_Body_AddForce(Handle, forcePtr);
    }

    public void AddForceAtPosition(in Vector3 force, in Vector3 position)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(AddForceAtPosition)}");

        fixed (Vector3* forcePtr = &force)
        fixed (Vector3* positionPtr = &position)
            JPH_Body_AddForceAtPosition(Handle, forcePtr, positionPtr);
    }

    public void AddForceAtPosition(in Vector3 force, in Double3 position)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(AddForceAtPosition)}");

        fixed (Vector3* forcePtr = &force)
        fixed (Double3* positionPtr = &position)
            JPH_Body_AddForceAtPositionDouble(Handle, forcePtr, positionPtr);
    }

    public void AddTorque(in Vector3 torque)
    {
        fixed (Vector3* torquePtr = &torque)
            JPH_Body_AddTorque(Handle, torquePtr);
    }

    public void AddImpulse(in Vector3 impulse)
    {
        JPH_Body_AddImpulse(Handle, impulse);
    }

    public void AddImpulseAtPosition(in Vector3 impulse, in Vector3 position)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(AddImpulseAtPosition)}");

        fixed (Vector3* impulsePtr = &impulse)
        fixed (Vector3* positionPtr = &position)
            JPH_Body_AddImpulseAtPosition(Handle, impulsePtr, positionPtr);
    }

    public void AddImpulseAtPosition(in Vector3 impulse, in Double3 position)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(AddImpulseAtPosition)}");

        fixed (Vector3* impulsePtr = &impulse)
        fixed (Double3* positionPtr = &position)
            JPH_Body_AddImpulseAtPositionDouble(Handle, impulsePtr, positionPtr);
    }

    public void AddAngularImpulse(in Vector3 angularImpulse)
    {
        JPH_Body_AddAngularImpulse(Handle, angularImpulse);
    }

    public ulong GetUserData()
    {
        return JPH_Body_GetUserData(Handle);
    }

    public void SetUserData(ulong userData)
    {
        JPH_Body_SetUserData(Handle, userData);
    }
}
