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
        public Property<string> Content;

        override public void Update()
        {
            if ((new Random()).NextDouble() > 0.995d)
                Content.Value += "a";
            ImGuiNET.ImGui.Text(Content.Value.ToString());
        }
    }
}
