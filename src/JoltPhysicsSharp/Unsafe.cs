// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
#if !NET
namespace JoltPhysicsSharp;
static class Unsafe
{
    public static unsafe ref T AsRef<T>(void* source) where T : unmanaged => ref *(T*)source;
}
#endif
