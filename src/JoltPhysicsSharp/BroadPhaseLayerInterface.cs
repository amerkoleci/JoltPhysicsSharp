// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BroadPhaseLayerInterface : NativeReferencableObject
{
    protected BroadPhaseLayerInterface(nint handle)
        : base(handle)
    {
    }

    protected override void Destroy()
    {
        JPH_BroadPhaseLayerInterface_Destroy(Handle);
    }
}
