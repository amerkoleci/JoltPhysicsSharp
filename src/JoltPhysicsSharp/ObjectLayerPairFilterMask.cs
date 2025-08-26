// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class ObjectLayerPairFilterMask : ObjectLayerPairFilter
{
    public const int NumBits = ObjectLayer.Bits / 2;
    public const int Mask = (1 << NumBits) - 1;

    internal ObjectLayerPairFilterMask(nint handle, bool owns)
        : base(handle, owns)
    {
    }

    public ObjectLayerPairFilterMask()
        : this(JPH_ObjectLayerPairFilterMask_Create(), true)
    {
    }

    public static ObjectLayer GetObjectLayer(uint group, uint mask = Mask)
    {
        return JPH_ObjectLayerPairFilterMask_GetObjectLayer(group, mask);
    }

    public static uint GetGroup(in ObjectLayer layer)
    {
        return JPH_ObjectLayerPairFilterMask_GetGroup(layer.Value);
    }

    public static uint GetMask(in ObjectLayer layer)
    {
        return JPH_ObjectLayerPairFilterMask_GetMask(layer.Value);
    }
}
