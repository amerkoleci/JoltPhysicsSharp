// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class StaticCompoundShapeSettings : CompoundShapeShapeSettings
{
    public StaticCompoundShapeSettings()
        : base(JPH_StaticCompoundShapeSettings_Create())
    {
    }

    public override Shape Create() => new StaticCompoundShape(this);
}

public sealed class StaticCompoundShape : CompoundShape
{
    internal StaticCompoundShape(nint handle)
        : base(handle)
    {
    }

    public StaticCompoundShape(StaticCompoundShapeSettings settings)
        : base(JPH_StaticCompoundShape_Create(settings.Handle))
    {
    }
}
