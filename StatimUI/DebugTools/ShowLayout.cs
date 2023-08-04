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
            ImGuiNET.ImGui.GetForegroundDrawList().AddRect(start, start + new Vector2(c.TotalPixelWidth, c.TotalPixelHeight), 0xFF0000FF);
            ImGuiNET.ImGui.GetForegroundDrawList().AddRect(start + c.TopLeftPadding, start + new Vector2(c.TotalPixelWidth, c.TotalPixelHeight) - c.BottomRightPadding, 0xFFFF00FF);
        }
    }
#endif
}
