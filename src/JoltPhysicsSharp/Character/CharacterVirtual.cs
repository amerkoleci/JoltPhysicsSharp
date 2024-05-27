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
    public delegate void ContactAddedHandler(CharacterVirtual character, in BodyID bodyID2, SubShapeID subShapeID2,in Double3 contactPosition, in Vector3 contactNornal, ref CharacterContactSettings settings);
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

    public CharacterVirtual(CharacterVirtualSettings settings, in Vector3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        Handle = JPH_CharacterVirtual_Create(settings.Handle, position, rotation, userData, physicsSystem.Handle);
        nint listenerContext = DelegateProxies.CreateUserData(this, true);
        _listenerHandle = JPH_CharacterContactListener_Create(_listener_Procs, listenerContext);
    }

    public CharacterVirtual(CharacterVirtualSettings settings, in Double3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        Handle = JPH_CharacterVirtual_Create(settings.Handle, position, rotation, userData, physicsSystem.Handle);

        nint listenerContext = DelegateProxies.CreateUserData(this, true);
        _listenerHandle = JPH_CharacterContactListener_Create(_listener_Procs, listenerContext);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_CharacterContactListener_Destroy(_listenerHandle);
        }

        base.Dispose(disposing);
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

    public ulong UserData
    {
        get => JPH_CharacterVirtual_GetUserData(Handle);
        set => JPH_CharacterVirtual_SetUserData(Handle, value);
    }

    public void Update(float deltaTime, in ObjectLayer layer, PhysicsSystem physicsSystem)
    {
        JPH_CharacterVirtual_Update(Handle, deltaTime, layer.Value, physicsSystem.Handle);
    }

    public unsafe void ExtendedUpdate(float deltaTime, ExtendedUpdateSettings settings, in ObjectLayer layer, PhysicsSystem physicsSystem)
    {
        JPH_CharacterVirtual_ExtendedUpdate(Handle, deltaTime, &settings, layer.Value, physicsSystem.Handle);
    }

    public void RefreshContacts(in ObjectLayer layer, PhysicsSystem physicsSystem)
    {
        JPH_CharacterVirtual_RefreshContacts(Handle, layer.Value, physicsSystem.Handle);
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

    #region CharacterContactListener
    [UnmanagedCallersOnly]
    private static unsafe void OnAdjustBodyVelocityCallback(nint context, nint character, nint body2, Vector3* linearVelocity, Vector3* angularVelocity)
    {
        CharacterVirtual listener = DelegateProxies.GetUserData<CharacterVirtual>(context, out _);

        if (listener.OnAdjustBodyVelocity != null)
        {
            listener.OnAdjustBodyVelocity(listener, new Body(body2), *linearVelocity, *angularVelocity);
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool32 OnContactValidateCallback(nint context, nint character, BodyID bodyID2, SubShapeID subShapeID2)
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
    public Bool32 CanPushCharacter;
    public Bool32 CanReceiveImpulses;
}
