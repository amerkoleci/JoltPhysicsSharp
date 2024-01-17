// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CharacterVirtual : CharacterBase
{
    public CharacterVirtual(CharacterVirtualSettings settings, in Double3 position, in Quaternion rotation, PhysicsSystem physicsSystem)
        : base(JPH_CharacterVirtual_Create(settings.Handle, position, rotation, physicsSystem.Handle))
    {

    }

    public Vector3 GetLinearVelocity()
    {
        JPH_CharacterVirtual_GetLinearVelocity(Handle, out Vector3 velocity);
        return velocity;
    }

    public void SetLinearVelocity(in Vector3 velocity)
    {
        JPH_CharacterVirtual_SetLinearVelocity(Handle, velocity);
    }

    public Double3 GetPosition()
    {
        JPH_CharacterVirtual_GetPosition(Handle, out Double3 position);
        return position;
    }

    public void SetPosition(in Double3 position)
    {
        JPH_CharacterVirtual_SetPosition(Handle, position);
    }

    public Quaternion GetRotation()
    {
        JPH_CharacterVirtual_GetRotation(Handle, out Quaternion rotation);
        return rotation;
    }

    public void SetRotation(in Quaternion rotation)
    {
        JPH_CharacterVirtual_SetRotation(Handle, in rotation);
    }

    public unsafe void ExtendedUpdate(float deltaTime, ExtendedUpdateSettings settings, ObjectLayer layer, PhysicsSystem physicsSystem)
    {
        JPH_CharacterVirtual_ExtendedUpdate(Handle, deltaTime, &settings, layer.Value, physicsSystem.Handle);
    }
}
