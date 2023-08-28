using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace StatimUI.Rendering
{
    // TODO : make this a struct ?
    public class RenderCommand
    {
        // TODO: maybe this is a Rectangle(not F)? I don't know if other rendering apis support float scissor
        public RectangleF ClipRect { get; set; }
        private List<Vertex> vertices { get; } = new();
        public List<uint> Indices { get; } = new();

        public int VerticesCount => vertices.Count;
        public uint VerticesCountU => (uint)vertices.Count;

        public Texture Texture { get; set; }
        // TODO: this is temporary, might want to find a better solution or make a Texture struct

        public Matrix4x4 Transform = Matrix4x4.Identity;

        internal void Clear()
        {
            vertices.Clear();
            Indices.Clear();
        }

        public Vertex[] VerticesToArray() => vertices.ToArray();



        public void AddVertex(Vertex vertex)
        {
            if (!StatimUI.TransformManager.IsEmpty)
                vertex.Position = Vector2.Transform(vertex.Position, StatimUI.TransformManager.Matrix);
            vertices.Add(vertex);
        }

        public static RenderCommand CreateDefault() => new RenderCommand { Texture =  new Texture(FontManager.DefaultFont.Texture, true) };
    }

    public struct Texture
    {
        public IntPtr Id { get; set; }
        public bool HasDefaultPixel { get; set; }

        public Texture(IntPtr id, bool hasDefaultPixel = false)
        {
            Id = id;
            HasDefaultPixel = hasDefaultPixel;
        }
    }
}
