using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class RotatedTranslatedShapeSettings : DecoratedShapeSettings
{
    public RotatedTranslatedShapeSettings(in Vector3 position, in Quaternion rotation, ShapeSettings shapeSettings)
        : base(JPH_RotatedTranslatedShapeSettings_Create(position, rotation, shapeSettings.Handle))
    {
    }

    public RotatedTranslatedShapeSettings(in Vector3 position, in Quaternion rotation, Shape shape)
        : base(JPH_RotatedTranslatedShapeSettings_Create2(position, rotation, shape.Handle))
    {
    }
}

public class RotatedTranslatedShape : DecoratedShape
{
    public RotatedTranslatedShape(in Vector3 position, in Quaternion rotation, Shape shape)
        : base(JPH_RotatedTranslatedShape_Create(position, rotation, shape.Handle))
    {}

    public RotatedTranslatedShape(RotatedTranslatedShapeSettings settings)
        : base(JPH_RotatedTranslatedShapeSettings_CreateShape(settings.Handle))
    {
    }
}