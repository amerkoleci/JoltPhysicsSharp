// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp;

internal delegate object UserDataDelegate();

internal static partial class DelegateProxies
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Create<T>(object managedDel, T nativeDel, out GCHandle gch, out nint contextPtr)
    {
        if (managedDel == null)
        {
            gch = default;
            contextPtr = IntPtr.Zero;
            return default;
        }

        gch = GCHandle.Alloc(managedDel);
        contextPtr = GCHandle.ToIntPtr(gch);
        return nativeDel;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Create(object managedDel, out GCHandle gch, out nint contextPtr)
    {
        if (managedDel == null)
        {
            gch = default;
            contextPtr = 0;
            return;
        }

        gch = GCHandle.Alloc(managedDel);
        contextPtr = GCHandle.ToIntPtr(gch);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint CreateUserData(object userData, bool makeWeak = false)
    {
        userData = makeWeak ? new WeakReference(userData) : userData;
        var del = new UserDataDelegate(() => userData);
        Create(del, out _, out nint ctx);
        return ctx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? Get<T>(nint context, out GCHandle handle)
    {
        if (context == 0)
        {
            handle = default;
            return default;
        }

        handle = GCHandle.FromIntPtr(context);
        return (T)handle.Target!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetUserData<T>(nint context, out GCHandle handle)
    {
        UserDataDelegate del = Get<UserDataDelegate>(context, out handle)!;
        object value = del.Invoke();
        return value is WeakReference weak ? (T)weak.Target! : (T)value;
    }
}
