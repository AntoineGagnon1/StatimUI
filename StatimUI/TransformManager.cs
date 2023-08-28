using System.Collections.Generic;
using System.Numerics;

namespace StatimUI
{
    public struct Transform
    {
        public Vector2 Origin;
        public float Rotation;
        public Vector2 Scale;

        public Transform(Vector2 origin, float rotation, Vector2 scale)
        {
            Origin = origin;
            Rotation = rotation;
            Scale = scale;
        }

        public static Transform FromComponent(Component component) => new Transform(component.Origin.Value, component.Rotation.Value, component.Scale.Value);
    }

    public static class TransformManager
    {
        public static float Rotation = 0f;
        public static Vector2 Scale = Vector2.One;

        private static Stack<Transform> transforms = new Stack<Transform>();

        private static Matrix3x2 _matrix = Matrix3x2.Identity;
        public static Matrix3x2 Matrix => _matrix;
        public static bool IsEmpty = true;

        private static void ApplyTransform(Vector2 origin)
        {
            _matrix = Matrix3x2.CreateTranslation(Vector2.One - origin) * Matrix3x2.CreateScale(Scale) *
                      Matrix3x2.CreateRotation(Rotation) * Matrix3x2.CreateTranslation(origin);
            IsEmpty = _matrix.IsIdentity;
        }

        public static void PushTransform(Transform transform)
        {
            transforms.Push(transform);
            Scale += transform.Scale - Vector2.One;
            Rotation += transform.Rotation;
            ApplyTransform(transform.Origin);
        }

        public static void PopTransform()
        {
            var transform = transforms.Pop();

            Scale -= transform.Scale - Vector2.One;
            Rotation -= transform.Rotation;

            ApplyTransform(transform.Origin);
        }
    }
}
