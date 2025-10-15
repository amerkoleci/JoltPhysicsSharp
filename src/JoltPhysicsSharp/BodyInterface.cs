// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly unsafe struct BodyInterface(nint handle) : IEquatable<BodyInterface>
{
    public nint Handle { get; } = handle;
    public bool IsNull => Handle == 0;
    public static BodyInterface Null => new(0);
    public static implicit operator BodyInterface(nint handle) => new(handle);
    public static bool operator ==(BodyInterface left, BodyInterface right) => left.Handle == right.Handle;
    public static bool operator !=(BodyInterface left, BodyInterface right) => left.Handle != right.Handle;
    public static bool operator ==(BodyInterface left, nint right) => left.Handle == right;
    public static bool operator !=(BodyInterface left, nint right) => left.Handle != right;
    public bool Equals(BodyInterface other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BodyInterface handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public Body CreateBody(BodyCreationSettings settings)
    {
        return new(JPH_BodyInterface_CreateBody(Handle, settings.Handle));
    }

    public Body CreateSoftBody(SoftBodyCreationSettings settings)
    {
        return new(JPH_BodyInterface_CreateSoftBody(Handle, settings.Handle));
    }

    public BodyID CreateAndAddBody(BodyCreationSettings settings, Activation activationMode)
    {
        return new(JPH_BodyInterface_CreateAndAddBody(Handle, settings.Handle, activationMode));
    }

    public Body CreateBodyWithID(in BodyID bodyID, BodyCreationSettings settings)
    {
        return new(JPH_BodyInterface_CreateBodyWithID(Handle, bodyID, settings.Handle));
    }

    public Body CreateBodyWithoutID(BodyCreationSettings settings)
    {
        return new(JPH_BodyInterface_CreateBodyWithoutID(Handle, settings.Handle));
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

    public void RemoveAndDestroyBody(in BodyID bodyID)
    {
        JPH_BodyInterface_RemoveAndDestroyBody(Handle, bodyID);
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
        return Body.GetObject(JPH_BodyInterface_UnassignBodyID(Handle, bodyID))!;
    }

    public Vector3 GetLinearVelocity(in BodyID bodyID)
    {
        Vector3 velocity;
        JPH_BodyInterface_GetLinearVelocity(Handle, bodyID, &velocity);
        return velocity;
    }

    public void GetLinearVelocity(in BodyID bodyID, out Vector3 velocity)
    {
        Unsafe.SkipInit(out velocity);
        fixed (Vector3* velocityPtr = &velocity)
        {
            JPH_BodyInterface_GetLinearVelocity(Handle, bodyID, velocityPtr);
        }
    }

    public void SetLinearVelocity(in Body body, in Vector3 velocity)
    {
        fixed (Vector3* velocityPtr = &velocity)
        {
            JPH_BodyInterface_SetLinearVelocity(Handle, body.ID, velocityPtr);
        }
    }

    public void SetLinearVelocity(in BodyID bodyID, in Vector3 velocity)
    {
        fixed (Vector3* velocityPtr = &velocity)
        {
            JPH_BodyInterface_SetLinearVelocity(Handle, bodyID, velocityPtr);
        }
    }

    public Vector3 GetCenterOfMassPosition(in BodyID bodyID)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassPosition)}");

        Vector3 result;
        JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, &result);
        return result;
    }

    public void GetRCenterOfMassPosition(in BodyID bodyID, out Vector3 position)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassPosition)}");

        Unsafe.SkipInit(out position);
        fixed (Vector3* positionPtr = &position)
        {
            JPH_BodyInterface_GetCenterOfMassPosition(Handle, bodyID, positionPtr);
        }
    }

    public RVector3 GetRCenterOfMassPosition(in BodyID bodyID)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassPosition)}");

        RVector3 result;
        JPH_BodyInterface_GetCenterOfMassPositionDouble(Handle, bodyID, &result);
        return result;
    }

    public void GetRCenterOfMassPosition(in BodyID bodyID, out RVector3 position)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassPosition)}");

        Unsafe.SkipInit(out position);
        fixed (RVector3* positionPtr = &position)
        {
            JPH_BodyInterface_GetCenterOfMassPositionDouble(Handle, bodyID, positionPtr);
        }
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

    public CollisionGroup GetCollisionGroup(in BodyID bodyID)
    {
        JPH_BodyInterface_GetCollisionGroup(Handle, bodyID, out JPH_CollisionGroup groupNative);
        return CollisionGroup.FromNative(groupNative);
    }

    public void SetCollisionGroup(in BodyID bodyId, in CollisionGroup collisionGroup)
    {
        collisionGroup.ToNative(out JPH_CollisionGroup groupNative);
        JPH_BodyInterface_SetCollisionGroup(Handle, bodyId, &groupNative);
    }

    public void SetPosition(in BodyID bodyID, in Vector3 position, Activation activationMode)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(SetRPosition)}");

        JPH_BodyInterface_SetPosition(Handle, bodyID, in position, activationMode);
    }

    public void SetRPosition(in BodyID bodyID, in RVector3 position, Activation activationMode)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(SetPosition)}");

        JPH_BodyInterface_SetPositionDouble(Handle, bodyID, in position, activationMode);
    }

    #region GetPosition
    public Vector3 GetPosition(in BodyID bodyID)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPosition)}");

        JPH_BodyInterface_GetPosition(Handle, bodyID, out Vector3 position);
        return position;
    }

    public void GetPosition(in BodyID bodyID, out Vector3 position)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPosition)}");

        JPH_BodyInterface_GetPosition(Handle, bodyID, out position);
    }

    public RVector3 GetRPosition(in BodyID bodyID)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPosition)}");

        JPH_BodyInterface_GetPositionDouble(Handle, bodyID, out RVector3 position);
        return position;
    }

    public void GetRPosition(in BodyID bodyID, out RVector3 position)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPosition)}");

        JPH_BodyInterface_GetPositionDouble(Handle, bodyID, out position);
    }
    #endregion

    public void SetRotation(in BodyID bodyID, in Quaternion rotation, Activation activationMode)
    {
        JPH_BodyInterface_SetRotation(Handle, bodyID, rotation, activationMode);
    }

    public Quaternion GetRotation(in BodyID bodyID)
    {
        JPH_BodyInterface_GetRotation(Handle, bodyID, out Quaternion rotation);
        return rotation;
    }

    public void SetPositionAndRotation(in BodyID bodyID, in Vector3 position, in Quaternion rotation, Activation activationMode)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(SetRPositionAndRotation)}");

        JPH_BodyInterface_SetPositionAndRotation(Handle, bodyID, in position, in rotation, activationMode);
    }

    public void SetRPositionAndRotation(in BodyID bodyID, in RVector3 position, in Quaternion rotation, Activation activationMode)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(SetPositionAndRotation)}");

        JPH_BodyInterface_SetPositionAndRotation(Handle, bodyID, in position, in rotation, activationMode);
    }

    public void SetPositionAndRotationWhenChanged(in BodyID bodyID, in Vector3 position, in Quaternion rotation, Activation activationMode)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(SetRPositionAndRotationWhenChanged)}");

        JPH_BodyInterface_SetPositionAndRotationWhenChanged(Handle, bodyID, in position, in rotation, activationMode);
    }

    public void SetRPositionAndRotationWhenChanged(in BodyID bodyID, in RVector3 position, in Quaternion rotation, Activation activationMode)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(SetPositionAndRotationWhenChanged)}");

        JPH_BodyInterface_SetPositionAndRotationWhenChanged(Handle, bodyID, in position, in rotation, activationMode);
    }

    public void SetPositionRotationAndVelocity(in BodyID bodyID, in Vector3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(SetRPositionRotationAndVelocity)}");

        fixed (Vector3* linearVelocityPtr = &linearVelocity)
        fixed (Vector3* angularVelocityPtr = &angularVelocity)
            JPH_BodyInterface_SetPositionRotationAndVelocity(Handle, bodyID, in position, in rotation, in linearVelocity, in angularVelocity);
    }

    public void SetRPositionRotationAndVelocity(in BodyID bodyID, in RVector3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(SetPositionRotationAndVelocity)}");

        JPH_BodyInterface_SetPositionRotationAndVelocity(Handle, bodyID, in position, in rotation, in linearVelocity, in angularVelocity);
    }

    public void SetShape(in BodyID bodyId, in Shape shape, bool updateMassProperties, Activation activationMode)
    {
        JPH_BodyInterface_SetShape(Handle, bodyId, shape.Handle, updateMassProperties, activationMode);
    }

    public Shape? GetShape(in BodyID bodyId) => Shape.GetObject(JPH_BodyInterface_GetShape(Handle, bodyId));

    public void NotifyShapeChanged(in BodyID bodyId, in Vector3 previousCenterOfMass, bool updateMassProperties, Activation activationMode)
    {
        JPH_BodyInterface_NotifyShapeChanged(Handle, bodyId, in previousCenterOfMass, updateMassProperties, activationMode);
    }

    public void ActivateBody(in BodyID bodyId)
    {
        JPH_BodyInterface_ActivateBody(Handle, bodyId);
    }

    public void ActivateBodiesInAABox(in BoundingBox box,
        BroadPhaseLayerFilter broadPhaseLayerFilter,
        ObjectLayerFilter objectLayerFilter)
    {
        JPH_BodyInterface_ActivateBodiesInAABox(Handle,
            in box,
            broadPhaseLayerFilter.Handle,
            objectLayerFilter.Handle
            );
    }

    public void ActivateBodies(Span<BodyID> bodyIDs)
    {
        uint* bodyIDsPtr = stackalloc uint[bodyIDs.Length];
        for (int i = 0; i < bodyIDs.Length; i++)
        {
            bodyIDsPtr[i] = bodyIDs[i];
        }

        JPH_BodyInterface_ActivateBodies(Handle, bodyIDsPtr, (uint)bodyIDs.Length);
    }

    public void DeactivateBody(in BodyID bodyId)
    {
        JPH_BodyInterface_DeactivateBody(Handle, bodyId);
    }

    public void DeactivateBodies(Span<BodyID> bodyIDs)
    {
        uint* bodyIDsPtr = stackalloc uint[bodyIDs.Length];
        for (int i = 0; i < bodyIDs.Length; i++)
        {
            bodyIDsPtr[i] = bodyIDs[i];
        }

        JPH_BodyInterface_DeactivateBodies(Handle, bodyIDsPtr, (uint)bodyIDs.Length);
    }

    public bool IsActive(in BodyID bodyID) => JPH_BodyInterface_IsActive(Handle, bodyID);
    public void ResetSleepTimer(in BodyID bodyID) => JPH_BodyInterface_ResetSleepTimer(Handle, bodyID);

    public void SetObjectLayer(in BodyID bodyId, in ObjectLayer layer)
    {
        JPH_BodyInterface_SetObjectLayer(Handle, bodyId, layer.Value);
    }

    public ObjectLayer GetObjectLayer(in BodyID bodyId)
    {
        return new ObjectLayer(JPH_BodyInterface_GetObjectLayer(Handle, bodyId));
    }

    #region GetWorldTransform
    public Matrix4x4 GetWorldTransform(in BodyID bodyID)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Mat4 result;
        JPH_BodyInterface_GetWorldTransform(Handle, bodyID, &result);
        return result.FromJolt();
    }

    public void GetWorldTransform(in BodyID bodyID, out Matrix4x4 transform)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Mat4 result;
        JPH_BodyInterface_GetWorldTransform(Handle, bodyID, &result);
        transform = result.FromJolt();
    }

    public RMatrix4x4 GetRWorldTransform(in BodyID bodyID)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        RMatrix4x4 result;
        JPH_BodyInterface_GetWorldTransformDouble(Handle, bodyID, &result);
        return result;
    }

    public void GetRWorldTransform(in BodyID bodyID, out RMatrix4x4 transform)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        Unsafe.SkipInit(out transform);
        fixed (RMatrix4x4* transformPtr = &transform)
        {
            JPH_BodyInterface_GetWorldTransformDouble(Handle, bodyID, transformPtr);
        }
    }
    #endregion

    #region GetCenterOfMassTransform
    public unsafe Matrix4x4 GetCenterOfMassTransform(in BodyID bodyID)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassTransform)}");

        Mat4 result;
        JPH_BodyInterface_GetCenterOfMassTransform(Handle, bodyID, &result);
        return result.FromJolt();
    }

    public unsafe void GetCenterOfMassTransform(in BodyID bodyID, out Matrix4x4 transform)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassTransform)}");

        Mat4 result;
        JPH_BodyInterface_GetCenterOfMassTransform(Handle, bodyID, &result);
        transform = result.FromJolt();
    }

    public unsafe RMatrix4x4 GetRCenterOfMassTransform(in BodyID bodyID)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassTransform)}");

        RMatrix4x4 result;
        JPH_BodyInterface_GetCenterOfMassTransformDouble(Handle, bodyID, &result);
        return result;
    }

    public unsafe void GetRCenterOfMassTransform(in BodyID bodyID, out RMatrix4x4 transform)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassTransform)}");

        Unsafe.SkipInit(out transform);
        fixed (RMatrix4x4* transformPtr = &transform)
        {
            JPH_BodyInterface_GetCenterOfMassTransformDouble(Handle, bodyID, transformPtr);
        }
    }
    #endregion

    public void MoveKinematic(in BodyID bodyId, in Vector3 targetPosition, in Quaternion targetRotation, float deltaTime)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(MoveKinematic)}");

        JPH_BodyInterface_MoveKinematic(Handle, bodyId, in targetPosition, in targetRotation, deltaTime);
    }

    public void MoveKinematic(in BodyID bodyId, in RVector3 targetPosition, in Quaternion targetRotation, float deltaTime)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(MoveKinematic)}");

        JPH_BodyInterface_MoveKinematic(Handle, bodyId, in targetPosition, in targetRotation, deltaTime);
    }

    public bool ApplyBuoyancyImpulse(
        in BodyID bodyId,
        in Vector3 surfacePosition,
        in Vector3 surfaceNormal,
        float buoyancy, float linearDrag, float angularDrag,
        in Vector3 fluidVelocity,
        in Vector3 gravity,
        float deltaTime)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(ApplyBuoyancyImpulse)}");

        return JPH_BodyInterface_ApplyBuoyancyImpulse(Handle, bodyId, in surfacePosition, in surfaceNormal,
            buoyancy, linearDrag, angularDrag,
            in fluidVelocity,
            in gravity,
            deltaTime);
    }

    public bool ApplyBuoyancyImpulse(
       in BodyID bodyId,
       in RVector3 surfacePosition,
       in Vector3 surfaceNormal,
       float buoyancy, float linearDrag, float angularDrag,
       in Vector3 fluidVelocity,
       in Vector3 gravity,
       float deltaTime)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(ApplyBuoyancyImpulse)}");

        return JPH_BodyInterface_ApplyBuoyancyImpulse(Handle, bodyId, in surfacePosition, in surfaceNormal,
            buoyancy, linearDrag, angularDrag,
            in fluidVelocity,
            in gravity,
            deltaTime);
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

    public Vector3 GetPointVelocity(in BodyID bodyId, in Vector3 point)
    {
        fixed (Vector3* pointPtr = &point)
        {
            Vector3 result;
            JPH_BodyInterface_GetPointVelocity(Handle, bodyId, pointPtr, &result);
            return result;
        }
    }

    public void GetPointVelocity(in BodyID bodyId, in Vector3 point, out Vector3 result)
    {
        Unsafe.SkipInit(out result);

        fixed (Vector3* pointPtr = &point)
        fixed (Vector3* resultPtr = &result)
        {
            JPH_BodyInterface_GetPointVelocity(Handle, bodyId, pointPtr, resultPtr);
        }
    }

    public void AddForce(in BodyID bodyId, in Vector3 force)
    {
        JPH_BodyInterface_AddForce(Handle, bodyId, force);
    }

    public void AddForce(in BodyID bodyId, in Vector3 force, in Vector3 point)
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

    public void AddImpulse(in BodyID bodyId, in Vector3 impulse, in Vector3 point)
    {
        JPH_BodyInterface_AddImpulse2(Handle, bodyId, impulse, point);
    }

    public void AddAngularImpulse(in BodyID bodyId, in Vector3 angularImpulse)
    {
        JPH_BodyInterface_AddAngularImpulse(Handle, bodyId, angularImpulse);
    }

    public Matrix4x4 GetInverseInertia(in BodyID bodyId)
    {
        Mat4 joltMatrix;
        JPH_BodyInterface_GetInverseInertia(Handle, bodyId, &joltMatrix);
        return joltMatrix.FromJolt();
    }

    public void SetGravityFactor(in BodyID bodyId, float gravityFactor)
    {
        JPH_BodyInterface_SetGravityFactor(Handle, bodyId, gravityFactor);
    }

    public float GetGravityFactor(in BodyID bodyId)
    {
        return JPH_BodyInterface_GetGravityFactor(Handle, bodyId);
    }

    public void SetUseManifoldReduction(in BodyID bodyId, bool value)
    {
        JPH_BodyInterface_SetUseManifoldReduction(Handle, bodyId, value);
    }

    public bool GetUseManifoldReduction(in BodyID bodyId)
    {
        return JPH_BodyInterface_GetUseManifoldReduction(Handle, bodyId);
    }

    public void SetIsSensor(in BodyID bodyId, bool value)
    {
        JPH_BodyInterface_SetIsSensor(Handle, bodyId, value);
    }

    public bool IsSensor(in BodyID bodyId)
    {
        return JPH_BodyInterface_IsSensor(Handle, bodyId);
    }

    public TransformedShape GetTransformedShape(in BodyLockInterface bodyLockInterface, in BodyID bodyId)
    {
        bodyLockInterface.LockRead(in bodyId, out BodyLockRead @lock);
        try
        {
            if (@lock.Succeeded)
                return @lock.Body!.GetTransformedShape();

            return default;
        }
        finally
        {
            bodyLockInterface.UnlockRead(@lock);
        }
    }

    public ulong GetUserData(in BodyID bodyId)
    {
        return JPH_BodyInterface_GetUserData(Handle, bodyId);
    }

    public void SetUserData(in BodyID bodyId, ulong userData)
    {
        JPH_BodyInterface_SetUserData(Handle, bodyId, userData);
    }

    public PhysicsMaterial? GetMaterial(in BodyID bodyId, SubShapeID subShapeID)
    {
        return PhysicsMaterial.GetObject(JPH_BodyInterface_GetMaterial(Handle, bodyId, subShapeID));
    }

    public void InvalidateContactCache(in BodyID bodyId)
    {
        JPH_BodyInterface_InvalidateContactCache(Handle, bodyId);
    }
}
