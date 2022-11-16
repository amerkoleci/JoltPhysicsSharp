// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// A native object.
/// </summary>
public abstract class NativeObject : DisposableObject
{
    protected NativeObject()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeObject" /> class.
    /// <param name="handle">The handle to initialize width.</param>
    /// </summary>
    protected NativeObject(nint handle)
    {
        Handle = handle;
    }

    public nint Handle { get; protected set; }
}
