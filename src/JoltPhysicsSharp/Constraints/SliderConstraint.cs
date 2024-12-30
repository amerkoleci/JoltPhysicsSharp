// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SliderConstraintSettings : TwoBodyConstraintSettings
{
    public SliderConstraintSettings()
        : base(JPH_SliderConstraintSettings_Create())
    {
    }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new SliderConstraint(JPH_SliderConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }

    public unsafe void SetSliderAxis(Vector3 axis)
    {
        JPH_SliderConstraintSettings_SetSliderAxis(Handle, &axis);
    }
}

public sealed class SliderConstraint : TwoBodyConstraint
{
    internal SliderConstraint(nint handle)
        : base(handle)
    {
    }

    public SliderConstraint(SliderConstraintSettings settings, in Body body1, in Body body2)
        : base(JPH_SliderConstraintSettings_CreateConstraint(settings.Handle, body1.Handle, body2.Handle))
    {
    }
}
