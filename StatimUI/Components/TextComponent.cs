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
        public Property<string> Content { get; set; }

        public override bool HasChanged()
        {
            return Content.HasChanged;
        }

        override public void Update()
        {
            ImGuiNET.ImGui.Text(Content);

            Content.HasChanged = false;
        }
    }
}
