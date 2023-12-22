// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public abstract class ObjectVsBroadPhaseLayerFilter : NativeObject
{
    protected ObjectVsBroadPhaseLayerFilter(nint handle)
        : base(handle)
    {
    }
}
