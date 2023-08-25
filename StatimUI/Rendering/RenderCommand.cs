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
        // TODO: this is temporary, might want to find a better solution or make a Texture struct
        public bool TextureHasDefaultPixel { get; set; }

        public Matrix4x4 Transform = Matrix4x4.Identity;

        internal void Clear()
        {
            Vertices.Clear();
            Indices.Clear();
        }
    }
}
