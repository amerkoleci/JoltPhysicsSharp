// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly partial struct PhysicsMaterial : IDisposable
{
    public readonly nint Handle;

    public bool IsNull => Handle is 0;

    public static PhysicsMaterial Null => new(0);

    public PhysicsMaterial(string name, in JoltColor color)
        : this(JPH_PhysicsMaterial_Create(name, color.PackedValue))
    {
    }

    private PhysicsMaterial(nint handle)
    {
        Handle = handle;
    }

    public void Dispose()
    {
        JPH_PhysicsMaterial_Destroy(Handle);
    }

    public string? DebugName => JPH_PhysicsMaterial_GetDebugName(Handle);
    public uint DebugColor => JPH_PhysicsMaterial_GetDebugColor(Handle);

    public static implicit operator PhysicsMaterial(nint handle) => new(handle);
}
