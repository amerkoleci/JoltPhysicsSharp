// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class GroupFilterTable : GroupFilter
{
    public GroupFilterTable(uint numSubGroups = 0)
        : base(JPH_GroupFilterTable_Create(numSubGroups))
    {
    }

    internal GroupFilterTable(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    public void DisableCollision(CollisionSubGroupID subGroup1, CollisionSubGroupID subGroup2)
    {
        JPH_GroupFilterTable_DisableCollision(Handle, subGroup1, subGroup2);
    }

    public void EnableCollision(CollisionSubGroupID subGroup1, CollisionSubGroupID subGroup2)
    {
        JPH_GroupFilterTable_EnableCollision(Handle, subGroup1, subGroup2);
    }
    public bool IsCollisionEnabled(CollisionSubGroupID subGroup1, CollisionSubGroupID subGroup2)
    {
        return JPH_GroupFilterTable_IsCollisionEnabled(Handle, subGroup1, subGroup2);
    }
}
