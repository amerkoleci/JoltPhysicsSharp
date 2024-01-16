namespace JoltPhysicsSharp;

public abstract class DecoratedShapeSettings : ShapeSettings
{
    protected DecoratedShapeSettings()
    {
    }

    internal DecoratedShapeSettings(nint handle)
        : base(handle)
    {
    }
}

public abstract class DecoratedShape : Shape
{
    internal DecoratedShape(nint handle)
        : base(handle)
    {
    }
}