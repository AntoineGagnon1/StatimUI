using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("textInput")]
    public class TextInputComponent : Component
    {
        public Property<string> Content = new ValueProperty<string>(string.Empty);

        public override void Render(Vector2 offset)
        {
            string temp = Content;
            if (ImGuiNET.ImGui.InputText($"##{this.GetHashCode()}", ref temp, 100))
                Content.Value = temp;
        }

        public override void Start(IList<Component> slots)
        {
        }

        override public bool Update()
        {
            return false;
        }
    }
}
