// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class PhysicsMaterial : NativeObject
{
    public PhysicsMaterial(string name, in Color color)
        : base(JPH_PhysicsMaterial_Create(name, color.PackedValue))    
    {
    }

    internal PhysicsMaterial(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ShapeSettings" /> class.
    /// </summary>
    ~PhysicsMaterial() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_PhysicsMaterial_Destroy(Handle);
        }
    }

    public string? DebugName => JPH_PhysicsMaterial_GetDebugName(Handle);
    public uint DebugColor => JPH_PhysicsMaterial_GetDebugColor(Handle);

    internal static PhysicsMaterial? GetObject(nint handle)
    {
        return GetOrAddObject(handle, (nint h) => new PhysicsMaterial(h));
    }
}
