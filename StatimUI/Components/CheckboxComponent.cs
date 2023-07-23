using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("checkbox")]
    public class CheckboxComponent : Component
    {
        public Property<bool> Content = new ValueProperty<bool>(false);

        public CheckboxComponent(List<Component> slots)
        { }

        override public void Update()
        {
            bool temp = Content;
            if (ImGuiNET.ImGui.Checkbox($"##{this.GetHashCode()}", ref temp))
                Content.Value = temp;
        }
    }
}
