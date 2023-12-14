// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class ObjectVsBroadPhaseLayerFilterTable : NativeObject
{
    public ObjectVsBroadPhaseLayerFilterTable(BroadPhaseLayerInterfaceTable broadPhaseLayerInterface, uint numBroadPhaseLayers, ObjectLayerPairFilterTable objectLayerPairFilter, uint numObjectLayers)
        : base(JPH_ObjectVsBroadPhaseLayerFilterTable_Create(broadPhaseLayerInterface.Handle, numBroadPhaseLayers, objectLayerPairFilter.Handle, numObjectLayers))
    {
    }
}
