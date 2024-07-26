// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// A native object.
/// </summary>
public abstract class NativeObject : DisposableObject
{
    protected static readonly Dictionary<nint, WeakReference> s_instances = [];
    internal static readonly IPlatformLock s_instancesLock = PlatformLock.Create();
#if DEBUG
    internal static readonly Dictionary<IntPtr, string> s_stackTraces = [];
#endif
    public nint Handle { get; protected set; }

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
        RegisterObject(handle, this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            UnregisterObject(Handle, this);
        }
    }

    protected internal void DisposeInternal()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static void RegisterObject(nint handle, NativeObject instance)
    {
        NativeObject? objectToDispose = null;

        s_instancesLock.EnterWriteLock();
        try
        {
            if (s_instances.TryGetValue(handle, out var oldValue)
                && oldValue.Target is NativeObject obj && !obj.IsDisposed)
            {
                // this means the ownership was handed off to a native object, so clean up the managed side
                objectToDispose = obj;
            }

            s_instances[handle] = new WeakReference(instance);
#if DEBUG
            s_stackTraces[handle] = Environment.StackTrace;
#endif
        }
        finally
        {
            s_instancesLock.ExitWriteLock();
        }

        // dispose the object we just replaced
        objectToDispose?.DisposeInternal();
    }

    public static void UnregisterObject(nint handle, NativeObject instance)
    {
        s_instancesLock.EnterWriteLock();
        try
        {
            var existed = s_instances.TryGetValue(handle, out WeakReference? weak);
            if (existed && (!weak!.IsAlive || weak.Target == instance))
            {
                s_instances.Remove(handle);
#if DEBUG
                s_stackTraces.Remove(handle);
#endif
            }
            else
            {
            }
        }
        finally
        {
            s_instancesLock.ExitWriteLock();
        }
    }

    internal static TNativeObject? GetOrAddObject<TNativeObject>(nint handle, Func<IntPtr, TNativeObject> objectFactory)
        where TNativeObject : NativeObject
    {
        if (handle == IntPtr.Zero)
            return null;

        s_instancesLock.EnterUpgradeableReadLock();
        try
        {
            if (GetInstanceNoLocks(handle, out TNativeObject? instance))
            {
                // TODO: Add support for RefCount from Jolt
                return instance;
            }

            TNativeObject obj = objectFactory.Invoke(handle);

            return obj;
        }
        finally
        {
            s_instancesLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Retrieve the living instance if there is one, or null if not. This does not use locks.
    /// </summary>
    /// <returns>The instance if it is alive, or null if there is none.</returns>
    private static bool GetInstanceNoLocks<TNativeObject>(nint handle, out TNativeObject? instance)
        where TNativeObject : NativeObject
    {
        if (s_instances.TryGetValue(handle, out WeakReference? weak) && weak.IsAlive)
        {
            if (weak.Target is TNativeObject match)
            {
                if (!match.IsDisposed)
                {
                    instance = match;
                    return true;
                }
            }
        }

        instance = null;
        return false;
    }
}
