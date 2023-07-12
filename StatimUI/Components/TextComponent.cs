using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("text")]
    public class TextComponent : Component
    {
        public Property<int> Content { get; set; }

        override public void Update()
        {
            ImGuiNET.ImGui.Text(Content.Value.ToString());
        }
    }
}
