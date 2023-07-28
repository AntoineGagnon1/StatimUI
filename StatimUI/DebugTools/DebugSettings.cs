using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Debug
{
#if DEBUG
    public static class DebugSettings
    {
        public static bool ShowLayout { get; set; } // Shows the layout (padding, margin, ...) using colored sections

        internal static void RenderDebugInfo(Component c, Vector2 offset)
        {
            if (ShowLayout)
                Debug.ShowLayout.Render(c, offset);
        }
    }
#endif
}
