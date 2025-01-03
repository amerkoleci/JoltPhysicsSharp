// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class CharacterVsCharacterCollision : NativeObject
{
    protected CharacterVsCharacterCollision(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_CharacterVsCharacterCollision_Destroy(Handle);
    }
}

public abstract class CharacterVsCharacterCollisionListener : NativeObject
{
    private readonly unsafe JPH_CharacterVsCharacterCollision_Procs _listener_Procs = new()
    {
        CollideCharacter = &OnCollideCharacterCallback,
        CastCharacter = &OnCastCharacterCallback,
    };

    public CharacterVsCharacterCollisionListener()
    {
        nint listenerContext = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_CharacterVsCharacterCollision_Create(_listener_Procs, listenerContext);
    }

    protected abstract void CollideCharacter(
        CharacterVirtual character,
        in Matrix4x4 centerOfMassTransform,
        in CollideShapeSettings collideShapeSettings,
        in Vector3 baseOffset/*, CollideShapeCollector &ioCollector*/);

    protected abstract void CastCharacter(
        CharacterVirtual character,
        in Matrix4x4 centerOfMassTransform,
        in Vector3 direction,
        in ShapeCastSettings collideShapeSettings,
        in Vector3 baseOffset/*, CastShapeCollector  &ioCollector*/);

    #region CharacterContactListener
    [UnmanagedCallersOnly]
    private static unsafe void OnCollideCharacterCallback(nint context,
        nint character, Matrix4x4* centerOfMassTransform,
        JPH_CollideShapeSettings* collideShapeSettings,
        Vector3* baseOffset)
    {
        CharacterVsCharacterCollisionListener listener = DelegateProxies.GetUserData<CharacterVsCharacterCollisionListener>(context, out _);

        listener.CollideCharacter(
            CharacterVirtual.GetObject(character)!,
            *centerOfMassTransform,
            CollideShapeSettings.FromNative(*collideShapeSettings),
            *baseOffset
            );
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnCastCharacterCallback(nint context,
        nint character, Matrix4x4* centerOfMassTransform,
        Vector3* direction,
        JPH_ShapeCastSettings* collideShapeSettings,
        Vector3* baseOffset)
    {
        CharacterVsCharacterCollisionListener listener = DelegateProxies.GetUserData<CharacterVsCharacterCollisionListener>(context, out _);

        listener.CastCharacter(
            CharacterVirtual.GetObject(character)!,
            *centerOfMassTransform,
            *direction,
            ShapeCastSettings.FromNative(*collideShapeSettings),
            *baseOffset
            );
    }
    #endregion
}

public sealed class CharacterVsCharacterCollisionSimple : CharacterVsCharacterCollision
{
    public CharacterVsCharacterCollisionSimple()
        : base(JPH_CharacterVsCharacterCollision_CreateSimple())
    {

    }

    public void Add(CharacterVirtual character)
    {
        JPH_CharacterVsCharacterCollisionSimple_AddCharacter(Handle, character.Handle);
    }

    public void Remove(CharacterVirtual character)
    {
        JPH_CharacterVsCharacterCollisionSimple_RemoveCharacter(Handle, character.Handle);
    }
}
