// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ObjectVsBroadPhaseLayerFilter : NativeObject
{
    protected ObjectVsBroadPhaseLayerFilter(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_ObjectVsBroadPhaseLayerFilter_Destroy(Handle);
    }
}
