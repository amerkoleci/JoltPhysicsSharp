// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// A native object.
/// </summary>
public abstract class NativeObject : DisposableObject
{
    private nint _handle;

    public nint Handle
    {
        get => _handle;
        protected set
        {
            if (value == 0)
            {
                UnregisterHandle(Handle, this);
                _handle = value;
            }
            else
            {
                _handle = value;
                RegisterHandle(Handle, this);
            }
        }
    }

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

    internal static void RegisterHandle(nint handle, NativeObject? instance)
    {
        if (handle != 0 && instance != null)
        {
            HandleDictionary.RegisterHandle(handle, instance);
        }
    }

    internal static void UnregisterHandle(nint handle, NativeObject instance)
    {
        if (handle != 0)
        {
            HandleDictionary.UnregisterHandle(handle, instance);
        }
    }

    internal static TNativeObject? GetOrAddObject<TNativeObject>(nint handle, Func<nint, TNativeObject> objectFactory)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
        {
            return null;
        }

        return HandleDictionary.GetOrAddObject(handle, unrefExisting: true, objectFactory);
    }

}
