// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;


public unsafe class GroupFilter : NativeObject
{
    internal GroupFilter(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    protected override void DisposeNative()
    {
        JPH_GroupFilter_Destroy(Handle);
    }

    public bool CanCollide(in CollisionGroup group1, in CollisionGroup group2)
    {
        group1.ToNative(out JPH_CollisionGroup group1Native);
        group2.ToNative(out JPH_CollisionGroup group2Native);
        return JPH_GroupFilter_CanCollide(Handle, &group1Native, &group2Native);
    }

    internal static GroupFilter? GetObject(nint handle)
    {
        return GetOrAddObject(handle, (nint h) => new GroupFilter(h, false));
    }
}
