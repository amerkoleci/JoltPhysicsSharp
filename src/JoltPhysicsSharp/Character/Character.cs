// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class Character : CharacterBase
{
    public unsafe Character(CharacterSettings settings, in Vector3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use constructor with Double3");

        JPH_CharacterSettings nativeSettings;
        settings.ToNative(&nativeSettings);
        Handle = JPH_Character_Create(&nativeSettings, position, rotation, userData, physicsSystem.Handle);
        GC.KeepAlive(settings);
        GC.KeepAlive(physicsSystem);
    }

    public unsafe Character(CharacterSettings settings, in RVector3 position, in Quaternion rotation, ulong userData, PhysicsSystem physicsSystem)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use constructor with Vector3");

        JPH_CharacterSettings nativeSettings;
        settings.ToNative(&nativeSettings);
        Handle = JPH_Character_Create(&nativeSettings, position, rotation, userData, physicsSystem.Handle);
        GC.KeepAlive(settings);
        GC.KeepAlive(physicsSystem);
    }

    public void AddToPhysicsSystem(Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        JPH_Character_AddToPhysicsSystem(Handle, activationMode, lockBodies);
    }

    public void RemoveFromPhysicsSystem(bool lockBodies = true)
    {
        JPH_Character_RemoveFromPhysicsSystem(Handle, lockBodies);
    }

    public void Activate(bool lockBodies = true)
    {
        JPH_Character_Activate(Handle, lockBodies);
    }

    public void PostSimulation(float maxSeparationDistance, bool lockBodies = true)
    {
        JPH_Character_PostSimulation(Handle, maxSeparationDistance, lockBodies);
    }

    public void SetLinearAndAngularVelocity(in Vector3 linearVelocity, in Vector3 angularVelocity, bool lockBodies = true)
    {
        JPH_Character_SetLinearAndAngularVelocity(Handle, in linearVelocity, in angularVelocity, lockBodies);
    }

    public Vector3 GetLinearVelocity()
    {
        JPH_Character_GetLinearVelocity(Handle, out Vector3 result);
        return result;
    }

    public void GetLinearVelocity(out Vector3 linearVelocity)
    {
        JPH_Character_GetLinearVelocity(Handle, out linearVelocity);
    }

    public void SetLinearVelocity(in Vector3 linearVelocity, bool lockBodies = true)
    {
        JPH_Character_SetLinearVelocity(Handle, in linearVelocity, lockBodies);
    }

    public void AddLinearVelocity(in Vector3 linearVelocity, bool lockBodies = true)
    {
        JPH_Character_AddLinearVelocity(Handle, in linearVelocity, lockBodies);
    }

    public void AddImpulse(in Vector3 impulse, bool lockBodies = true)
    {
        JPH_Character_AddImpulse(Handle, in impulse, lockBodies);
    }

    public (Vector3 position, Quaternion rotation) GetPositionAndRotation(bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPositionAndRotation)}");

        JPH_Character_GetPositionAndRotation(Handle, out Vector3 position, out Quaternion rotation, lockBodies);
        return (position, rotation);
    }

    public void GetPositionAndRotation(out Vector3 position, out Quaternion rotation, bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPositionAndRotation)}");

        JPH_Character_GetPositionAndRotation(Handle, out position, out rotation, lockBodies);
    }

    public (RVector3 position, Quaternion rotation) GetRPositionAndRotation(bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPositionAndRotation)}");

        JPH_Character_GetPositionAndRotation(Handle, out RVector3 position, out Quaternion rotation, lockBodies);
        return (position, rotation);
    }

    public void GetRPositionAndRotation(out RVector3 position, out Quaternion rotation, bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPositionAndRotation)}");

        JPH_Character_GetPositionAndRotation(Handle, out position, out rotation, lockBodies);
    }

    public void SetPositionAndRotation(in Vector3 position, in Quaternion rotation, Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(SetRPositionAndRotation)}");

        JPH_Character_SetPositionAndRotation(Handle, in position, in rotation, activationMode, lockBodies);
    }

    public void SetRPositionAndRotation(in RVector3 position, in Quaternion rotation, Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(SetPositionAndRotation)}");

        JPH_Character_SetPositionAndRotation(Handle, in position, in rotation, activationMode, lockBodies);
    }

    public Vector3 GetPosition(bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPosition)}");

        JPH_Character_GetPosition(Handle, out Vector3 result, lockBodies);
        return result;
    }

    public void GetPosition(out Vector3 position, bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRPosition)}");

        JPH_Character_GetPosition(Handle, out position, lockBodies);
    }

    public RVector3 GetRPosition(bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPosition)}");

        JPH_Character_GetPosition(Handle, out RVector3 result, lockBodies);
        return result;
    }

    public void GetRPosition(out RVector3 position, bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetPosition)}");

        JPH_Character_GetPosition(Handle, out position, lockBodies);
    }

    public void SetPosition(in Vector3 position, Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(SetRPosition)}");

        JPH_Character_SetPosition(Handle, in position, activationMode, lockBodies);
    }

    public void SetRPosition(in RVector3 position, Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(SetPosition)}");

        JPH_Character_SetPosition(Handle, in position, activationMode, lockBodies);
    }

    public Quaternion GetRotation(bool lockBodies = true)
    {
        JPH_Character_GetRotation(Handle, out Quaternion result, lockBodies);
        return result;
    }

    public void GetRotation(out Quaternion rotation, bool lockBodies = true)
    {
        JPH_Character_GetRotation(Handle, out rotation, lockBodies);
    }

    public void SetRotation(in Quaternion rotation, Activation activationMode = Activation.Activate, bool lockBodies = true)
    {
        JPH_Character_SetRotation(Handle, in rotation, activationMode, lockBodies);
    }

    public Vector3 GetCenterOfMassPosition(bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassPosition)}");

        JPH_Character_GetCenterOfMassPosition(Handle, out Vector3 result, lockBodies);
        return result;
    }

    public void GetCenterOfMassPosition(out Vector3 position, bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRCenterOfMassPosition)}");

        JPH_Character_GetCenterOfMassPosition(Handle, out position, lockBodies);
    }

    public RVector3 GetRCenterOfMassPosition(bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassPosition)}");

        JPH_Character_GetCenterOfMassPosition(Handle, out RVector3 result, lockBodies);
        return result;
    }

    public void GetRCenterOfMassPosition(out RVector3 position, bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetCenterOfMassPosition)}");

        JPH_Character_GetCenterOfMassPosition(Handle, out position, lockBodies);
    }

    public unsafe Matrix4x4 GetWorldTransform(bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Mat4 joltMatrix;
        JPH_Character_GetWorldTransform(Handle, &joltMatrix, lockBodies);
        return joltMatrix.FromJolt();
    }

    public unsafe void GetWorldTransform(out Matrix4x4 result, bool lockBodies = true)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldTransform)}");

        Mat4 joltMatrix;
        JPH_Character_GetWorldTransform(Handle, &joltMatrix, lockBodies);
        result = joltMatrix.FromJolt();
    }

    public RMatrix4x4 GetRWorldTransform(bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        JPH_Character_GetWorldTransform(Handle, out RMatrix4x4 result, lockBodies);
        return result;
    }

    public void GetRWorldTransform(out RMatrix4x4 result, bool lockBodies = true)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldTransform)}");

        JPH_Character_GetWorldTransform(Handle, out result, lockBodies);
    }

    public BodyID BodyID
    {
        get => JPH_Character_GetBodyID(Handle);
    }

    public ObjectLayer Layer
    {
        get => JPH_Character_GetLayer(Handle);
        set => JPH_Character_SetLayer(Handle, value, true);
    }

    public void SetLayer(in ObjectLayer layer, bool lockBodies = true)
    {
        JPH_Character_SetLayer(Handle, layer.Value, lockBodies);
    }

    public void SetShape(Shape shape, float maxPenetrationDepth, bool lockBodies = true)
    {
        JPH_Character_SetShape(Handle, shape.Handle, maxPenetrationDepth, lockBodies);
        GC.KeepAlive(shape);
    }
}
