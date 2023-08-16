using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace StatimUI.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector2 Position;
        public Vector2 UV;
        public Color Color;

        public Vertex(Vector2 pos, Color color)
        {
            Position = pos;
            UV = Vector2.Zero;
            Color = color;
        }

        public Vertex(Vector2 pos, Vector2 uv, Color color)
        {
            Position = pos;
            UV = uv;
            Color = color;
        }
    }
}
