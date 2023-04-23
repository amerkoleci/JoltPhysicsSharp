// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct BodyInterface : IEquatable<BodyInterface>
{
    public BodyInterface(IntPtr handle) { Handle = handle; }
    public IntPtr Handle { get; }
    public bool IsNull => Handle == IntPtr.Zero;
    public static BodyInterface Null => new(IntPtr.Zero);
    public static implicit operator BodyInterface(IntPtr handle) => new(handle);
    public static bool operator ==(BodyInterface left, BodyInterface right) => left.Handle == right.Handle;
    public static bool operator !=(BodyInterface left, BodyInterface right) => left.Handle != right.Handle;
    public static bool operator ==(BodyInterface left, IntPtr right) => left.Handle == right;
    public static bool operator !=(BodyInterface left, IntPtr right) => left.Handle != right;
    public bool Equals(BodyInterface other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BodyInterface handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public Body CreateBody(BodyCreationSettings settings)
    {
        return JPH_BodyInterface_CreateBody(Handle, settings.Handle);
    }

    public BodyID CreateAndAddBody(BodyCreationSettings settings, ActivationMode activationMode)
    {
        return new(JPH_BodyInterface_CreateAndAddBody(Handle, settings.Handle, activationMode));
    }

    public Body CreateBodyWithID(in BodyID bodyID, BodyCreationSettings settings)
    {
        return JPH_BodyInterface_CreateBodyWithID(Handle, bodyID, settings.Handle);
    }

    public Body CreateBodyWithoutID(BodyCreationSettings settings)
    {
        return JPH_BodyInterface_CreateBodyWithoutID(Handle, settings.Handle);
    }

    public void DestroyBody(in BodyID bodyID)
    {
        JPH_BodyInterface_DestroyBody(Handle, bodyID);
    }

    public void DestroyBodyWithoutID(in Body body)
    {
        JPH_BodyInterface_DestroyBodyWithoutID(Handle, body.Handle);
    }

    public void AddBody(in BodyID bodyID, ActivationMode activationMode)
    {
        JPH_BodyInterface_AddBody(Handle, bodyID, activationMode);
    }

    public void AddBody(in Body body, ActivationMode activationMode)
    {
        JPH_BodyInterface_AddBody(Handle, body.ID, activationMode);
    }

    public void RemoveBody(in BodyID bodyID)
    {
        JPH_BodyInterface_RemoveBody(Handle, bodyID);
    }

    public bool AssignBodyID(in Body body)
    {
        return JPH_BodyInterface_AssignBodyID(Handle, body.Handle) == 1;
    }

    public bool AssignBodyID(in Body body, in BodyID bodyID)
    {
        return JPH_BodyInterface_AssignBodyID2(Handle, body.Handle, bodyID) == 1;
    }

    public Body UnassignBodyID(in BodyID bodyID)
    {
        return JPH_BodyInterface_UnassignBodyID(Handle, bodyID);
    }

    public Vector3 GetLinearVelocity(in BodyID bodyID)
    {
        JPH_BodyInterface_GetLinearVelocity(Handle, bodyID, out Vector3 velocity);
        return velocity;
    }

    public void GetLinearVelocity(in BodyID bodyID, out Vector3 velocity)
    {
        JPH_BodyInterface_GetLinearVelocity(Handle, bodyID, out velocity);
    }

    public void SetLinearVelocity(in Body body, in Vector3 velocity)
    {
        JPH_BodyInterface_SetLinearVelocity(Handle, body.ID, velocity);
    }

    public void SetLinearVelocity(in BodyID bodyID, in Vector3 velocity)
    {
        JPH_BodyInterface_SetLinearVelocity(Handle, bodyID, velocity);
    }

    public Vector3 GetCenterOfMassPosition(in BodyID bodyID)
    {
        JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, out Vector3 velocity);
        return velocity;
    }

    public void GetCenterOfMassPosition(in BodyID bodyID, out Vector3 velocity)
    {
        JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, out velocity);
    }

    public MotionType GetMotionType(in BodyID bodyID)
    {
        return JPH_BodyInterface_GetMotionType(Handle, bodyID);
    }

    public void SetMotionType(in BodyID bodyID, MotionType motionType, ActivationMode activationMode)
    {
        JPH_BodyInterface_SetMotionType(Handle, bodyID, motionType, activationMode);
    }

    public bool IsActive(in BodyID bodyID) => JPH_BodyInterface_IsActive(Handle, bodyID) == 1;
    public bool IsAdded(in BodyID bodyID) => JPH_BodyInterface_IsAdded(Handle, bodyID) == 1;

    public float GetRestitution(in BodyID bodyID)
    {
        return JPH_BodyInterface_GetRestitution(Handle, bodyID);
    }

    public void SetRestitution(in BodyID bodyID, float restitution)
    {
        JPH_BodyInterface_SetRestitution(Handle, bodyID, restitution);
    }

    public float GetFriction(in BodyID bodyID)
    {
        return JPH_BodyInterface_GetFriction(Handle, bodyID);
    }

    public void SetFriction(in BodyID bodyID, float friction)
    {
        JPH_BodyInterface_SetFriction(Handle, bodyID, friction);
    }

    public void SetPosition(in BodyID bodyID, in Vector3 position, ActivationMode activationMode)
    {
        JPH_BodyInterface_SetPosition(Handle, bodyID, position, activationMode);
    }

    public Vector3 GetPosition(in BodyID bodyID)
    {
        JPH_BodyInterface_GetPosition(Handle, bodyID, out Vector3 position);
        return position;
    }

    public void SetRotation(in BodyID bodyID, in Quaternion rotation, ActivationMode activationMode)
    {
        JPH_BodyInterface_SetRotation(Handle, bodyID, rotation, activationMode);
    }

    public Quaternion GetRotation(in BodyID bodyID)
    {
        JPH_BodyInterface_GetRotation(Handle, bodyID, out Quaternion rotation);
        return rotation;
    }

    public void SetPositionAndRotation(in BodyID bodyID, in Vector3 position, in Quaternion rotation, ActivationMode activationMode)
    {
        JPH_BodyInterface_SetPositionAndRotation(Handle, bodyID, position, rotation, activationMode);
    }

    public void SetPositionAndRotationWhenChanged(in BodyID bodyID, in Vector3 position, in Quaternion rotation, ActivationMode activationMode)
    {
        JPH_BodyInterface_SetPositionAndRotationWhenChanged(Handle, bodyID, position, rotation, activationMode);
    }

    public void SetPositionRotationAndVelocity(in BodyID bodyID, in Vector3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity)
    {
        JPH_BodyInterface_SetPositionRotationAndVelocity(Handle, bodyID, position, rotation, linearVelocity, angularVelocity);
    }

    public void SetShape(in BodyID bodyId, in Shape shape, bool updateMassProperties, ActivationMode activationMode)
    {
        JPH_BodyInterface_SetShape(Handle, bodyId, shape.Handle, updateMassProperties, activationMode);
    }
}
