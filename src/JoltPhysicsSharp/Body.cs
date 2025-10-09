// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class Body : NativeObject
{
    internal Body(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    public BodyID ID => JPH_Body_GetID(Handle);

    /// <summary>
    /// Get the type of body (rigid or soft)
    /// </summary>
    public BodyType BodyType => JPH_Body_GetBodyType(Handle);

    /// <summary>
    /// Gets where this body is a rigid body.
    /// </summary>
    public bool IsRigidBody => JPH_Body_GetBodyType(Handle) == BodyType.Rigid;

    /// <summary>
    /// Gets where this body is a soft body.
    /// </summary>
    public bool IsSoftBody => JPH_Body_GetBodyType(Handle) == BodyType.Soft;
    public bool IsActive => JPH_Body_IsActive(Handle);
    public bool IsStatic => JPH_Body_IsStatic(Handle);
    public bool IsKinematic => JPH_Body_IsKinematic(Handle);
    public bool IsDynamic => JPH_Body_IsDynamic(Handle);
    public bool CanBeKinematicOrDynamic => JPH_Body_CanBeKinematicOrDynamic(Handle);

    public bool IsSensor
    {
        get => JPH_Body_IsSensor(Handle);
        set => JPH_Body_SetIsSensor(Handle, value);
    }

    public bool CollideKinematicVsNonDynamic
    {
        get => JPH_Body_GetCollideKinematicVsNonDynamic(Handle);
        set => JPH_Body_SetCollideKinematicVsNonDynamic(Handle, value);
    }

    public bool UseManifoldReduction
    {
        get => JPH_Body_GetUseManifoldReduction(Handle);
        set => JPH_Body_SetUseManifoldReduction(Handle, value);
    }

    public bool ApplyGyroscopicForce
    {
        get => JPH_Body_GetApplyGyroscopicForce(Handle);
        set => JPH_Body_SetApplyGyroscopicForce(Handle, value);
    }

    public bool EnhancedInternalEdgeRemoval
    {
        get => JPH_Body_GetEnhancedInternalEdgeRemoval(Handle);
        set => JPH_Body_SetEnhancedInternalEdgeRemoval(Handle, value);
    }

    public BoundingBox WorldSpaceBounds
    {
        get
        {
            JPH_Body_GetWorldSpaceBounds(Handle, out BoundingBox result);
            return result;
        }
    }

    public MotionProperties MotionProperties => JPH_Body_GetMotionProperties(Handle);
    public MotionProperties MotionPropertiesUnchecked => JPH_Body_GetMotionPropertiesUnchecked(Handle);

    public MotionType MotionType
    {
        get => JPH_Body_GetMotionType(Handle);
        set => JPH_Body_SetMotionType(Handle, value);
    }

    public BroadPhaseLayer BroadPhaseLayer
    {
        get => JPH_Body_GetBroadPhaseLayer(Handle);
    }

    public ObjectLayer ObjectLayer
    {
        get => JPH_Body_GetObjectLayer(Handle);
    }

    public CollisionGroup CollisionGroup
    {
        get
        {
            JPH_Body_GetCollisionGroup(Handle, out JPH_CollisionGroup groupNative);
            return CollisionGroup.FromNative(groupNative);
        }
        set
        {
            value.ToNative(out JPH_CollisionGroup groupNative);
            JPH_Body_SetCollisionGroup(Handle, in groupNative);
        }
    }

    public bool AllowSleeping
    {
        get => JPH_Body_GetAllowSleeping(Handle);
        set => JPH_Body_SetAllowSleeping(Handle, value);
    }

    public float Friction
    {
        get => JPH_Body_GetFriction(Handle);
        set => JPH_Body_SetFriction(Handle, value);
    }

    public float Restitution
    {
        get => JPH_Body_GetRestitution(Handle);
        set => JPH_Body_SetRestitution(Handle, value);
    }

    public Vector3 Position
    {
        get
        {
            if (DoublePrecision)
                throw new InvalidOperationException($"Double precision is enabled: use {nameof(RPosition)}");

            JPH_Body_GetPosition(Handle, out Vector3 result);
            return result;
        }
    }

    public RVector3 RPosition
    {
        get
        {
            if (!DoublePrecision)
                throw new InvalidOperationException($"Double precision is disabled: use {nameof(Position)}");

            JPH_Body_GetPosition(Handle, out RVector3 result);
            return result;
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
            if (DoublePrecision)
                throw new InvalidOperationException($"Double precision is enabled: use {nameof(RCenterOfMassPosition)}");

            JPH_Body_GetCenterOfMassPosition(Handle, out Vector3 value);
            return value;
        }
    }

    public RVector3 RCenterOfMassPosition
    {
        get
        {
            if (!DoublePrecision)
                throw new InvalidOperationException($"Double precision is disabled: use {nameof(CenterOfMassPosition)}");

            JPH_Body_GetCenterOfMassPosition(Handle, out RVector3 value);
            return value;
        }
    }

    public bool GetUseManifoldReductionWithBody(in Body other)
    {
        return JPH_Body_GetUseManifoldReductionWithBody(Handle, other.Handle);
    }

    public bool GetEnhancedInternalEdgeRemovalWithBody(in Body other)
    {
        return JPH_Body_GetEnhancedInternalEdgeRemovalWithBody(Handle, other.Handle);
    }

    public void GetPosition(out Vector3 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPosition)}");

        JPH_Body_GetPosition(Handle, out result);
    }

    public void GetRPosition(out RVector3 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPosition)}");

        JPH_Body_GetPosition(Handle, out result);
    }

    public void GetRotation(out Quaternion result)
    {
        JPH_Body_GetRotation(Handle, out result);
    }

    public void GetCenterOfMassPosition(out Vector3 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassPosition)}");

        JPH_Body_GetCenterOfMassPosition(Handle, out result);
    }

    public void GetRCenterOfMassPosition(out RVector3 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassPosition)}");

        JPH_Body_GetCenterOfMassPosition(Handle, out result);
    }

    #region GetWorldTransform
    public unsafe Matrix4x4 GetWorldTransform()
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Mat4 joltMatrix;
        JPH_Body_GetWorldTransform(Handle, &joltMatrix);
        return joltMatrix.FromJolt();
    }

    public unsafe void GetWorldTransform(out Matrix4x4 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Mat4 joltMatrix;
        JPH_Body_GetWorldTransform(Handle, &joltMatrix);
        result = joltMatrix.FromJolt();
    }

    public RMatrix4x4 GetRWorldTransform()
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        JPH_Body_GetWorldTransform(Handle, out RMatrix4x4 result);
        return result;
    }

    public void GetRWorldTransform(out RMatrix4x4 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        JPH_Body_GetWorldTransform(Handle, out result);
    }
    #endregion

    #region GetCenterOfMassTransform
    public unsafe Matrix4x4 GetCenterOfMassTransform()
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassTransform)}");

        Mat4 joltMatrix;
        JPH_Body_GetCenterOfMassTransform(Handle, &joltMatrix);
        return joltMatrix.FromJolt();
    }

    public unsafe void GetCenterOfMassTransform(out Matrix4x4 result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassTransform)}");

        Mat4 joltMatrix;
        JPH_Body_GetCenterOfMassTransform(Handle, &joltMatrix);
        result = joltMatrix.FromJolt();
    }

    public RMatrix4x4 GetRCenterOfMassTransform()
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassTransform)}");

        JPH_Body_GetCenterOfMassTransform(Handle, out RMatrix4x4 result);
        return result;
    }

    public void GetRCenterOfMassTransform(out RMatrix4x4 result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassTransform)}");

        JPH_Body_GetCenterOfMassTransform(Handle, out result);
    }
    #endregion

    public void GetWorldSpaceBounds(out BoundingBox bounds)
    {
        JPH_Body_GetWorldSpaceBounds(Handle, out bounds);
    }

    public void SetIsSensor(bool value) => JPH_Body_SetIsSensor(Handle, value);
    public void SetCollideKinematicVsNonDynamic(bool value) => JPH_Body_SetCollideKinematicVsNonDynamic(Handle, value);
    public void SetUseManifoldReduction(bool value) => JPH_Body_SetUseManifoldReduction(Handle, value);
    public void SetApplyGyroscopicForce(bool value) => JPH_Body_SetApplyGyroscopicForce(Handle, value);
    public void SetEnhancedInternalEdgeRemoval(bool value) => JPH_Body_SetEnhancedInternalEdgeRemoval(Handle, value);
    public void SetMotionType(MotionType value) => JPH_Body_SetMotionType(Handle, value);

    public void SetAllowSleeping(bool value) => JPH_Body_SetAllowSleeping(Handle, value);

    public void SetFriction(float value) => JPH_Body_SetFriction(Handle, value);
    public void SetRestitution(float value) => JPH_Body_SetRestitution(Handle, value);

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
        JPH_Body_SetLinearVelocity(Handle, in velocity);
    }

    public void SetLinearVelocityClamped(in Vector3 velocity)
    {
        JPH_Body_SetLinearVelocityClamped(Handle, in velocity);
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
        JPH_Body_SetAngularVelocity(Handle, in velocity);
    }

    public void SetAngularVelocityClamped(in Vector3 velocity)
    {
        JPH_Body_SetAngularVelocityClamped(Handle, in velocity);
    }

    public Vector3 GetPointVelocityCOM(in Vector3 pointRelativeToCOM)
    {
        JPH_Body_GetPointVelocityCOM(Handle, in pointRelativeToCOM, out Vector3 result);
        return result;
    }

    public void GetPointVelocityCOM(in Vector3 pointRelativeToCOM, out Vector3 result)
    {
        JPH_Body_GetPointVelocityCOM(Handle, in pointRelativeToCOM, out result);
    }

    public Vector3 GetPointVelocity(in Vector3 point)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetPointVelocity)}");

        JPH_Body_GetPointVelocity(Handle, in point, out Vector3 result);
        return result;
    }

    public void GetPointVelocity(in Vector3 point, out Vector3 velocity)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetPointVelocity)}");

        JPH_Body_GetPointVelocity(Handle, in point, out velocity);
    }

    public Vector3 GetPointVelocity(in RVector3 point)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPointVelocity)}");

        JPH_Body_GetPointVelocity(Handle, in point, out Vector3 result);
        return result;
    }

    public void GetPointVelocity(in RVector3 point, out Vector3 velocity)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPointVelocity)}");

        JPH_Body_GetPointVelocity(Handle, in point, out velocity);
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

    public void ResetForce() => JPH_Body_ResetForce(Handle);
    public void ResetTorque() => JPH_Body_ResetTorque(Handle);
    public void ResetMotion() => JPH_Body_ResetMotion(Handle);

    public unsafe Matrix4x4 GetInverseInertia()
    {
        Mat4 joltMatrix;
        JPH_Body_GetInverseInertia(Handle, &joltMatrix);
        return joltMatrix.FromJolt();
    }

    public unsafe void GetInverseInertia(out Matrix4x4 result)
    {
        Mat4 joltMatrix;
        JPH_Body_GetInverseInertia(Handle, &joltMatrix);
        result = joltMatrix.FromJolt();
    }

    public void GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in Vector3 position, out Vector3 normal)
    {
        if (DoublePrecision)
        {
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, new RVector3(in position), out normal);
        }
        else
        {
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out normal);
        }
    }

    public Vector3 GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in Vector3 position)
    {
        if (DoublePrecision)
        {
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, new RVector3(in position), out Vector3 normal);
            return normal;
        }
        else
        {
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out Vector3 normal);
            return normal;
        }
    }

    public void GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in RVector3 position, out Vector3 normal)
    {
        if (!DoublePrecision)
        {
            Vector3 sPosition = (Vector3)position;
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in sPosition, out normal);
        }
        else
        {
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out normal);
        }
    }

    public Vector3 GetWorldSpaceSurfaceNormal(in SubShapeID subShapeID, in RVector3 position)
    {
        if (!DoublePrecision)
        {
            Vector3 sPosition = (Vector3)position;
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in sPosition, out Vector3 normal);
            return normal;

        }
        else
        {
            JPH_Body_GetWorldSpaceSurfaceNormal(Handle, subShapeID.Value, in position, out Vector3 normal);
            return normal;
        }
    }

    public TransformedShape GetTransformedShape()
    {
        // JPH_ASSERT(BodyAccess::sCheckRights(BodyAccess::sPositionAccess(), BodyAccess::EAccess::Read)); 
        // return TransformedShape(mPosition, mRotation, mShape, mID);
        return new TransformedShape(CenterOfMassPosition, Rotation, Shape, ID);
    }

    public void AddForce(in Vector3 force)
    {
        JPH_Body_AddForce(Handle, in force);
    }

    public void AddForceAtPosition(in Vector3 force, in Vector3 position)
    {
        if (DoublePrecision)
        {
            JPH_Body_AddForceAtPosition(Handle, in force, new RVector3(in position));
        }
        else
        {
            JPH_Body_AddForceAtPosition(Handle, in force, in position);
        }
    }

    public void AddForceAtPosition(in Vector3 force, in RVector3 position)
    {
        if (!DoublePrecision)
        {
            Vector3 sPosition = (Vector3)position;
            JPH_Body_AddForceAtPosition(Handle, in force, in sPosition);
        }
        else
        {
            JPH_Body_AddForceAtPosition(Handle, in force, in position);
        }
    }

    public void AddTorque(in Vector3 torque)
    {
        JPH_Body_AddTorque(Handle, in torque);
    }

    public void AddImpulse(in Vector3 impulse)
    {
        JPH_Body_AddImpulse(Handle, impulse);
    }

    public void AddImpulseAtPosition(in Vector3 impulse, in Vector3 position)
    {
        if (DoublePrecision)
        {
            JPH_Body_AddImpulseAtPosition(Handle, in impulse, new RVector3(in position));
        }
        else
        {
            JPH_Body_AddImpulseAtPosition(Handle, in impulse, in position);
        }
    }

    public void AddImpulseAtPosition(in Vector3 impulse, in RVector3 position)
    {
        if (!DoublePrecision)
        {
            Vector3 sPosition = (Vector3)position;
            JPH_Body_AddImpulseAtPosition(Handle, in impulse, in sPosition);
        }
        else
        {
            JPH_Body_AddImpulseAtPosition(Handle, in impulse, in position);
        }
    }

    public void AddAngularImpulse(in Vector3 angularImpulse)
    {
        JPH_Body_AddAngularImpulse(Handle, angularImpulse);
    }

    public void MoveKinematic(in Vector3 targetPosition, in Quaternion targetRotation, float deltaTime)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(MoveKinematic)}");

        JPH_Body_MoveKinematic(Handle, in targetPosition, in targetRotation, deltaTime);
    }

    public void MoveKinematic(in RVector3 targetPosition, in Quaternion targetRotation, float deltaTime)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(MoveKinematic)}");

        JPH_Body_MoveKinematic(Handle, in targetPosition, in targetRotation, deltaTime);
    }

    public bool ApplyBuoyancyImpulse(
        in Vector3 surfacePosition,
        in Vector3 surfaceNormal,
        float buoyancy, float linearDrag, float angularDrag,
        in Vector3 fluidVelocity,
        in Vector3 gravity,
        float deltaTime)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(ApplyBuoyancyImpulse)}");

        return JPH_Body_ApplyBuoyancyImpulse(Handle, in surfacePosition, in surfaceNormal,
            buoyancy, linearDrag, angularDrag,
            in fluidVelocity,
            in gravity,
            deltaTime);
    }

    public bool ApplyBuoyancyImpulse(
       in RVector3 surfacePosition,
       in Vector3 surfaceNormal,
       float buoyancy, float linearDrag, float angularDrag,
       in Vector3 fluidVelocity,
       in Vector3 gravity,
       float deltaTime)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(ApplyBuoyancyImpulse)}");

        return JPH_Body_ApplyBuoyancyImpulse(Handle, in surfacePosition, in surfaceNormal,
            buoyancy, linearDrag, angularDrag,
            in fluidVelocity,
            in gravity,
            deltaTime);
    }

    public bool IsInBroadPhase => JPH_Body_IsInBroadPhase(Handle);
    public bool IsCollisionCacheInvalid => JPH_Body_IsCollisionCacheInvalid(Handle);

    public Shape Shape
    {
        get
        {
            nint shapeHandle = JPH_Body_GetShape(Handle);
            if (shapeHandle == 0)
                throw new InvalidOperationException("Invalid body shape");

            return Shape.GetObject(shapeHandle)!;
        }
    }

    public ulong GetUserData()
    {
        return JPH_Body_GetUserData(Handle);
    }

    public void SetUserData(ulong userData)
    {
        JPH_Body_SetUserData(Handle, userData);
    }

    internal static Body? GetObject(nint handle) => GetOrAddObject(handle, h => new Body(h, false));
}
