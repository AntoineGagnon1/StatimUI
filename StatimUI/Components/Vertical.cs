using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("vertical")]
    public class Vertical : Component
    {
        public Property<Component[]> Content;

        public override void Update()
        {
            foreach (var component in Content.Value)
            {
                component.Update();
                ImGui.NewLine();
            }
        }
    }
}
