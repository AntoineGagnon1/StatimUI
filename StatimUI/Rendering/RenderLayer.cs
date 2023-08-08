using System;
using System.Collections.Generic;
using System.Text;

namespace StatimUI.Rendering
{
    public class RenderLayer
    {
        public List<Vertex> Vertices { get; } = new();
        public List<uint> Indices { get; } = new();

        
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
