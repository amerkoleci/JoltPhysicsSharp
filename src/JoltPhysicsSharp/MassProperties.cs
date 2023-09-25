// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct MassProperties : IEquatable<MassProperties>
{
    public MassProperties(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public bool IsNotNull => Handle != 0;
    public static MassProperties Null => new(0);
    public static implicit operator MassProperties(nint handle) => new(handle);
    public static bool operator ==(MassProperties left, MassProperties right) => left.Handle == right.Handle;
    public static bool operator !=(MassProperties left, MassProperties right) => left.Handle != right.Handle;
    public static bool operator ==(MassProperties left, nint right) => left.Handle == right;
    public static bool operator !=(MassProperties left, nint right) => left.Handle != right;
    public bool Equals(MassProperties other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MassProperties handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public void ScaleToMass(float mass)
    {
        JPH_MassProperties_ScaleToMass(Handle, mass);
    }
}
