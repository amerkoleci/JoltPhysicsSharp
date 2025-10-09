// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed unsafe class BodyCreationSettings : NativeObject
{
    public BodyCreationSettings()
        : base(JPH_BodyCreationSettings_Create())
    {
    }

    public BodyCreationSettings(ShapeSettings shapeSettings, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create2(shapeSettings.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public BodyCreationSettings(ShapeSettings shapeSettings, in RVector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        fixed (RVector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create2Double(shapeSettings.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public BodyCreationSettings(Shape shape, in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        fixed (Vector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create3(shape.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public BodyCreationSettings(Shape shape, in RVector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        fixed (RVector3* positionPtr = &position)
        fixed (Quaternion* rotationPtr = &rotation)
        {
            Handle = JPH_BodyCreationSettings_Create3Double(shape.Handle, positionPtr, rotationPtr, motionType, objectLayer);
        }
    }

    public Vector3 Position
    {
        get
        {
            Vector3 result;
            JPH_BodyCreationSettings_GetPosition(Handle, &result);
            return result;
        }
        set => JPH_BodyCreationSettings_SetPosition(Handle, &value);
    }

    public Quaternion Rotation
    {
        get
        {
            Quaternion result;
            JPH_BodyCreationSettings_GetRotation(Handle, &result);
            return result;
        }
        set => JPH_BodyCreationSettings_SetRotation(Handle, &value);
    }

    public Vector3 LinearVelocity
    {
        get
        {
            Vector3 velocity;
            JPH_BodyCreationSettings_GetLinearVelocity(Handle, &velocity);
            return velocity;
        }
        set => JPH_BodyCreationSettings_SetLinearVelocity(Handle, &value);
    }

    public Vector3 AngularVelocity
    {
        get
        {
            Vector3 velocity;
            JPH_BodyCreationSettings_GetAngularVelocity(Handle, &velocity);
            return velocity;
        }
        set => JPH_BodyCreationSettings_SetAngularVelocity(Handle, &value);
    }

    public ulong UserData
    {
        get => JPH_BodyCreationSettings_GetUserData(Handle);
        set => JPH_BodyCreationSettings_SetUserData(Handle, value);
    }

    public ObjectLayer ObjectLayer
    {
        get => JPH_BodyCreationSettings_GetObjectLayer(Handle);
        set => JPH_BodyCreationSettings_SetObjectLayer(Handle, value);
    }

    public CollisionGroup CollisionGroup
    {
        get
        {
            JPH_BodyCreationSettings_GetCollisionGroup(Handle, out JPH_CollisionGroup groupNative);
            return CollisionGroup.FromNative(groupNative);
        }
        set
        {
            value.ToNative(out JPH_CollisionGroup groupNative);
            JPH_BodyCreationSettings_SetCollisionGroup(Handle, in groupNative);
        }
    }

    public MotionType MotionType
    {
        get => JPH_BodyCreationSettings_GetMotionType(Handle);
        set => JPH_BodyCreationSettings_SetMotionType(Handle, value);
    }

    public AllowedDOFs AllowedDOFs
    {
        get => JPH_BodyCreationSettings_GetAllowedDOFs(Handle);
        set => JPH_BodyCreationSettings_SetAllowedDOFs(Handle, value);
    }

    public bool AllowDynamicOrKinematic
    {
        get => JPH_BodyCreationSettings_GetAllowDynamicOrKinematic(Handle);
        set => JPH_BodyCreationSettings_SetAllowDynamicOrKinematic(Handle, value);
    }

    public bool IsSensor
    {
        get => JPH_BodyCreationSettings_GetIsSensor(Handle);
        set => JPH_BodyCreationSettings_SetIsSensor(Handle, value);
    }

    public bool CollideKinematicVsNonDynamic
    {
        get => JPH_BodyCreationSettings_GetCollideKinematicVsNonDynamic(Handle);
        set => JPH_BodyCreationSettings_SetCollideKinematicVsNonDynamic(Handle, value);
    }

    public bool UseManifoldReduction
    {
        get => JPH_BodyCreationSettings_GetUseManifoldReduction(Handle);
        set => JPH_BodyCreationSettings_SetUseManifoldReduction(Handle, value);
    }

    public bool ApplyGyroscopicForce
    {
        get => JPH_BodyCreationSettings_GetApplyGyroscopicForce(Handle);
        set => JPH_BodyCreationSettings_SetApplyGyroscopicForce(Handle, value);
    }

    public MotionQuality MotionQuality
    {
        get => JPH_BodyCreationSettings_GetMotionQuality(Handle);
        set => JPH_BodyCreationSettings_SetMotionQuality(Handle, value);
    }

    public bool EnhancedInternalEdgeRemoval
    {
        get => JPH_BodyCreationSettings_GetEnhancedInternalEdgeRemoval(Handle);
        set => JPH_BodyCreationSettings_SetEnhancedInternalEdgeRemoval(Handle, value);
    }

    public bool AllowSleeping
    {
        get => JPH_BodyCreationSettings_GetAllowSleeping(Handle);
        set => JPH_BodyCreationSettings_SetAllowSleeping(Handle, value);
    }

    public float Friction
    {
        get => JPH_BodyCreationSettings_GetFriction(Handle);
        set => JPH_BodyCreationSettings_SetFriction(Handle, value);
    }

    public float Restitution
    {
        get => JPH_BodyCreationSettings_GetRestitution(Handle);
        set => JPH_BodyCreationSettings_SetRestitution(Handle, value);
    }

    public float LinearDamping
    {
        get => JPH_BodyCreationSettings_GetLinearDamping(Handle);
        set => JPH_BodyCreationSettings_SetLinearDamping(Handle, value);
    }

    public float AngularDamping
    {
        get => JPH_BodyCreationSettings_GetAngularDamping(Handle);
        set => JPH_BodyCreationSettings_SetAngularDamping(Handle, value);
    }

    public float MaxLinearVelocity
    {
        get => JPH_BodyCreationSettings_GetMaxLinearVelocity(Handle);
        set => JPH_BodyCreationSettings_SetMaxLinearVelocity(Handle, value);
    }

    public float MaxAngularVelocity
    {
        get => JPH_BodyCreationSettings_GetMaxAngularVelocity(Handle);
        set => JPH_BodyCreationSettings_SetMaxAngularVelocity(Handle, value);
    }

    public float GravityFactor
    {
        get => JPH_BodyCreationSettings_GetGravityFactor(Handle);
        set => JPH_BodyCreationSettings_SetGravityFactor(Handle, value);
    }

    public uint NumVelocityStepsOverride
    {
        get => JPH_BodyCreationSettings_GetNumVelocityStepsOverride(Handle);
        set => JPH_BodyCreationSettings_SetNumVelocityStepsOverride(Handle, value);
    }

    public uint NumPositionStepsOverride
    {
        get => JPH_BodyCreationSettings_GetNumPositionStepsOverride(Handle);
        set => JPH_BodyCreationSettings_SetNumPositionStepsOverride(Handle, value);
    }

    public OverrideMassProperties OverrideMassProperties
    {
        get => JPH_BodyCreationSettings_GetOverrideMassProperties(Handle);
        set => JPH_BodyCreationSettings_SetOverrideMassProperties(Handle, value);
    }

    public float InertiaMultiplier
    {
        get => JPH_BodyCreationSettings_GetInertiaMultiplier(Handle);
        set => JPH_BodyCreationSettings_SetInertiaMultiplier(Handle, value);
    }

    public MassProperties MassPropertiesOverride
    {
        get
        {
            JPH_BodyCreationSettings_GetMassPropertiesOverride(Handle, out MassProperties result);
            return result;
        }
        set => JPH_BodyCreationSettings_SetMassPropertiesOverride(Handle, &value);
    }

    protected override void DisposeNative()
    {
        JPH_BodyCreationSettings_Destroy(Handle);
    }

    public void GetLinearVelocity(out Vector3 velocity)
    {
        Unsafe.SkipInit(out velocity);
        fixed (Vector3* velocityPtr = &velocity)
        {
            JPH_BodyCreationSettings_GetLinearVelocity(Handle, velocityPtr);
        }
    }

    public void GetAngularVelocity(out Vector3 velocity)
    {
        Unsafe.SkipInit(out velocity);
        fixed (Vector3* velocityPtr = &velocity)
        {
            JPH_BodyCreationSettings_GetAngularVelocity(Handle, velocityPtr);
        }
    }
}
