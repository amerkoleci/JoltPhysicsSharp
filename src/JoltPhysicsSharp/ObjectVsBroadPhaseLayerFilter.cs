// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public abstract class ObjectVsBroadPhaseLayerFilter
{
    protected ObjectVsBroadPhaseLayerFilter(nint handle)
    {
        Handle = handle;
    }

    public nint Handle { get; }
}
