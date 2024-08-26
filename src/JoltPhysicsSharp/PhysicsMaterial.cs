// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class PhysicsMaterial : NativeObject
{
    public PhysicsMaterial()
        : base(JPH_PhysicsMaterial_Create())    
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
}
