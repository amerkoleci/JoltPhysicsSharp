// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SoftBodyCreationSettings : NativeObject
{
    public SoftBodyCreationSettings()
        : base(JPH_SoftBodyCreationSettings_Create())
    {
    }

    protected override void DisposeNative()
    {
        JPH_SoftBodyCreationSettings_Destroy(Handle);
    }
}
