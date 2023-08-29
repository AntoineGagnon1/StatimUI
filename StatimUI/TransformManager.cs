using System.Collections.Generic;
using System.Numerics;

namespace StatimUI
{
    public struct Transform
    {
        public Vector2 Origin;
        public Angle Rotation;
        public Vector2 Scale;

        public Transform(Vector2 origin, Angle rotation, Vector2 scale)
        {
            Origin = origin;
            Rotation = rotation;
            Scale = scale;
        }

        public static Transform FromComponent(Vector2 componentPos, Component component) => new Transform(componentPos + component.Origin.Value, component.Rotation.Value, component.Scale.Value);
    }

    public static class TransformManager
    {
        public static float Rotation = 0f;
        public static Vector2 Scale = Vector2.One;

        private static Stack<Transform> transforms = new Stack<Transform>();

        private static Matrix3x2 _matrix = Matrix3x2.Identity;
        public static Matrix3x2 Matrix => _matrix;
        public static bool IsEmpty = true;

        private static void SetTotalMatrix()
        {
            Matrix3x2 scaleMatrix = Matrix3x2.Identity;
            foreach (Transform transform in transforms)
                scaleMatrix *= Matrix3x2.CreateScale(transform.Scale, transform.Origin);

            Matrix3x2 totalMatrix = scaleMatrix;
            foreach (Transform transform in transforms)
                totalMatrix *= Matrix3x2.CreateRotation(transform.Rotation.Radians, Vector2.Transform(transform.Origin, scaleMatrix));

            _matrix = totalMatrix;
            IsEmpty = _matrix.IsIdentity;
        }

        public static void PushTransform(Transform transform)
        {
            transforms.Push(transform);
            SetTotalMatrix();
        }

        public static void PopTransform()
        {
            transforms.Pop();
            SetTotalMatrix();
        }
    }
}
