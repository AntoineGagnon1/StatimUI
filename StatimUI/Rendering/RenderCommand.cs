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

        public void AddRectangle(Vector2 topLeft, Vector2 bottomRight, Color fillColor, int rounding = 0)
        {
            if (rounding == 0)
            {
                Indices.Add((uint)Vertices.Count);
                Indices.Add((uint)Vertices.Count + 1);
                Indices.Add((uint)Vertices.Count + 3);

                Indices.Add((uint)Vertices.Count + 3);
                Indices.Add((uint)Vertices.Count + 2);
                Indices.Add((uint)Vertices.Count);

                Vertices.Add(new Vertex(topLeft, fillColor));
                Vertices.Add(new Vertex(new Vector2(bottomRight.X, topLeft.Y), fillColor));
                Vertices.Add(new Vertex(new Vector2(topLeft.X, bottomRight.Y), fillColor));
                Vertices.Add(new Vertex(bottomRight, fillColor));
            }
            else
            {
                // Split the rectangle in multiple triangle, all starting from the middle
                // 4 main triangles
                Indices.Add((uint)Vertices.Count);
                Indices.Add((uint)Vertices.Count + 1);
                Indices.Add((uint)Vertices.Count + 2);

                Indices.Add((uint)Vertices.Count);
                Indices.Add((uint)Vertices.Count + 4);
                Indices.Add((uint)Vertices.Count + 6);

                Indices.Add((uint)Vertices.Count);
                Indices.Add((uint)Vertices.Count + 8);
                Indices.Add((uint)Vertices.Count + 7);

                Indices.Add((uint)Vertices.Count);
                Indices.Add((uint)Vertices.Count + 5);
                Indices.Add((uint)Vertices.Count + 3);
                int indexOff = Vertices.Count;

                Vector2 center = Vector2.Lerp(topLeft, bottomRight, 0.5f);
                Vertices.Add(new Vertex(center, fillColor));
                Vertices.Add(new Vertex(new Vector2(topLeft.X + rounding, topLeft.Y), fillColor));
                Vertices.Add(new Vertex(new Vector2(bottomRight.X - rounding, topLeft.Y), fillColor));
                Vertices.Add(new Vertex(new Vector2(topLeft.X, topLeft.Y + rounding), fillColor));
                Vertices.Add(new Vertex(new Vector2(bottomRight.X, topLeft.Y + rounding), fillColor));
                Vertices.Add(new Vertex(new Vector2(topLeft.X, bottomRight.Y - rounding), fillColor));
                Vertices.Add(new Vertex(new Vector2(bottomRight.X, bottomRight.Y - rounding), fillColor));
                Vertices.Add(new Vertex(new Vector2(topLeft.X + rounding, bottomRight.Y), fillColor));
                Vertices.Add(new Vertex(new Vector2(bottomRight.X - rounding, bottomRight.Y), fillColor));

                // Corners
                int numDivisions = (int)Math.Ceiling(rounding / 2f);
                RoundTriangle(indexOff + 1, indexOff + 3, indexOff, Math.PI / 2, Math.PI, numDivisions, rounding, new Vector2(topLeft.X + rounding, topLeft.Y + rounding), fillColor);
                RoundTriangle(indexOff + 4, indexOff + 2, indexOff, 0, Math.PI / 2, numDivisions, rounding, new Vector2(bottomRight.X - rounding, topLeft.Y + rounding), fillColor);
                RoundTriangle(indexOff + 8, indexOff + 6, indexOff, (3 * Math.PI) / 2, 2 * Math.PI, numDivisions, rounding, new Vector2(bottomRight.X - rounding, bottomRight.Y - rounding), fillColor);
                RoundTriangle(indexOff + 5, indexOff + 7, indexOff, Math.PI, (3 * Math.PI) / 2, numDivisions, rounding, new Vector2(topLeft.X + rounding, bottomRight.Y - rounding), fillColor);
            }
        }

        internal void Clear()
        {
            Vertices.Clear();
            Indices.Clear();
        }


        // Will create [divisions] triangles to smoothly round the end of the triangle made of the vertices indexStart, indexEnd, indexCenter
        // indexStart : the index of the start vertex
        // indexEnd : the index of the end vertex
        // indexCenter : the index of the center vertex
        // angleStart : the start angle
        // angleEnd : the end angle
        // divisions : the number of triangles to make, MUST be >= 1
        // radius : the radius of the rounding
        // roundCenter : the position of the center of the rounding, must be in +y is up +x is right space (does not have to be the same as the center index)
        // color : the color of the new vertices that will be created
        private void RoundTriangle(int indexStart, int indexEnd, int indexCenter, double angleStart, double angleEnd, int divisions, int radius, Vector2 roundCenter, Color color)
        {
            int currentStart = indexStart;
            double angle = angleStart;
            double angleDelta = (angleEnd - angleStart) / divisions;

            for (int i = 0; i < (divisions - 1); i++)
            {
                angle += angleDelta;

                Vector2 currentEnd = new Vector2((float)Math.Cos(angle) * radius, -(float)Math.Sin(angle) * radius) + roundCenter; // Invert the y because -y is up

                Indices.Add((uint)indexCenter);
                Indices.Add((uint)currentStart);
                Indices.Add((uint)Vertices.Count);
                currentStart = Vertices.Count;

                Vertices.Add(new Vertex(currentEnd, color));
            }

            //Last triangle that connects to indexEnd
            Indices.Add((uint)indexCenter);
            Indices.Add((uint)currentStart);
            Indices.Add((uint)indexEnd);
        }
    }
}
