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

            Vector2 start = offset + c.Position.Value;
            // Size + margins
            ImGuiNET.ImGui.GetForegroundDrawList().AddRect(start, start + new Vector2(c.TotalPixelWidth, c.TotalPixelHeight), 0xFF00FFFF);
            // Size
            start += c.Margin.Value.TopLeft;
            ImGuiNET.ImGui.GetForegroundDrawList().AddRect(start, start + new Vector2(c.PixelWidth, c.PixelHeight), 0xFF0000FF);
            // Content size (size - padding)
            ImGuiNET.ImGui.GetForegroundDrawList().AddRect(start + c.Padding.Value.TopLeft, start + new Vector2(c.PixelWidth, c.PixelHeight) - c.Padding.Value.BottomRight, 0xFFFF00FF);
        }
    }
#endif
}
