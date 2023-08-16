using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StatimUI.Rendering
{
    // TODO : make this a struct ?
    public class RenderCommand
    {
        public List<Vertex> Vertices { get; } = new();
        public List<uint> Indices { get; } = new();

        public IntPtr Texture { get; set; }

        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;

        public void AddTriangle(Vertex a, Vertex b, Vertex c)
        {
            Indices.Add((uint)Vertices.Count);
            Indices.Add((uint)Vertices.Count + 1);
            Indices.Add((uint)Vertices.Count + 2);

            Vertices.Add(a);
            Vertices.Add(b);
            Vertices.Add(c);
        }

        public void AddRectangle(Vertex topLeft, Vertex topRight, Vertex bottomLeft, Vertex bottomRight)
        {
            Indices.Add((uint)Vertices.Count);
            Indices.Add((uint)Vertices.Count + 1);
            Indices.Add((uint)Vertices.Count + 3);

            Indices.Add((uint)Vertices.Count + 3);
            Indices.Add((uint)Vertices.Count + 2);
            Indices.Add((uint)Vertices.Count);

            Vertices.Add(topLeft);
            Vertices.Add(topRight);
            Vertices.Add(bottomLeft);
            Vertices.Add(bottomRight);
        }

        public void AddFilledRectangle(Vector2 topLeft, Vector2 bottomRight, Color color)
        {
            AddRectangle(
                new Vertex(topLeft, color), 
                new Vertex(new Vector2(bottomRight.X, topLeft.Y), color),
                new Vertex(new Vector2(topLeft.X, bottomRight.Y), color),
                new Vertex(bottomRight, color)
                );
        }

        internal void Clear()
        {
            Vertices.Clear();
            Indices.Clear();
        }
    }
}
