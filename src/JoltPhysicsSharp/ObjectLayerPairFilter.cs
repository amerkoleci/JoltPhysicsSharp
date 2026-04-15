// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ObjectLayerPairFilter : NativeObject
{
    protected ObjectLayerPairFilter(nint handle, bool owns)
        : base(handle, owns)
    {
    }

    protected override void DisposeNative()
    {
        JPH_ObjectLayerPairFilter_Destroy(Handle);
    }
}
