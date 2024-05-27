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

    /// <summary>
    /// Finalizes an instance of the <see cref="SoftBodyCreationSettings" /> class.
    /// </summary>
    ~SoftBodyCreationSettings() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_SoftBodyCreationSettings_Destroy(Handle);
        }
    }
}
