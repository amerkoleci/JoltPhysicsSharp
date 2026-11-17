// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;
public struct CharacterContact
{
    public ulong Hash;
    public BodyID BodyB;
    public CharacterID CharacterIDB;
    public SubShapeID SubShapeIDB;
    public Vector3 Position;
    public RVector3 RPosition;
    public Vector3 LinearVelocity;
    public Vector3 ContactNormal;
    public Vector3 SurfaceNormal;
    public float Distance;
    public float Fraction;
    public MotionType MotionTypeB;
    public bool IsSensorB;
    public CharacterVirtual? CharacterB;
    public ulong UserData;
    public PhysicsMaterial? Material;
    public bool HadCollision;
    public bool WasDiscarded;
    public bool CanPushCharacter;
    public bool IsBackFacingContact;

    internal unsafe void FromNative(JPH_CharacterContact* native)
    {
        if (DoublePrecision)
        {
            FromNative((JPH_CharacterContactDouble*)native);
            return;
        }

        Hash = native->hash;
        BodyB = native->bodyB;
        CharacterIDB = native->characterIDB;
        SubShapeIDB = native->subShapeIDB;
        Position = native->position;
        RPosition = native->position;
        LinearVelocity = native->linearVelocity;
        ContactNormal = native->contactNormal;
        SurfaceNormal = native->surfaceNormal;
        Distance = native->distance;
        Fraction = native->fraction;
        MotionTypeB = native->motionTypeB;
        IsSensorB = native->isSensorB;
        CharacterB = CharacterVirtual.GetObject(native->characterB);
        UserData = native->userData;
        Material = native->material;
        HadCollision = native->hadCollision;
        WasDiscarded = native->wasDiscarded;
        CanPushCharacter = native->canPushCharacter;
        IsBackFacingContact = native->isBackFacingContact;
    }

    internal unsafe void FromNative(JPH_CharacterContactDouble* native)
    {
        Hash = native->hash;
        BodyB = native->bodyB;
        CharacterIDB = native->characterIDB;
        SubShapeIDB = native->subShapeIDB;
        Position = (Vector3)native->position;
        RPosition = native->position;
        LinearVelocity = native->linearVelocity;
        ContactNormal = native->contactNormal;
        SurfaceNormal = native->surfaceNormal;
        Distance = native->distance;
        Fraction = native->fraction;
        MotionTypeB = native->motionTypeB;
        IsSensorB = native->isSensorB;
        CharacterB = CharacterVirtual.GetObject(native->characterB);
        UserData = native->userData;
        Material = native->material;
        HadCollision = native->hadCollision;
        WasDiscarded = native->wasDiscarded;
        CanPushCharacter = native->canPushCharacter;
        IsBackFacingContact = native->isBackFacingContact;
    }
}
