// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CharacterVirtual : CharacterBase
{
    private readonly nint _listenerHandle;
    private readonly unsafe JPH_CharacterContactListener_Procs _listener_Procs = new JPH_CharacterContactListener_Procs
    {
        OnAdjustBodyVelocity = &OnAdjustBodyVelocityCallback,
        OnContactValidate = &OnContactValidateCallback,
        OnContactAdded = &OnContactAddedCallback,
        OnContactSolve = &OnContactSolveCallback
    };

    public delegate void AdjustBodyVelocityHandler(CharacterVirtual character, in Body body2, Vector3 linearVelocity, Vector3 angularVelocity);
    public delegate bool ContactValidateHandler(CharacterVirtual character, in BodyID bodyID2, SubShapeID subShapeID2);
    public delegate void ContactAddedHandler(CharacterVirtual character, in BodyID bodyID2, SubShapeID subShapeID2, in Double3 contactPosition, in Vector3 contactNornal, ref CharacterContactSettings settings);
    public delegate void ContactSolveHandler(CharacterVirtual character, in BodyID bodyID2, SubShapeID subShapeID2,
        in Double3 contactPosition,
        in Vector3 contactNornal,
        in Vector3 contactVelocity,
        /*JPH_PhysicsMaterial*/nint contactMaterial,
        in Vector3 characterVelocity,
        out Vector3 newCharacterVelocity);

    public event AdjustBodyVelocityHandler? OnAdjustBodyVelocity;
    public event ContactValidateHandler? OnContactValidate;
    public event ContactAddedHandler? OnContactAdded;
    public event ContactSolveHandler? OnContactSolve;

    public unsafe CharacterVirtual(CharacterVirtualSettings settings, in Vector3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        JPH_CharacterVirtualSettings nativeSettings;
        settings.ToNative(&nativeSettings);
        Handle = JPH_CharacterVirtual_Create(&nativeSettings, position, rotation, userData, physicsSystem.Handle);
        nint listenerContext = DelegateProxies.CreateUserData(this, true);
        _listenerHandle = JPH_CharacterContactListener_Create(_listener_Procs, listenerContext);
    }

    public unsafe CharacterVirtual(CharacterVirtualSettings settings, in Double3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        JPH_CharacterVirtualSettings nativeSettings;
        settings.ToNative(&nativeSettings);
        Handle = JPH_CharacterVirtual_Create(&nativeSettings, position, rotation, userData, physicsSystem.Handle);

        nint listenerContext = DelegateProxies.CreateUserData(this, true);
        _listenerHandle = JPH_CharacterContactListener_Create(_listener_Procs, listenerContext);
    }

    protected override void DisposeNative()
    {
        JPH_CharacterContactListener_Destroy(_listenerHandle);
    }

    public Vector3 LinearVelocity
    {
        get
        {
            JPH_CharacterVirtual_GetLinearVelocity(Handle, out Vector3 result);
            return result;
        }
        set
        {
            JPH_CharacterVirtual_SetLinearVelocity(Handle, in value);
        }
    }

    public Vector3 Position
    {
        get
        {
            JPH_CharacterVirtual_GetPosition(Handle, out Vector3 result);
            return result;
        }
        set
        {
            JPH_CharacterVirtual_SetPosition(Handle, in value);
        }
    }

    public Quaternion Rotation
    {
        get
        {
            JPH_CharacterVirtual_GetRotation(Handle, out Quaternion result);
            return result;
        }
        set
        {
            JPH_CharacterVirtual_SetRotation(Handle, in value);
        }
    }

    public Matrix4x4 WorldTransform
    {
        get
        {
            JPH_CharacterVirtual_GetWorldTransform(Handle, out Matrix4x4 result);
            return result;
        }
    }

    public Matrix4x4 CenterOfMassTransform
    {
        get
        {
            JPH_CharacterVirtual_GetCenterOfMassTransform(Handle, out Matrix4x4 result);
            return result;
        }
    }

    public float Mass
    {
        get => JPH_CharacterVirtual_GetMass(Handle);
        set => JPH_CharacterVirtual_SetMass(Handle, value);
    }

    public float MaxStrength
    {
        get => JPH_CharacterVirtual_GetMaxStrength(Handle);
        set => JPH_CharacterVirtual_SetMaxStrength(Handle, value);
    }

    public float PenetrationRecoverySpeed
    {
        get => JPH_CharacterVirtual_GetPenetrationRecoverySpeed(Handle);
        set => JPH_CharacterVirtual_SetPenetrationRecoverySpeed(Handle, value);
    }

    public bool EnhancedInternalEdgeRemoval
    {
        get => JPH_CharacterVirtual_GetEnhancedInternalEdgeRemoval(Handle);
        set => JPH_CharacterVirtual_SetEnhancedInternalEdgeRemoval(Handle, value);
    }

    public float CharacterPadding
    {
        get => JPH_CharacterVirtual_GetCharacterPadding(Handle);
    }

    public uint MaxNumHits
    {
        get => JPH_CharacterVirtual_GetMaxNumHits(Handle);
        set => JPH_CharacterVirtual_SetMaxNumHits(Handle, value);
    }

    public float HitReductionCosMaxAngle
    {
        get => JPH_CharacterVirtual_GetHitReductionCosMaxAngle(Handle);
        set => JPH_CharacterVirtual_SetHitReductionCosMaxAngle(Handle, value);
    }

    public bool MaxHitsExceeded
    {
        get => JPH_CharacterVirtual_GetMaxHitsExceeded(Handle);
    }

    public Vector3 ShapeOffset
    {
        get
        {
            JPH_CharacterVirtual_GetShapeOffset(Handle, out Vector3 result);
            return result;
        }
        set => JPH_CharacterVirtual_SetShapeOffset(Handle, in value);
    }

    public ulong UserData
    {
        get => JPH_CharacterVirtual_GetUserData(Handle);
        set => JPH_CharacterVirtual_SetUserData(Handle, value);
    }

    public BodyID InnerBodyID
    {
        get => JPH_CharacterVirtual_GetInnerBodyID(Handle);
    }

    public void Update(float deltaTime, in ObjectLayer layer, PhysicsSystem physicsSystem,
        BodyFilter? bodyFilter = default, ShapeFilter? shapeFilter = default)
    {
        JPH_CharacterVirtual_Update(Handle, deltaTime, layer.Value, physicsSystem.Handle,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public unsafe void ExtendedUpdate(float deltaTime,
        ExtendedUpdateSettings settings,
        in ObjectLayer layer,
        PhysicsSystem physicsSystem,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        JPH_CharacterVirtual_ExtendedUpdate(Handle, deltaTime,
            &settings,
            layer.Value,
            physicsSystem.Handle,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public void RefreshContacts(in ObjectLayer layer,
        PhysicsSystem physicsSystem,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        JPH_CharacterVirtual_RefreshContacts(Handle,
            layer.Value,
            physicsSystem.Handle,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public Vector3 CancelVelocityTowardsSteepSlopes(in Vector3 desiredVelocity)
    {
        JPH_CharacterVirtual_CancelVelocityTowardsSteepSlopes(Handle, in desiredVelocity, out Vector3 velocity);
        return velocity;
    }

    public void CancelVelocityTowardsSteepSlopes(in Vector3 desiredVelocity, out Vector3 velocity)
    {
        JPH_CharacterVirtual_CancelVelocityTowardsSteepSlopes(Handle, in desiredVelocity, out velocity);
    }

    public bool CanWalkStairs(in Vector3 linearVelocity)
    {
        return JPH_CharacterVirtual_CanWalkStairs(Handle, in linearVelocity);
    }

    public bool WalkStairs(float deltaTime,
        in Vector3 stepUp,
        in Vector3 stepForward,
        in Vector3 stepForwardTest,
        in Vector3 stepDownExtra,
        in ObjectLayer layer,
        PhysicsSystem system,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        return JPH_CharacterVirtual_WalkStairs(Handle, deltaTime,
            in stepUp, in stepForward, in stepForwardTest, in stepDownExtra,
            in layer, system.Handle,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0
            );
    }

    public bool StickToFloor(
        in Vector3 stepDown,
        in ObjectLayer layer,
        PhysicsSystem system,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        return JPH_CharacterVirtual_StickToFloor(Handle,
            in stepDown,
            in layer,
            system.Handle,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0
            );
    }

    public void UpdateGroundVelocity()
    {
        JPH_CharacterVirtual_UpdateGroundVelocity(Handle);
    }

    public bool SetShape(float deltaTime,
        Shape shape,
        float maxPenetrationDepth,
        in ObjectLayer layer,
        PhysicsSystem system,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        return JPH_CharacterVirtual_SetShape(Handle,
            shape.Handle,
            maxPenetrationDepth,
            layer.Value,
            system.Handle,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0
            );
    }

    public void SetInnerBodyShape(Shape shape)
    {
        JPH_CharacterVirtual_SetInnerBodyShape(Handle, shape.Handle);
    }

    #region CharacterContactListener
    [UnmanagedCallersOnly]
    private static unsafe void OnAdjustBodyVelocityCallback(nint context, nint character, nint body2, Vector3* linearVelocity, Vector3* angularVelocity)
    {
        CharacterVirtual listener = DelegateProxies.GetUserData<CharacterVirtual>(context, out _);

        if (listener.OnAdjustBodyVelocity != null)
        {
            listener.OnAdjustBodyVelocity(listener, Body.GetObject(body2)!, *linearVelocity, *angularVelocity);
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool8 OnContactValidateCallback(nint context, nint character, BodyID bodyID2, SubShapeID subShapeID2)
    {
        CharacterVirtual listener = DelegateProxies.GetUserData<CharacterVirtual>(context, out _);

        if (listener.OnContactValidate != null)
        {
            return listener.OnContactValidate(listener, bodyID2, subShapeID2);
        }

        return true;
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnContactAddedCallback(nint context, nint character,
        BodyID bodyID2, SubShapeID subShapeID2,
        Vector3* contactPosition, // JPH_RVec3
        Vector3* contactNormal,
        CharacterContactSettings* ioSettings)
    {
        CharacterVirtual listener = DelegateProxies.GetUserData<CharacterVirtual>(context, out _);

        if (listener.OnContactAdded != null)
        {
            CharacterContactSettings settings = *ioSettings;
            listener.OnContactAdded(listener, bodyID2, subShapeID2, new Double3(*contactPosition), *contactNormal, ref settings);
            *ioSettings = settings;
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnContactSolveCallback(nint context, nint character,
        BodyID bodyID2, SubShapeID subShapeID2,
        Vector3* contactPosition, // JPH_RVec3
        Vector3* contactNormal,
        Vector3* contactVelocity,
        nint contactMaterial, // JPH_PhysicsMaterial
        Vector3* characterVelocity,
        Vector3* newCharacterVelocity)
    {
        CharacterVirtual listener = DelegateProxies.GetUserData<CharacterVirtual>(context, out _);

        if (listener.OnContactSolve != null)
        {
            Vector3 newCharacterVelocityIn = *newCharacterVelocity;
            listener.OnContactSolve(listener, bodyID2, subShapeID2,
                new Double3(*contactPosition),
                *contactNormal,
                *contactVelocity,
                contactMaterial, *characterVelocity, out newCharacterVelocityIn);
            *newCharacterVelocity = newCharacterVelocityIn;
        }
    }
    #endregion
}

public struct CharacterContactSettings
{
    public Bool8 CanPushCharacter;
    public Bool8 CanReceiveImpulses;
}
