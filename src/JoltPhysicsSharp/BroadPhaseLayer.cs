// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class BroadPhaseLayer : NativeObject
{
    public BroadPhaseLayer()
        : base(JPH_BroadPhaseLayer_Create())
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BroadPhaseLayer" /> class.
    /// </summary>
    ~BroadPhaseLayer() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_BroadPhaseLayer_Destroy(Handle);
        }
    }
}
