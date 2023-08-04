﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public class Window
    {
        public Component Root { get; set; }

        public void Update()
        {
            Root.Update();

            Root.Render(Vector2.Zero);
#if DEBUG
            RenderDebug(Root, ImGuiNET.ImGui.GetWindowPos());
#endif
        }

#if DEBUG
        private void RenderDebug(Component c, Vector2 offset)
        {
            Debug.DebugSettings.RenderDebugInfo(c, offset);

            foreach(var child in c.Children)
            {
                RenderDebug(child, offset + c.Padding.Value.TopLeft);
            }
        }
#endif
    }
}