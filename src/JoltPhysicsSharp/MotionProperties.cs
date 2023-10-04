// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct MotionProperties : IEquatable<MotionProperties>
{
    public MotionProperties(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public bool IsNotNull => Handle != 0;
    public static MotionProperties Null => new(0);
    public static implicit operator MotionProperties(nint handle) => new(handle);
    public static bool operator ==(MotionProperties left, MotionProperties right) => left.Handle == right.Handle;
    public static bool operator !=(MotionProperties left, MotionProperties right) => left.Handle != right.Handle;
    public static bool operator ==(MotionProperties left, nint right) => left.Handle == right;
    public static bool operator !=(MotionProperties left, nint right) => left.Handle != right;
    public bool Equals(MotionProperties other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MotionProperties handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public float LinearDamping
    {
        readonly get => JPH_MotionProperties_GetLinearDamping(Handle);
        set => JPH_MotionProperties_SetLinearDamping(Handle, value);
    }

    public float AngularDamping
    {
        readonly get => JPH_MotionProperties_GetAngularDamping(Handle);
        set => JPH_MotionProperties_SetAngularDamping(Handle, value);
    }

    public float InverseMassUnchecked
    {
        get => JPH_MotionProperties_GetInverseMassUnchecked(Handle);
    }

    public void SetMassProperties(AllowedDOFs allowedDOFs, in MassProperties massProperties)
    {
        //JPH_MotionProperties_SetMassProperties(Handle, allowedDOFs, massProperties.Handle);
    }
}
