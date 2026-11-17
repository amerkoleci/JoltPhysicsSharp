// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class ObjectVsBroadPhaseLayerFilterMask : ObjectVsBroadPhaseLayerFilter
{
    public ObjectVsBroadPhaseLayerFilterMask(BroadPhaseLayerInterface broadPhaseLayerInterface)
        : base(JPH_ObjectVsBroadPhaseLayerFilterMask_Create(broadPhaseLayerInterface.Handle))
    {
    }
}
