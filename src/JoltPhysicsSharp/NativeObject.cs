// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Collections.Concurrent;

namespace JoltPhysicsSharp;

/// <summary>
/// A native object which is disposable.
/// </summary>
/// <remarks>
/// Disposal must be externally synchronized with native operations. In particular, do not
/// dispose or configure collision filters concurrently with physics system construction.
/// </remarks>
public abstract class NativeObject : IDisposable
{
    private volatile uint _isDisposed;
    internal bool fromFinalizer = false;
    private nint _handle;
    private bool _ownsHandle;
    private readonly object _locker = new();
    private ConcurrentDictionary<nint, NativeObject>? _ownedObjects;

    protected NativeObject()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeObject" /> class.
    /// <param name="handle">The handle to initialize width.</param>
    /// </summary>
    internal NativeObject(nint handle)
        : this(handle, true)
    {
    }

    internal NativeObject(nint handle, bool ownsHandle)
    {
        Handle = handle;
        OwnsHandle = ownsHandle;
    }

    ~NativeObject()
    {
        fromFinalizer = true;

        Dispose(false);
    }


    /// <summary>Gets <c>true</c> if the object has been disposed; otherwise, <c>false</c>.</summary>
    public bool IsDisposed => _isDisposed != 0;
    protected internal bool IgnorePublicDispose { get; set; }

    internal ConcurrentDictionary<nint, NativeObject> OwnedObjects
    {
        get
        {
            if (_ownedObjects == null)
            {
                lock (_locker)
                {
                    _ownedObjects ??= new();
                }
            }
            return _ownedObjects;
        }
    }

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

    protected internal virtual bool OwnsHandle
    {
        get => _ownsHandle;
        protected set => _ownsHandle = value;
    }
    internal bool IsOwnershipTransferred { get; private set; }

    internal nint GetHandleChecked()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (Handle == 0)
            throw new InvalidOperationException("The native handle is invalid.");

        return Handle;
    }

    internal nint GetHandleForConfiguration()
    {
        nint handle = GetHandleChecked();
        if (IsOwnershipTransferred)
            throw new InvalidOperationException("The filter belongs to a physics system and can no longer be configured.");

        return handle;
    }

    // The native owner must invalidate this wrapper when it destroys the handle.
    internal void TransferOwnership()
    {
        IsOwnershipTransferred = true;
        IgnorePublicDispose = true;
        GC.SuppressFinalize(this);
        _ownsHandle = false;
    }

    // joltc owns the allocation once native system creation succeeds.
    // Clear the handle before managed cleanup, without calling a native destructor.
    internal void InvalidateHandle(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
            return;

        Handle = 0;
        GC.SuppressFinalize(this);
        if (disposing)
        {
            try
            {
                DisposeUnownedManaged();
            }
            finally
            {
                DisposeManaged();
            }
        }
    }

    public void Dispose()
    {
        if (IgnorePublicDispose)
            return;

        DisposeInternal();
    }

    protected internal void DisposeInternal()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
            return;

        // dispose any objects that are owned/created by native code
        if (disposing)
            DisposeUnownedManaged();

        // destroy the native object
        if (Handle != IntPtr.Zero && !IsOwnershipTransferred && OwnsHandle)
            DisposeNative();

        // dispose any remaining managed-created objects
        if (disposing)
            DisposeManaged();

        Handle = IntPtr.Zero;
    }

    private void DisposeUnownedManaged()
    {
        if (_ownedObjects != null)
        {
            foreach (var child in _ownedObjects)
            {
                if (child.Value is NativeObject c && !c.OwnsHandle)
                    c.DisposeInternal();
            }
        }
    }

    protected virtual void DisposeManaged()
    {
        // dispose of any managed resources
    }

    protected virtual void DisposeNative()
    {
        // dispose of any unmanaged resources
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

    internal static TNativeObject? GetOrAddObject<TNativeObject>(nint handle)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
        {
            return null;
        }

        return HandleDictionary.GetOrAddObject<TNativeObject>(handle);
    }

    internal static TNativeObject? GetOrAddObject<TNativeObject>(nint handle, Func<nint, TNativeObject> objectFactory)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
        {
            return null;
        }

        return HandleDictionary.GetOrAddObject(handle, objectFactory);
    }
}
