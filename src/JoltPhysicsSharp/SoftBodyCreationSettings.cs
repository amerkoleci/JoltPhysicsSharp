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
    ~SoftBodyCreationSettings() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_SoftBodyCreationSettings_Destroy(Handle);
        }
    }
}
