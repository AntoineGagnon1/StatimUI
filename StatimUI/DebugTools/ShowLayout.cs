using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;
using StatimUI.Rendering;
using Color = StatimUI.Rendering.Color;

namespace StatimUI.DebugTools
{
#if DEBUG
    public static class ShowLayout
    {
        public static void Render(Component c, Vector2 offset)
        {
            if (!c.Visible)
                return;

            uint color = (uint)c.GetHashCode() & 0x00FFFFFF; // only keep the lower 6 bytes (r, g, b)

            Vector2 marginTopLeft = (offset - c.Margin.Value.TopLeft) - c.Padding.Value.TopLeft;
            Vector2 marginTopRight = marginTopLeft + new Vector2(c.TotalPixelWidth, 0);
            Vector2 marginBottomLeft = marginTopLeft + new Vector2(0, c.TotalPixelHeight);
            Vector2 marginBottomRight = marginTopLeft + new Vector2(c.TotalPixelWidth, c.TotalPixelHeight);
            
            Vector2 topLeft = offset - c.Padding.Value.TopLeft;
            Vector2 topRight = marginTopRight + new Vector2(-c.Margin.Value.Right, c.Margin.Value.Top);
            Vector2 bottomLeft = marginBottomLeft + new Vector2(c.Margin.Value.Left, -c.Margin.Value.Bottom);
            Vector2 bottomRight = marginBottomRight - c.Margin.Value.BottomRight;

            Vector2 paddingTopLeft = offset;
            Vector2 paddingTopRight = topRight + new Vector2(-c.Padding.Value.Right, c.Padding.Value.Top);
            Vector2 paddingBottomLeft = bottomLeft + new Vector2(c.Padding.Value.Left, -c.Padding.Value.Bottom);
            Vector2 paddingBottomRight = bottomRight - c.Padding.Value.BottomRight;

            var cmd = new RenderCommand();

            // Margins
            Color marginColor = Color.FromHex(color, 255);
            cmd.AddFilledRectangle(marginTopLeft, bottomLeft, marginColor);
            cmd.AddFilledRectangle(topLeft, marginTopRight, marginColor);
            cmd.AddFilledRectangle(topRight, marginBottomRight, marginColor);
            cmd.AddFilledRectangle(marginBottomLeft, bottomRight, marginColor);

            // Padding
            Color paddingColor = Color.FromHex(color, 120);
            cmd.AddFilledRectangle(topLeft, paddingBottomLeft, paddingColor);
            cmd.AddFilledRectangle(paddingTopLeft, topRight, paddingColor);
            cmd.AddFilledRectangle(paddingTopRight, bottomRight, paddingColor);
            cmd.AddFilledRectangle(bottomLeft, paddingBottomRight, paddingColor);

            Renderer.CurrentLayer.Commands.Add(cmd);
        }
    }
#endif
}
