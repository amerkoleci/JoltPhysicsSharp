// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class TempAllocator : NativeObject
{
    public TempAllocator(int size)
        : base(JPH_TempAllocator_Create((uint)size))
    {
    }

    /// <summary>Finalizes an instance of the <see cref="TempAllocator" /> class.</summary>
    ~TempAllocator() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_TempAllocator_Destroy(Handle);
        }
    }
}
