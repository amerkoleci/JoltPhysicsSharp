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

    public Body CreateSoftBody(SoftBodyCreationSettings settings)
    {
        return JPH_BodyInterface_CreateSoftBody(Handle, settings.Handle);
    }

    public BodyID CreateAndAddBody(BodyCreationSettings settings, Activation activationMode)
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

    public void AddBody(in BodyID bodyID, Activation activationMode)
    {
        JPH_BodyInterface_AddBody(Handle, bodyID, activationMode);
    }

    public void AddBody(in Body body, Activation activationMode)
    {
        JPH_BodyInterface_AddBody(Handle, body.ID, activationMode);
    }

    public void RemoveBody(in BodyID bodyID)
    {
        JPH_BodyInterface_RemoveBody(Handle, bodyID);
    }

    public bool AssignBodyID(in Body body)
    {
        return JPH_BodyInterface_AssignBodyID(Handle, body.Handle);
    }

    public bool AssignBodyID(in Body body, in BodyID bodyID)
    {
        return JPH_BodyInterface_AssignBodyID2(Handle, body.Handle, bodyID);
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

    public Double3 GetCenterOfMassPosition(in BodyID bodyID)
    {
        JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, out Double3 value);
        return value;
    }

    public void GetCenterOfMassPosition(in BodyID bodyID, out Double3 position)
    {
        JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, out position);
    }

    public MotionType GetMotionType(in BodyID bodyID)
    {
        return JPH_BodyInterface_GetMotionType(Handle, bodyID);
    }

    public void SetMotionType(in BodyID bodyID, MotionType motionType, Activation activationMode)
    {
        JPH_BodyInterface_SetMotionType(Handle, bodyID, motionType, activationMode);
    }

    public MotionQuality GetMotionQuality(in BodyID bodyID)
    {
        return JPH_BodyInterface_GetMotionQuality(Handle, bodyID);
    }

    public void SetMotionQuality(in BodyID bodyID, MotionQuality quality)
    {
        JPH_BodyInterface_SetMotionQuality(Handle, bodyID, quality);
    }

    public bool IsActive(in BodyID bodyID) => JPH_BodyInterface_IsActive(Handle, bodyID);
    public bool IsAdded(in BodyID bodyID) => JPH_BodyInterface_IsAdded(Handle, bodyID);
    public BodyType GetBodyType(in BodyID bodyID) => JPH_BodyInterface_GetBodyType(Handle, bodyID);

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

    public void SetPosition(in BodyID bodyID, in Double3 position, Activation activationMode)
    {
        JPH_BodyInterface_SetPosition(Handle, bodyID, position, activationMode);
    }

    public Double3 GetPosition(in BodyID bodyID)
    {
        JPH_BodyInterface_GetPosition(Handle, bodyID, out Double3 position);
        return position;
    }

    public void GetPosition(in BodyID bodyID, out Double3 position)
    {
        JPH_BodyInterface_GetPosition(Handle, bodyID, out position);
    }

    public void SetRotation(in BodyID bodyID, in Quaternion rotation, Activation activationMode)
    {
        JPH_BodyInterface_SetRotation(Handle, bodyID, rotation, activationMode);
    }

    public Quaternion GetRotation(in BodyID bodyID)
    {
        JPH_BodyInterface_GetRotation(Handle, bodyID, out Quaternion rotation);
        return rotation;
    }

    public void SetPositionAndRotation(in BodyID bodyID, in Double3 position, in Quaternion rotation, Activation activationMode)
    {
        JPH_BodyInterface_SetPositionAndRotation(Handle, bodyID, position, rotation, activationMode);
    }

    public void SetPositionAndRotationWhenChanged(in BodyID bodyID, in Double3 position, in Quaternion rotation, Activation activationMode)
    {
        JPH_BodyInterface_SetPositionAndRotationWhenChanged(Handle, bodyID, position, rotation, activationMode);
    }

    public void SetPositionRotationAndVelocity(in BodyID bodyID, in Double3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity)
    {
        JPH_BodyInterface_SetPositionRotationAndVelocity(Handle, bodyID, position, rotation, linearVelocity, angularVelocity);
    }

    public void SetShape(in BodyID bodyId, in Shape shape, bool updateMassProperties, Activation activationMode)
    {
        JPH_BodyInterface_SetShape(Handle, bodyId, shape.Handle, updateMassProperties, activationMode);
    }

    public void NotifyShapeChanged(in BodyID bodyId, in Vector3 previousCenterOfMass, bool updateMassProperties, Activation activationMode)
    {

    }

    public void ActivateBody(in BodyID bodyId)
    {
        JPH_BodyInterface_ActivateBody(Handle, bodyId);
    }

    public void DeactivateBody(in BodyID bodyId)
    {
        JPH_BodyInterface_DeactivateBody(Handle, bodyId);
    }

    public void SetObjectLayer(in BodyID bodyId, in ObjectLayer layer)
    {
        JPH_BodyInterface_SetObjectLayer(Handle, bodyId, layer.Value);
    }

    public ObjectLayer GetObjectLayer(in BodyID bodyId)
    {
        return new ObjectLayer(JPH_BodyInterface_GetObjectLayer(Handle, bodyId));
    }

    public Matrix4x4 GetWorldTransform(in BodyID bodyID)
    {
        JPH_BodyInterface_GetWorldTransform(Handle, bodyID, out Matrix4x4 result);
        return result;
    }

    public Matrix4x4 GetCenterOfMassTransform(in BodyID bodyId)
    {
        JPH_BodyInterface_GetCenterOfMassTransform(Handle, bodyId, out Matrix4x4 result);
        return result;
    }

    public void MoveKinematic(in BodyID bodyId, in Double3 targetPosition, in Quaternion targetRotation, float deltaTime)
    {
        JPH_BodyInterface_MoveKinematic(Handle, bodyId, targetPosition, targetRotation, deltaTime);
    }

    public void SetLinearAndAngularVelocity(in BodyID bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity)
    {
        JPH_BodyInterface_SetLinearAndAngularVelocity(Handle, bodyId, linearVelocity, angularVelocity);
    }

    public void GetLinearAndAngularVelocity(in BodyID bodyId, out Vector3 linearVelocity, out Vector3 angularVelocity)
    {
        JPH_BodyInterface_GetLinearAndAngularVelocity(Handle, bodyId, out linearVelocity, out angularVelocity);
    }

    public void AddLinearVelocity(in BodyID bodyId, in Vector3 linearVelocity)
    {
        JPH_BodyInterface_AddLinearVelocity(Handle, bodyId, linearVelocity);
    }

    public void AddLinearAndAngularVelocity(in BodyID bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity)
    {
        JPH_BodyInterface_AddLinearAndAngularVelocity(Handle, bodyId, linearVelocity, angularVelocity);
    }

    public void SetAngularVelocity(in BodyID bodyId, in Vector3 angularVelocity)
    {
        JPH_BodyInterface_SetAngularVelocity(Handle, bodyId, angularVelocity);
    }

    public Vector3 GetAngularVelocity(in BodyID bodyId)
    {
        JPH_BodyInterface_GetAngularVelocity(Handle, bodyId, out Vector3 result);
        return result;
    }

    public Vector3 GetPointVelocity(in BodyID bodyId, in Double3 point)
    {
        JPH_BodyInterface_GetPointVelocity(Handle, bodyId, point, out Vector3 result);
        return result;
    }

    public void AddForce(in BodyID bodyId, in Vector3 force)
    {
        JPH_BodyInterface_AddForce(Handle, bodyId, force);
    }

    public void AddForce(in BodyID bodyId, in Vector3 force, in Double3 point)
    {
        JPH_BodyInterface_AddForce2(Handle, bodyId, force, point);
    }

    public void AddTorque(in BodyID bodyId, in Vector3 torque)
    {
        JPH_BodyInterface_AddTorque(Handle, bodyId, torque);
    }

    public void AddForceAndTorque(in BodyID bodyId, in Vector3 force, in Vector3 torque)
    {
        JPH_BodyInterface_AddForceAndTorque(Handle, bodyId, force, torque);
    }

    public void AddImpulse(in BodyID bodyId, in Vector3 impulse)
    {
        JPH_BodyInterface_AddImpulse(Handle, bodyId, impulse);
    }

    public void AddImpulse(in BodyID bodyId, in Vector3 impulse, in Double3 point)
    {
        JPH_BodyInterface_AddImpulse2(Handle, bodyId, impulse, point);
    }

    public void AddAngularImpulse(in BodyID bodyId, in Vector3 angularImpulse)
    {
        JPH_BodyInterface_AddAngularImpulse(Handle, bodyId, angularImpulse);
    }

    public Matrix4x4 GetInverseInertia(in BodyID bodyId)
    {
        JPH_BodyInterface_GetInverseInertia(Handle, bodyId, out Matrix4x4 result);
        return result;
    }

    public void SetGravityFactor(in BodyID bodyId, float gravityFactor)
    {
        JPH_BodyInterface_SetGravityFactor(Handle, bodyId, gravityFactor);
    }

    public float GetGravityFactor(in BodyID bodyId)
    {
        return JPH_BodyInterface_GetGravityFactor(Handle, bodyId);
    }

    public void InvalidateContactCache(in BodyID bodyId)
    {
        JPH_BodyInterface_InvalidateContactCache(Handle, bodyId);
    }

    public ulong GetUserData(in BodyID bodyId)
    {
        return JPH_BodyInterface_GetUserData(Handle, bodyId);
    }

    public void SetUserData(in BodyID bodyId, ulong userData)
    {
        JPH_BodyInterface_SetUserData(Handle, bodyId, userData);
    }
}
