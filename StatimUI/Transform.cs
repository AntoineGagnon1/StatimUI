using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StatimUI
{
    public static class Transform
    {
        private static Stack<float> Rotations = new Stack<float>();
        private static Stack<Vector2> Scales = new Stack<Vector2>();
        private static Matrix3x2 _matrix = Matrix3x2.Identity;
        public static Matrix3x2 Matrix => _matrix;
        public static bool IsIdentity = true;

        public static void UpdateMatrix(float rotation, Vector2 scale)
        {
            _matrix = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateRotation(rotation);
            IsIdentity = _matrix.IsIdentity;
        }

        public static void PushRotation(float rotation)
        {
            Rotations.Push(rotation);
            UpdateMatrix(rotation, Scales.PeekOrDefault());
        }

        public static void PopRotation()
        {
            UpdateMatrix(Rotations.Pop(), Scales.PeekOrDefault());
        }

        public static void PushScale(Vector2 scale)
        {
            Scales.Push(scale);
            UpdateMatrix(Rotations.PeekOrDefault(), scale);
        }

        public static void PopScale()
        {
            UpdateMatrix(Rotations.PeekOrDefault(), Scales.Pop());
        }

        private static Vector2 PeekOrDefault(this Stack<Vector2> stack)
        {
            if (stack.Count > 0)
                return stack.Peek();

            return Vector2.One;
        }

        private static float PeekOrDefault(this Stack<float> stack)
        {
            if (stack.Count > 0)
                return stack.Peek();

            return 0f;
        }
    }
}
