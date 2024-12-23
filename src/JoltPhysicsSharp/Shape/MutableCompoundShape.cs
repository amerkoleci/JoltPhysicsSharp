// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class MutableCompoundShapeSettings : CompoundShapeShapeSettings
{
    public MutableCompoundShapeSettings()
        : base(JPH_MutableCompoundShapeSettings_Create())
    {
    }

    public override Shape Create() => new MutableCompoundShape(this);
}

public sealed class MutableCompoundShape : CompoundShape
{
    public MutableCompoundShape(MutableCompoundShapeSettings settings)
        : base(JPH_MutableCompoundShape_Create(settings.Handle))
    {
    }

    public uint AddShape(in Vector3 position, in Quaternion rotation, Shape child, uint userData = 0, uint index = uint.MaxValue)
    {
        return JPH_MutableCompoundShape_AddShape(Handle, in position, in rotation, child.Handle, userData, index);
    }

    public void RemoveShape(uint index)
    {
        JPH_MutableCompoundShape_RemoveShape(Handle, index);
    }

    public void ModifyShape(uint index,in Vector3 position, in Quaternion rotation)
    {
        JPH_MutableCompoundShape_ModifyShape(Handle, index, in position, in rotation);
    }

    public void ModifyShape(uint index, in Vector3 position, in Quaternion rotation, Shape newShape)
    {
        JPH_MutableCompoundShape_ModifyShape2(Handle, index, in position, in rotation, newShape.Handle);
    }

    public void AdjustCenterOfMass()
    {
        JPH_MutableCompoundShape_AdjustCenterOfMass(Handle);
    }
}
