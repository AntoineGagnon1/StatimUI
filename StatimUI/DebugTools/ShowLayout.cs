using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace StatimUI.Debug
{
#if DEBUG
    public static class ShowLayout
    {
        public static void Render(Component c, Vector2 offset)
        {
            if (!c.Visible)
                return;

            uint color = (uint)c.GetHashCode() & 0x00FFFFFF; // only keep the lower 6 bytes (r, g, b)

            Vector2 marginTopLeft = offset + c.Position.Value;
            Vector2 marginTopRight = marginTopLeft + new Vector2(c.TotalPixelWidth, 0);
            Vector2 marginBottomLeft = marginTopLeft + new Vector2(0, c.TotalPixelHeight);
            Vector2 marginBottomRight = marginTopLeft + new Vector2(c.TotalPixelWidth, c.TotalPixelHeight);
            
            Vector2 topLeft = marginTopLeft + c.Margin.Value.TopLeft;
            Vector2 topRight = marginTopRight + new Vector2(-c.Margin.Value.Right, c.Margin.Value.Top);
            Vector2 bottomLeft = marginBottomLeft + new Vector2(c.Margin.Value.Left, -c.Margin.Value.Bottom); ;
            Vector2 bottomRight = marginBottomRight - c.Margin.Value.BottomRight;

            Vector2 paddingTopLeft = topLeft + c.Padding.Value.TopLeft;
            Vector2 paddingTopRight = topRight + new Vector2(-c.Padding.Value.Right, c.Padding.Value.Top);
            Vector2 paddingBottomLeft = bottomLeft + new Vector2(c.Padding.Value.Left, -c.Padding.Value.Bottom); ;
            Vector2 paddingBottomRight = bottomRight - c.Padding.Value.BottomRight;

            // Margins
            uint marginColor = color | 0xFF000000;
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(marginTopLeft, bottomLeft, marginColor);
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(topLeft, marginTopRight, marginColor);
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(topRight, marginBottomRight, marginColor);
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(marginBottomLeft, bottomRight, marginColor);

            // Padding
            uint paddingColor = color | 0x50000000;
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(topLeft, paddingBottomLeft, paddingColor);
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(paddingTopLeft, topRight, paddingColor);
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(paddingTopRight, bottomRight, paddingColor);
            ImGuiNET.ImGui.GetForegroundDrawList().AddRectFilled(bottomLeft, paddingBottomRight, paddingColor);
        }
    }
#endif
}
