// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// see: https://github.com/mono/SkiaSharp/blob/main/binding/SkiaSharp/HandleDictionary.cs

#if DEBUG
using System.Collections.Concurrent;
#endif

namespace JoltPhysicsSharp;

internal static class HandleDictionary
{
    private static readonly IPlatformLock s_instancesLock = PlatformLock.Create();
    private static readonly Dictionary<IntPtr, WeakReference> s_instances = [];
    private static readonly Dictionary<Type, Func<nint, NativeObject>> s_registeredFactories = [];

    public static void RegisterFactory<TNativeObject>(Func<nint, TNativeObject> factory)
        where TNativeObject : NativeObject
    {
        s_registeredFactories.Add(typeof(TNativeObject), factory);
    }

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
    public static TNativeObject? GetOrAddObject<TNativeObject>(nint handle)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
            return default;

        if (!s_registeredFactories.TryGetValue(typeof(TNativeObject), out Func<nint, NativeObject>? objectFactory))
        {
            throw new InvalidOperationException($"No Factory registered for object type: {typeof(TNativeObject)}");
        }

        s_instancesLock.EnterUpgradeableReadLock();
        try
        {
            if (GetInstanceNoLocks(handle, out TNativeObject? instance))
            {
                return instance;
            }

            TNativeObject obj = (TNativeObject)objectFactory.Invoke(handle);
            return obj;
        }
        finally
        {
            s_instancesLock.ExitUpgradeableReadLock();
        }
    }

    /// <summary>
    /// Retrieve or create an instance for the native handle.
    /// </summary>
    /// <returns>The instance, or null if the handle was null.</returns>
    public static TNativeObject? GetOrAddObject<TNativeObject>(nint handle, Func<nint, TNativeObject> objectFactory)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
            return default;

        s_instancesLock.EnterUpgradeableReadLock();
        try
        {
            if (GetInstanceNoLocks(handle, out TNativeObject? instance))
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
#if DEBUG
                if (obj.OwnsHandle)
                {
                    // a mostly recoverable error
                    // if there is a managed object, then maybe something happened and the native object is dead
                    throw new InvalidOperationException(
                        $"A managed object already exists for the specified native object. " +
                        $"H: {handle.ToString("x")} Type: ({obj.GetType()}, {instance.GetType()})");
                }
#endif
                // this means the ownership was handed off to a native object, so clean up the managed side
                objectToDispose = obj;
            }

            s_instances[handle] = new WeakReference(instance);
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
            }
            else
            {
#if DEBUG
                InvalidOperationException? ex = null;
                if (!existed)
                {
                    // the object may have been replaced

                    if (!instance.IsDisposed)
                    {
                        // recoverable error
                        // there was no object there, but we are still alive
                        ex = new InvalidOperationException(
                            $"A managed object did not exist for the specified native object. " +
                            $"H: {handle.ToString("x")} Type: {instance.GetType()}");
                    }
                }
                else if (weak.Target! is NativeObject o && o != instance)
                {
                    // there was an object in the dictionary, but it was NOT this object

                    if (!instance.IsDisposed)
                    {
                        // recoverable error
                        // there was a new living object there, but we are still alive
                        ex = new InvalidOperationException(
                            $"Trying to remove a different object with the same native handle. " +
                            $"H: {handle.ToString("x")} Type: ({o.GetType()}, {instance.GetType()})");
                    }
                }
                if (ex != null)
                {
                    if (!instance.fromFinalizer)
                        throw ex;
                }
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
#if DEBUG
                }
                else if (weak.Target is NativeObject obj)
                {
                    if (!obj.IsDisposed && obj.OwnsHandle)
                    {
                        throw new InvalidOperationException(
                            $"A managed object exists for the handle, but is not the expected type. " +
                            $"H: {handle.ToString("x")} Type: ({obj.GetType()}, {typeof(TNativeObject)})");
                    }
                }
                else if (weak.Target is object o)
                {
                    throw new InvalidOperationException(
                        $"An unknown object exists for the handle when trying to fetch an instance. " +
                        $"H: {handle:x} Type: ({o.GetType()}, {typeof(TNativeObject)})");
#endif
                }
            }
        }

        instance = null;
        return false;
    }
}
