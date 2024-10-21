// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// see: https://github.com/mono/SkiaSharp/blob/main/binding/SkiaSharp/PlatformLock.cs

using System.Runtime.InteropServices;

namespace JoltPhysicsSharp;

/// <summary>
/// Abstracts a platform dependant lock implementation
/// </summary>
internal interface IPlatformLock
{
    void EnterReadLock();
    void ExitReadLock();
    void EnterWriteLock();
    void ExitWriteLock();
    void EnterUpgradeableReadLock();
    void ExitUpgradeableReadLock();
}

/// <summary>
/// Helper class to create a IPlatformLock instance, by default according to the current platform
/// but also client toolkits can plugin their own implementation.
/// </summary>
internal static partial class PlatformLock
{
    /// <summary>
    /// Creates a platform lock
    /// </summary>
    /// <returns></returns>
    public static IPlatformLock Create()
    {
        // Just call the factory
        return Factory();
    }

    /// <summary>
    /// The factory for creating platform locks
    /// </summary>
    /// <remarks>
    /// Use this to plugin your own lock implementation.  Must be set
    /// before using other SkiaSharp functionality that causes the lock
    /// to be created (currently only used by SkiaSharps internal
    /// HandleDictionary).
    /// </remarks>
    public static Func<IPlatformLock> Factory { get; set; } = DefaultFactory;

    /// <summary>
    /// Default platform lock factory
    /// </summary>
    /// <returns>A reference to a new platform lock implementation</returns>
    public static IPlatformLock DefaultFactory()
    {
        if (OperatingSystem.IsWindows())
            return new NonAlertableWin32Lock();

        return new ReadWriteLock();
    }


    /// <summary>
    /// Non-Windows platform lock uses ReaderWriteLockSlim
    /// </summary>
    class ReadWriteLock : IPlatformLock
    {
        public void EnterReadLock() => _lock.EnterReadLock();
        public void ExitReadLock() => _lock.ExitReadLock();
        public void EnterWriteLock() => _lock.EnterWriteLock();
        public void ExitWriteLock() => _lock.ExitWriteLock();
        public void EnterUpgradeableReadLock() => _lock.EnterUpgradeableReadLock();
        public void ExitUpgradeableReadLock() => _lock.ExitUpgradeableReadLock();

        private readonly ReaderWriterLockSlim _lock = new();
    }

    /// <summary>
    /// Windows platform lock uses Win32 CRITICAL_SECTION
    /// </summary>
    partial class NonAlertableWin32Lock : IPlatformLock
    {
        public unsafe NonAlertableWin32Lock()
        {
            _cs = Marshal.AllocHGlobal(sizeof(CRITICAL_SECTION));
            if (_cs == IntPtr.Zero)
                throw new OutOfMemoryException("Failed to allocate memory for critical section");

            InitializeCriticalSectionEx(_cs, 4000, 0);
        }

        ~NonAlertableWin32Lock()
        {
            if (_cs != IntPtr.Zero)
            {
                DeleteCriticalSection(_cs);
                Marshal.FreeHGlobal(_cs);
                _cs = IntPtr.Zero;
            }
        }

        IntPtr _cs;

        void Enter()
        {
            if (_cs != IntPtr.Zero)
            {
                EnterCriticalSection(_cs);
            }
        }

        void Leave()
        {
            if (_cs != IntPtr.Zero)
            {
                LeaveCriticalSection(_cs);
            }
        }

        public void EnterReadLock() { Enter(); }
        public void ExitReadLock() { Leave(); }
        public void EnterWriteLock() { Enter(); }
        public void ExitWriteLock() { Leave(); }
        public void EnterUpgradeableReadLock() { Enter(); }
        public void ExitUpgradeableReadLock() { Leave(); }

        [StructLayout(LayoutKind.Sequential)]
        public struct CRITICAL_SECTION
        {
            public IntPtr DebugInfo;
            public int LockCount;
            public int RecursionCount;
            public IntPtr OwningThread;
            public IntPtr LockSemaphore;
            public UIntPtr SpinCount;
        }

        [LibraryImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool InitializeCriticalSectionEx(IntPtr lpCriticalSection, uint dwSpinCount, uint Flags);
        [LibraryImport("Kernel32.dll")]
        private static partial void DeleteCriticalSection(IntPtr lpCriticalSection);
        [LibraryImport("Kernel32.dll")]
        private static partial void EnterCriticalSection(IntPtr lpCriticalSection);
        [LibraryImport("Kernel32.dll")]
        private static partial void LeaveCriticalSection(IntPtr lpCriticalSection);
    }
}
