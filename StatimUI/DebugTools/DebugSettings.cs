using StatimUI.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.DebugTools
{
#if DEBUG
    public static class DebugSettings
    {
        public static bool ShowLayout { get; set; } // Shows the layout (padding, margin, ...) using colored sections
        public static bool ShowTextRect { get; set; } // Shows the bouding box of each letter

        internal static void RenderDebugInfo(Component c, Vector2 offset)
        {
            Renderer.PushLayer();
            RenderDebugInfoRecursive(c, offset);

            if (ShowTextRect)
                DebugTools.ShowTextRect.Render();

            Renderer.PopLayer();
        }

        private static void RenderDebugInfoRecursive(Component c, Vector2 offset)
        {
            if (ShowLayout)
                DebugTools.ShowLayout.Render(c, offset + c.DrawPosition);

            foreach (var child in c.Children)
            {
                RenderDebugInfoRecursive(child, offset + c.DrawPosition);
            }
        }
    }
#endif
}
