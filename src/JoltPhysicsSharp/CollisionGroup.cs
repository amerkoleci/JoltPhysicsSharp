// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public partial struct CollisionGroup
{
    public GroupFilter? GroupFilter = default;
    public CollisionGroupID GroupID = CollisionGroupID.Invalid;
    public CollisionSubGroupID SubGroupID = CollisionSubGroupID.Invalid;

    public CollisionGroup()
    {
    }

    public CollisionGroup(GroupFilter? groupFilter, CollisionGroupID groupID, CollisionSubGroupID subGroupID)
    {
        GroupFilter = groupFilter;
        GroupID = groupID;
        SubGroupID = subGroupID;
    }

    /// <summary>
    /// Check if this object collides with another object
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
	public readonly bool CanCollide(in CollisionGroup other)
    {
        // Call the CanCollide function of the first group filter that's not null
        if (GroupFilter != null)
            return GroupFilter.CanCollide(this, other);
        else if (other.GroupFilter != null)
            return other.GroupFilter.CanCollide(other, this);
        else
            return true;
    }

    internal void ToNative(out JPH_CollisionGroup result)
    {
        result = new JPH_CollisionGroup
        {
            groupFilter = (GroupFilter != null) ? GroupFilter.Handle : 0,
            groupID = GroupID,
            subGroupID = SubGroupID
        };
    }

    internal static CollisionGroup FromNative(in JPH_CollisionGroup group)
    {
        return new(GroupFilter.GetObject(group.groupFilter), group.groupID, group.subGroupID);
    }
}
