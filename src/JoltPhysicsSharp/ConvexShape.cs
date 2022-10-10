// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ConvexShape : Shape
{
    internal ConvexShape(IntPtr handle)
        : base(handle)
    {
    }
}
