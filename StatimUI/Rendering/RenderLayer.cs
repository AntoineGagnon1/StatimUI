using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StatimUI.Rendering
{
    public class RenderLayer
    {
        public List<RenderCommand> Commands { get; set; } = new ();
        public RenderCommand LastCommand => Commands[Commands.Count - 1];
        private Stack<RectangleF> clipRects = new();


        public void PushClipRect(RectangleF rect)
        {
            clipRects.Push(rect);
            var command = RenderCommand.CreateDefault();
            command.ClipRect = rect;
            Commands.Add(command);
        }

        public void PopClipRect()
        {
            var clipRect = clipRects.Pop();
            var command = RenderCommand.CreateDefault();
            command.ClipRect = clipRect;
            Commands.Add(command);
        }

        private RenderCommand GetDrawCommand()
        {
            if (Commands.Count > 0)
            {
                var lastCommand = Commands[Commands.Count - 1];
                if (lastCommand.Texture.HasDefaultPixel)
                    return lastCommand;
            }

            var newCommand = RenderCommand.CreateDefault();
            Commands.Add(newCommand);
            return newCommand;
        }

        public void AddTriangle(Vertex a, Vertex b, Vertex c)
        {
            var cmd = GetDrawCommand();

            cmd.Indices.Add(cmd.VerticesCountU);
            cmd.Indices.Add(cmd.VerticesCountU + 1);
            cmd.Indices.Add(cmd.VerticesCountU + 2);

            cmd.AddVertex(a);
            cmd.AddVertex(b);
            cmd.AddVertex(c);
        }

        public void AddRectangle(Vertex a, Vertex b, Vertex c, Vertex d)
        {
            var cmd = GetDrawCommand();

            cmd.Indices.Add(cmd.VerticesCountU);
            cmd.Indices.Add(cmd.VerticesCountU + 1);
            cmd.Indices.Add(cmd.VerticesCountU + 2);

            cmd.Indices.Add(cmd.VerticesCountU + 2);
            cmd.Indices.Add(cmd.VerticesCountU + 3);
            cmd.Indices.Add(cmd.VerticesCountU);

            cmd.AddVertex(a);
            cmd.AddVertex(b);
            cmd.AddVertex(c);
            cmd.AddVertex(d);
        }
        public void AddRectangle(Vector2 topLeft, Vector2 bottomRight, Color strokeColor, float width, int rounding = 0)
        {
            var cmd = GetDrawCommand();

            if (rounding == 0)
            {
                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 1);
                cmd.Indices.Add(cmd.VerticesCountU + 4);

                cmd.Indices.Add(cmd.VerticesCountU + 1);
                cmd.Indices.Add(cmd.VerticesCountU + 4);
                cmd.Indices.Add(cmd.VerticesCountU + 5);


                cmd.Indices.Add(cmd.VerticesCountU + 1);
                cmd.Indices.Add(cmd.VerticesCountU + 3);
                cmd.Indices.Add(cmd.VerticesCountU + 5);

                cmd.Indices.Add(cmd.VerticesCountU + 3);
                cmd.Indices.Add(cmd.VerticesCountU + 5);
                cmd.Indices.Add(cmd.VerticesCountU + 7);


                cmd.Indices.Add(cmd.VerticesCountU + 3);
                cmd.Indices.Add(cmd.VerticesCountU + 2);
                cmd.Indices.Add(cmd.VerticesCountU + 7);

                cmd.Indices.Add(cmd.VerticesCountU + 2);
                cmd.Indices.Add(cmd.VerticesCountU + 7);
                cmd.Indices.Add(cmd.VerticesCountU + 6);


                cmd.Indices.Add(cmd.VerticesCountU + 2);
                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 6);

                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 6);
                cmd.Indices.Add(cmd.VerticesCountU + 4);

                var widthSize = new Vector2(width);
                cmd.AddVertex(new Vertex(topLeft - widthSize, strokeColor));
                cmd.AddVertex(new Vertex(new Vector2(bottomRight.X + width, topLeft.Y - width), strokeColor));
                cmd.AddVertex(new Vertex(new Vector2(topLeft.X - width, bottomRight.Y + width), strokeColor));
                cmd.AddVertex(new Vertex(bottomRight + widthSize, strokeColor));
                        
                cmd.AddVertex(new Vertex(topLeft, strokeColor));
                cmd.AddVertex(new Vertex(new Vector2(bottomRight.X, topLeft.Y), strokeColor));
                cmd.AddVertex(new Vertex(new Vector2(topLeft.X, bottomRight.Y), strokeColor));
                cmd.AddVertex(new Vertex(bottomRight, strokeColor));
            }

            // TODO: rounding
        }

        public void AddRectangleFilled(Vector2 topLeft, Vector2 bottomRight, Color fillColor, int rounding = 0)
        {
            var cmd = GetDrawCommand();

            /*if (!Transform.IsIdentity)
            {

                topLeft = Vector2.Transform(topLeft, Transform.Matrix);
                bottomRight = Vector2.Transform(bottomRight, Transform.Matrix);
            }*/

            if (rounding == 0)
            {
                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 1);
                cmd.Indices.Add(cmd.VerticesCountU + 3);

                cmd.Indices.Add(cmd.VerticesCountU + 3);
                cmd.Indices.Add(cmd.VerticesCountU + 2);
                cmd.Indices.Add(cmd.VerticesCountU);

                cmd.AddVertex(new Vertex(topLeft, fillColor));
                cmd.AddVertex(new Vertex(new Vector2(bottomRight.X, topLeft.Y), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(topLeft.X, bottomRight.Y), fillColor));
                cmd.AddVertex(new Vertex(bottomRight, fillColor));
            }
            else
            {
                // Split the rectangle in multiple triangle, all starting from the middle
                // 4 main triangles
                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 1);
                cmd.Indices.Add(cmd.VerticesCountU + 2);
                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 4);
                cmd.Indices.Add(cmd.VerticesCountU + 6);

                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 8);
                cmd.Indices.Add(cmd.VerticesCountU + 7);

                cmd.Indices.Add(cmd.VerticesCountU);
                cmd.Indices.Add(cmd.VerticesCountU + 5);
                cmd.Indices.Add(cmd.VerticesCountU + 3);
                int indexOff = cmd.VerticesCount;

                Vector2 center = Vector2.Lerp(topLeft, bottomRight, 0.5f);
                cmd.AddVertex(new Vertex(center, fillColor));
                cmd.AddVertex(new Vertex(new Vector2(topLeft.X + rounding, topLeft.Y), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(bottomRight.X - rounding, topLeft.Y), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(topLeft.X, topLeft.Y + rounding), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(bottomRight.X, topLeft.Y + rounding), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(topLeft.X, bottomRight.Y - rounding), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(bottomRight.X, bottomRight.Y - rounding), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(topLeft.X + rounding, bottomRight.Y), fillColor));
                cmd.AddVertex(new Vertex(new Vector2(bottomRight.X - rounding, bottomRight.Y), fillColor));

                // Corners
                int numDivisions = (int)Math.Ceiling(rounding / 2f);
                RoundTriangle(cmd, indexOff + 1, indexOff + 3, indexOff, Math.PI / 2, Math.PI, numDivisions, rounding, new Vector2(topLeft.X + rounding, topLeft.Y + rounding), fillColor);
                RoundTriangle(cmd, indexOff + 4, indexOff + 2, indexOff, 0, Math.PI / 2, numDivisions, rounding, new Vector2(bottomRight.X - rounding, topLeft.Y + rounding), fillColor);
                RoundTriangle(cmd, indexOff + 8, indexOff + 6, indexOff, (3 * Math.PI) / 2, 2 * Math.PI, numDivisions, rounding, new Vector2(bottomRight.X - rounding, bottomRight.Y - rounding), fillColor);
                RoundTriangle(cmd, indexOff + 5, indexOff + 7, indexOff, Math.PI, (3 * Math.PI) / 2, numDivisions, rounding, new Vector2(topLeft.X + rounding, bottomRight.Y - rounding), fillColor);
            }
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
        private void RoundTriangle(RenderCommand cmd, int indexStart, int indexEnd, int indexCenter, double angleStart, double angleEnd, int divisions, int radius, Vector2 roundCenter, Color color)
        {
            int currentStart = indexStart;
            double angle = angleStart;
            double angleDelta = (angleEnd - angleStart) / divisions;

            for (int i = 0; i < (divisions - 1); i++)
            {
                angle += angleDelta;

                Vector2 currentEnd = new Vector2((float)Math.Cos(angle) * radius, -(float)Math.Sin(angle) * radius) + roundCenter; // Invert the y because -y is up

                cmd.Indices.Add((uint)indexCenter);
                cmd.Indices.Add((uint)currentStart);
                cmd.Indices.Add(cmd.VerticesCountU);
                currentStart = cmd.VerticesCount;

                cmd.AddVertex(new Vertex(currentEnd, color));
            }

            //Last triangle that connects to indexEnd
            cmd.Indices.Add((uint)indexCenter);
            cmd.Indices.Add((uint)currentStart);
            cmd.Indices.Add((uint)indexEnd);
        }

        public Vector2 AddText(string text, Vector2 pos, Color color, Font font)
        {
            RenderCommand cmd;
            if (LastCommand.Texture.Id == font.Texture)
            {
                cmd = LastCommand;
            }
            else
            {
                cmd = new RenderCommand { Texture = new(font.Texture) };
                Commands.Add(cmd);
            }

            return font.MakeText(cmd, text, pos, color);
        }

        internal void Clear()
        {
            clipRects.Clear();
            Commands.Clear();
        }
    }
}
