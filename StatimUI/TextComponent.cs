using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI
{
    [Component("text")]
    public class TextComponent : Component
    {
        public Property<string> Content;

        public override bool HasChanged()
        {
            return Content.HasChanged;
        }

        override public void Update()
        {
            string temp = Content;
            if (ImGuiNET.ImGui.InputText("Hello", ref temp, 100))
                Content.Value = temp;

            Content.HasChanged = false;
        }
    }
}
