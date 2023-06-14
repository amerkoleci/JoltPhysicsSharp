// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct Body : IEquatable<Body>
{
    public Body(IntPtr handle) { Handle = handle; }
    public IntPtr Handle { get; }
    public bool IsNull => Handle == IntPtr.Zero;
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
    public readonly bool IsActive => JPH_Body_IsActive(Handle) == 1;
    public readonly bool IsStatic => JPH_Body_IsStatic(Handle) == 1;
    public readonly bool IsKinematic => JPH_Body_IsKinematic(Handle) == 1;
    public readonly bool IsDynamic => JPH_Body_IsDynamic(Handle) == 1;

    public bool IsSensor
    {
        readonly get => JPH_Body_IsSensor(Handle) == 1;
        set => JPH_Body_SetIsSensor(Handle, (uint)(value ? 1 : 0));
    }

    public MotionType MotionType
    {
        readonly get => JPH_Body_GetMotionType(Handle);
        set => JPH_Body_SetMotionType(Handle, value);
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
            JPH_Body_GetPosition(Handle, out Vector3 value);
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

    public Vector3 CenterOfMassPosition
    {
        get
        {
            JPH_Body_GetCenterOfMassPosition(Handle, out Vector3 value);
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

    public void GetPosition(out Vector3 result)
    {
        JPH_Body_GetPosition(Handle, out result);
    }

    public void GetRotation(out Quaternion result)
    {
        JPH_Body_GetRotation(Handle, out result);
    }

    public void GetCenterOfMassPosition(out Vector3 result)
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

    public void AddForceAtPosition(in Vector3 force, in Vector3 position)
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

    public void AddImpulseAtPosition(in Vector3 impulse, in Vector3 position)
    {
        JPH_Body_AddImpulseAtPosition(Handle, impulse, position);
    }

    public void AddAngularImpulse(in Vector3 angularImpulse)
    {
        JPH_Body_AddAngularImpulse(Handle, angularImpulse);
    }
}
