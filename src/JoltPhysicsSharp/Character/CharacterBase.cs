// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class CharacterBase : NativeObject
{
    protected CharacterBase()
    {
    }

    protected CharacterBase(nint handle)
        : base(handle)
    {
    }

    protected CharacterBase(nint handle, bool owns)
        : base(handle, owns)
    {
    }

    protected override void DisposeNative()
    {
        JPH_CharacterBase_Destroy(Handle);
    }

    public float CosMaxSlopeAngle => JPH_CharacterBase_GetCosMaxSlopeAngle(Handle);
    public float MaxSlopeAngle
    {
        set => JPH_CharacterBase_SetMaxSlopeAngle(Handle, value);
    }

    public Vector3 Up
    {
        get
        {
            JPH_CharacterBase_GetUp(Handle, out Vector3 result);
            return result;
        }
        set
        {
            JPH_CharacterBase_SetUp(Handle, in value);
        }
    }

    public GroundState GroundState
    {
        get => JPH_CharacterBase_GetGroundState(Handle);
    }

    public bool IsSupported
    {
        get => JPH_CharacterBase_IsSupported(Handle);
    }

    public Vector3 GroundPosition
    {
        get
        {
            JPH_CharacterBase_GetGroundPosition(Handle, out Vector3 position);
            return position;
        }
    }

    public Vector3 GroundNormal
    {
        get
        {
            JPH_CharacterBase_GetGroundNormal(Handle, out Vector3 normal);
            return normal;
        }
    }

    public Vector3 GroundVelocity
    {
        get
        {
            JPH_CharacterBase_GetGroundVelocity(Handle, out Vector3 velocity);
            return velocity;
        }
    }

    public uint GroundBodyId
    {
        get => JPH_CharacterBase_GetGroundBodyId(Handle);
    }

    public uint GroundSubShapeId
    {
        get => JPH_CharacterBase_GetGroundSubShapeId(Handle);
    }

    public ulong GroundUserDat
    {
        get => JPH_CharacterBase_GetGroundUserData(Handle);
    }


    public bool IsSlopeTooSteep(in Vector3 value)
    {
        return JPH_CharacterBase_IsSlopeTooSteep(Handle, in value);
    }
}
