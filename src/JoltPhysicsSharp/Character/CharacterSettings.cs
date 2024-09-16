// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CharacterSettings : CharacterBaseSettings
{
    public CharacterSettings()
        : base(JPH_CharacterSettings_Create())
    {
    }

    public ObjectLayer Layer
    {
        get => JPH_CharacterSettings_GetLayer(Handle);
        set => JPH_CharacterSettings_SetLayer(Handle, value);
    }

    public float Mass
    {
        get => JPH_CharacterSettings_GetMass(Handle);
        set => JPH_CharacterSettings_SetMass(Handle, value);
    }

    public float Friction
    {
        get => JPH_CharacterSettings_GetFriction(Handle);
        set => JPH_CharacterSettings_SetFriction(Handle, value);
    }

    public float GravityFactor
    {
        get => JPH_CharacterSettings_GetGravityFactor(Handle);
        set => JPH_CharacterSettings_SetGravityFactor(Handle, value);
    }
}

