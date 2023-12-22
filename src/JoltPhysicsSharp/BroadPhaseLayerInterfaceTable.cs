// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class BroadPhaseLayerInterfaceTable : BroadPhaseLayerInterface
{
    public BroadPhaseLayerInterfaceTable(uint numObjectLayers, uint numBroadPhaseLayers)
        : base(JPH_BroadPhaseLayerInterfaceTable_Create(numObjectLayers, numBroadPhaseLayers))
    {
    }

    public void MapObjectToBroadPhaseLayer(ObjectLayer objectLayer, BroadPhaseLayer broadPhaseLayer)
    {
        JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(Handle, objectLayer, broadPhaseLayer);
    }
}
