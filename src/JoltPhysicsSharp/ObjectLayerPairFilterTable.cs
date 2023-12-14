// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class ObjectLayerPairFilterTable : NativeObject
{
    public ObjectLayerPairFilterTable(uint numObjectLayers)
        : base(JPH_ObjectLayerPairFilterTable_Create(numObjectLayers))
    {
    }

    public void DisableCollision(ObjectLayer layer1, ObjectLayer layer2)
    {
        JPH_ObjectLayerPairFilterTable_DisableCollision(Handle, layer1, layer2);
    }

    public void EnableCollision(ObjectLayer layer1, ObjectLayer layer2)
    {
        JPH_ObjectLayerPairFilterTable_EnableCollision(Handle, layer1, layer2);
    }
}
