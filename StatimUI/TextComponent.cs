using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    public class TextComponent : Component
    {
        public string Content { get; set; } = "Hello";

        override public void Render()
        {
            ImGuiNET.ImGui.Text(Content);
        }
    }
}
