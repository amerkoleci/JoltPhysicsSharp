// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// see: https://github.com/mono/SkiaSharp/blob/main/binding/SkiaSharp/HandleDictionary.cs

namespace JoltPhysicsSharp;

internal static class HandleDictionary
{
    internal static readonly IPlatformLock s_instancesLock = PlatformLock.Create();
    internal static readonly Dictionary<IntPtr, WeakReference> s_instances = [];
#if DEBUG
    internal static readonly Dictionary<IntPtr, string> s_stackTraces = [];
#endif

    /// <summary>
    /// Retrieve the living instance if there is one, or null if not.
    /// </summary>
    /// <returns>The instance if it is alive, or null if there is none.</returns>
    public static bool GetInstance<TNativeObject>(nint handle, out TNativeObject? instance)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
        {
            instance = null;
            return false;
        }

        s_instancesLock.EnterReadLock();
        try
        {
            return GetInstanceNoLocks(handle, out instance);
        }
        finally
        {
            s_instancesLock.ExitReadLock();
        }
    }

    /// <summary>
    /// Retrieve or create an instance for the native handle.
    /// </summary>
    /// <returns>The instance, or null if the handle was null.</returns>
    public static TNativeObject? GetOrAddObject<TNativeObject>(nint handle, bool unrefExisting, Func<IntPtr, TNativeObject> objectFactory)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
            return default;

        s_instancesLock.EnterUpgradeableReadLock();
        try
        {
            if (GetInstanceNoLocks<TNativeObject>(handle, out TNativeObject? instance))
            {
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
    /// Registers the specified instance with the dictionary.
    /// </summary>
    public static void RegisterHandle(nint handle, NativeObject instance)
    {
        if (handle == 0 || instance == null)
            return;

        NativeObject? objectToDispose = null;

        s_instancesLock.EnterWriteLock();
        try
        {
            if (s_instances.TryGetValue(handle, out WeakReference? oldValue) && oldValue.Target is NativeObject obj && !obj.IsDisposed)
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

    /// <summary>
    /// Removes the registered instance from the dictionary.
    /// </summary>
    public static void UnregisterHandle(nint handle, NativeObject instance)
    {
        if (handle == 0)
            return;

        s_instancesLock.EnterWriteLock();
        try
        {
            bool existed = s_instances.TryGetValue(handle, out WeakReference? weak);
            if (existed && (!weak!.IsAlive || weak.Target == instance))
            {
                s_instances.Remove(handle);
#if DEBUG
                s_stackTraces.Remove(handle);
#endif
            }
        }
        finally
        {
            s_instancesLock.ExitWriteLock();
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
