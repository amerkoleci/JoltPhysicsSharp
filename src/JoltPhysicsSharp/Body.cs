// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct Body(IntPtr handle) : IEquatable<Body>
{
    public IntPtr Handle { get; } = handle; public bool IsNull => Handle == IntPtr.Zero;
    public static Body Null => new(IntPtr.Zero);
    public static implicit operator Body(IntPtr handle) => new(handle);
    public static bool operator ==(Body left, Body right) => left.Handle == right.Handle;
    public static bool operator !=(Body left, Body right) => left.Handle != right.Handle;
    public static bool operator ==(Body left, IntPtr right) => left.Handle == right;
    public static bool operator !=(Body left, IntPtr right) => left.Handle != right;
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

    public Double3 Position
    {
        get
        {
            JPH_Body_GetPosition(Handle, out Double3 value);
            return value;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            JPH_Body_GetRotation(Handle, out Quaternion value);
            return value;
        }
    }

    public Double3 CenterOfMassPosition
    {
        get
        {
            JPH_Body_GetCenterOfMassPosition(Handle, out Double3 value);
            return value;
        }
    }

    public Matrix4x4 WorldTransform
    {
        get
        {
            JPH_Body_GetWorldTransform(Handle, out Matrix4x4 value);
            return value;
        }
    }

    public Matrix4x4 CenterOfMassTransform
    {
        get
        {
            JPH_Body_GetCenterOfMassTransform(Handle, out Matrix4x4 value);
            return value;
        }
    }

    public bool GetUseManifoldReductionWithBody(in Body other)
    {
        return JPH_Body_GetUseManifoldReductionWithBody(Handle, other.Handle);
    }

    public void GetPosition(out Double3 result)
    {
        JPH_Body_GetPosition(Handle, out result);
    }

    public void GetRotation(out Quaternion result)
    {
        JPH_Body_GetRotation(Handle, out result);
    }

    public void GetCenterOfMassPosition(out Double3 result)
    {
        JPH_Body_GetCenterOfMassPosition(Handle, out result);
    }

    public void GetWorldTransform(out Matrix4x4 result)
    {
        JPH_Body_GetWorldTransform(Handle, out result);
    }

    public void GetCenterOfMassTransform(out Matrix4x4 result)
    {
        JPH_Body_GetCenterOfMassTransform(Handle, out result);
    }

    public Vector3 GetLinearVelocity()
    {
        JPH_Body_GetLinearVelocity(Handle, out Vector3 velocity);
        return velocity;
    }

    public void GetLinearVelocity(out Vector3 velocity)
    {
        JPH_Body_GetLinearVelocity(Handle, out velocity);
    }

    public void SetLinearVelocity(in Vector3 velocity)
    {
        JPH_Body_SetLinearVelocity(Handle, velocity);
    }

    public Vector3 GetAngularVelocity()
    {
        JPH_Body_GetAngularVelocity(Handle, out Vector3 velocity);
        return velocity;
    }

    public void GetAngularVelocity(out Vector3 velocity)
    {
        JPH_Body_GetAngularVelocity(Handle, out velocity);
    }

    public void SetAngularVelocity(in Vector3 velocity)
    {
        JPH_Body_SetAngularVelocity(Handle, velocity);
    }

    public Vector3 GetAccumulatedForce()
    {
        JPH_Body_GetAccumulatedForce(Handle, out Vector3 force);
        return force;
    }

    public void GetAccumulatedForce(out Vector3 force)
    {
        JPH_Body_GetAccumulatedForce(Handle, out force);
    }

    public Vector3 GetAccumulatedTorque()
    {
        JPH_Body_GetAccumulatedTorque(Handle, out Vector3 torque);
        return torque;
    }

    public void GetAccumulatedTorque(out Vector3 torque)
    {
        JPH_Body_GetAccumulatedTorque(Handle, out torque);
    }

    public void AddForce(in Vector3 force)
    {
        JPH_Body_AddForce(Handle, force);
    }

    public void AddForceAtPosition(in Vector3 force, in Double3 position)
    {
        JPH_Body_AddForceAtPosition(Handle, force, position);
    }

    public void AddTorque(in Vector3 torque)
    {
        JPH_Body_AddTorque(Handle, torque);
    }

    public void AddImpulse(in Vector3 impulse)
    {
        JPH_Body_AddImpulse(Handle, impulse);
    }

    public void AddImpulseAtPosition(in Vector3 impulse, in Double3 position)
    {
        JPH_Body_AddImpulseAtPosition(Handle, in impulse, in position);
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
