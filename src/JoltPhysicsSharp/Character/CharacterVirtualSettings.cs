// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CharacterVirtualSettings : CharacterBaseSettings
{
    public CharacterVirtualSettings()
        : base(JPH_CharacterVirtualSettings_Create())
    {
    }

    public float Mass
    {
        get => JPH_CharacterVirtualSettings_GetMass(Handle);
        set => JPH_CharacterVirtualSettings_SetMass(Handle, value);
    }

    public float MaxStrength
    {
        get => JPH_CharacterVirtualSettings_GetMaxStrength(Handle);
        set => JPH_CharacterVirtualSettings_SetMaxStrength(Handle, value);
    }

    public Vector3 ShapeOffset
    {
        get
        {
            JPH_CharacterVirtualSettings_GetShapeOffset(Handle, out Vector3 result);
            return result;
        }
        set
        {
            JPH_CharacterVirtualSettings_SetShapeOffset(Handle, in value);
        }
    }

    public BackFaceMode BackFaceMode
    {
        get => JPH_CharacterVirtualSettings_GetBackFaceMode(Handle);
        set => JPH_CharacterVirtualSettings_SetBackFaceMode(Handle, value);
    }

    public float PredictiveContactDistance
    {
        get => JPH_CharacterVirtualSettings_GetPredictiveContactDistance(Handle);
        set => JPH_CharacterVirtualSettings_SetPredictiveContactDistance(Handle, value);
    }

    public uint MaxCollisionIterations
    {
        get => JPH_CharacterVirtualSettings_GetMaxCollisionIterations(Handle);
        set => JPH_CharacterVirtualSettings_SetMaxCollisionIterations(Handle, value);
    }

    public uint MaxConstraintIterations
    {
        get => JPH_CharacterVirtualSettings_GetMaxConstraintIterations(Handle);
        set => JPH_CharacterVirtualSettings_SetMaxConstraintIterations(Handle, value);
    }

    public float MinTimeRemaining
    {
        get => JPH_CharacterVirtualSettings_GetMinTimeRemaining(Handle);
        set => JPH_CharacterVirtualSettings_SetMinTimeRemaining(Handle, value);
    }

    public float CollisionTolerance
    {
        get => JPH_CharacterVirtualSettings_GetCollisionTolerance(Handle);
        set => JPH_CharacterVirtualSettings_SetCollisionTolerance(Handle, value);
    }

    public float CharacterPadding
    {
        get => JPH_CharacterVirtualSettings_GetCharacterPadding(Handle);
        set => JPH_CharacterVirtualSettings_SetCharacterPadding(Handle, value);
    }

    public uint MaxNumHits
    {
        get => JPH_CharacterVirtualSettings_GetMaxNumHits(Handle);
        set => JPH_CharacterVirtualSettings_SetMaxNumHits(Handle, value);
    }

    public float HitReductionCosMaxAngle
    {
        get => JPH_CharacterVirtualSettings_GetHitReductionCosMaxAngle(Handle);
        set => JPH_CharacterVirtualSettings_SetHitReductionCosMaxAngle(Handle, value);
    }

    public float PenetrationRecoverySpeed
    {
        get => JPH_CharacterVirtualSettings_GetPenetrationRecoverySpeed(Handle);
        set => JPH_CharacterVirtualSettings_SetPenetrationRecoverySpeed(Handle, value);
    }
}

