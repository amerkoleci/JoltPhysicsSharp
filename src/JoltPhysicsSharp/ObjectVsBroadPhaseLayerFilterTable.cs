// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class ObjectVsBroadPhaseLayerFilterTable : ObjectVsBroadPhaseLayerFilter
{
    public ObjectVsBroadPhaseLayerFilterTable(
        BroadPhaseLayerInterface broadPhaseLayerInterface, uint numBroadPhaseLayers,
        ObjectLayerPairFilter objectLayerPairFilter, uint numObjectLayers)
        : base(JPH_ObjectVsBroadPhaseLayerFilterTable_Create(broadPhaseLayerInterface.Handle, numBroadPhaseLayers, objectLayerPairFilter.Handle, numObjectLayers))
    {
    }
}
