// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

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
    internal bool _fromFinalizer = false;

    protected NativeObject(nint handle, bool ownsHandle = true)
    {
        Handle = handle;
        OwnsHandle = ownsHandle;
    }

    ~NativeObject()
    {
        _fromFinalizer = true;

        Dispose(false);
    }


    /// <summary>Gets <c>true</c> if the object has been disposed; otherwise, <c>false</c>.</summary>
    public bool IsDisposed => _isDisposed != 0;

    public nint Handle
    {
        get;
        protected set
        {
            if (value == 0)
            {
                UnregisterHandle(Handle, this);
                field = value;
            }
            else
            {
                field = value;
                RegisterHandle(Handle, this);
            }
        }
    }

    public bool OwnsHandle { get; protected set; }
    protected internal bool IgnorePublicDispose { get; private set; }

    internal void PreventPublicDisposal()
    {
#if DEBUG
        // All callers (GetOrAddObject) hold the HandleDictionary upgradeable-read lock and target either a
        // freshly-created wrapper or one that GetInstanceNoLocks just confirmed !IsDisposed.
        // A live target can only become disposed via: public Dispose() (blocked here by the
        // mutually-exclusive write lock), an owned-child / ownership-handoff / replacement
        // DisposeInternal, or the finalizer. No dispose-protected call site registers its
        // singleton as an owned child, hands off its ownership, or lets it be replaced, and
        // the promoting thread holds a strong reference (so no finalizer race). Observing a
        // disposed wrapper here therefore means one of those invariants was broken — a real
        // bug worth surfacing.
        if (IsDisposed)
        {
            throw new InvalidOperationException($"Attempted to dispose-protect an already-disposed wrapper. " + $"H: {Handle.ToString("x")} Type: {GetType()}");
        }
#endif

        IgnorePublicDispose = true;
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

        // destroy the native object
        if (Handle != 0 && OwnsHandle)
            DisposeNative();

        // dispose any remaining managed-created objects
        if (disposing)
            DisposeManaged();

        Handle = 0;
    }

    protected virtual void DisposeManaged()
    {
        // dispose of any managed resources
    }

    protected virtual void DisposeNative()
    {
        // dispose of any unmanaged resources
    }

    protected nint GetHandleChecked()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (Handle == 0)
            throw new InvalidOperationException("The native handle is invalid.");

        return Handle;
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
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
