// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly unsafe struct ContactSettings(nint handle) : IEquatable<ContactSettings>
{
    public nint Handle { get; } = handle; public bool IsNull => Handle == 0;
    public static ContactSettings Null => new(0);
    public static implicit operator ContactSettings(nint handle) => new(handle);
    public static bool operator ==(ContactSettings left, ContactSettings right) => left.Handle == right.Handle;
    public static bool operator !=(ContactSettings left, ContactSettings right) => left.Handle != right.Handle;
    public static bool operator ==(ContactSettings left, nint right) => left.Handle == right;
    public static bool operator !=(ContactSettings left, nint right) => left.Handle != right;
    public bool Equals(ContactSettings other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ContactSettings handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public float Friction
    {
        readonly get => JPH_ContactSettings_GetFriction(Handle);
        set => JPH_ContactSettings_SetFriction(Handle, value);
    }

    public float Restitution
    {
        readonly get => JPH_ContactSettings_GetRestitution(Handle);
        set => JPH_ContactSettings_SetRestitution(Handle, value);
    }

    public float InvMassScale1
    {
        readonly get => JPH_ContactSettings_GetInvMassScale1(Handle);
        set => JPH_ContactSettings_SetInvMassScale1(Handle, value);
    }

    public float InvInertiaScale1
    {
        readonly get => JPH_ContactSettings_GetInvInertiaScale1(Handle);
        set => JPH_ContactSettings_SetInvInertiaScale1(Handle, value);
    }

    public float InvMassScale2
    {
        readonly get => JPH_ContactSettings_GetInvMassScale2(Handle);
        set => JPH_ContactSettings_SetInvMassScale2(Handle, value);
    }

    public float InvInertiaScale2
    {
        readonly get => JPH_ContactSettings_GetInvInertiaScale2(Handle);
        set => JPH_ContactSettings_SetInvInertiaScale2(Handle, value);
    }

    public bool IsSensor
    {
        get => JPH_ContactSettings_GetIsSensor(Handle);
        set => JPH_ContactSettings_SetIsSensor(Handle, value);
    }

    public Vector3 RelativeLinearSurfaceVelocity
    {
        get
        {
            Vector3 result;
            JPH_ContactSettings_GetRelativeLinearSurfaceVelocity(Handle, &result);
            return result;
        }
        set
        {
            JPH_ContactSettings_SetRelativeLinearSurfaceVelocity(Handle, &value);
        }
    }

    public Vector3 RelativeAngularSurfaceVelocity
    {
        get
        {
            Vector3 result;
            JPH_ContactSettings_GetRelativeAngularSurfaceVelocity(Handle, &result);
            return result;
        }
        set
        {
            JPH_ContactSettings_SetRelativeAngularSurfaceVelocity(Handle, &value);
        }
    }
}
