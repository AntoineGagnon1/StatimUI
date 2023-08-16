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


        internal void Clear()
        {
            Vertices.Clear();
            Indices.Clear();
        }
    }
}
