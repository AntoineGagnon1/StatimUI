using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("textInput")]
    public class TextInputComponent : Component
    {
        public Property<string> Content;

        override public void Update()
        {
            string temp = Content;
            if (ImGuiNET.ImGui.InputText($"##{this.GetHashCode()}", ref temp, 100))
                Content.Value = temp;
        }
    }
}
