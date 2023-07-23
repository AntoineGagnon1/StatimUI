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
        public Vertical(List<Component> slots)
        {
            Children = slots;
        }

        public override void Update()
        {
            foreach (var child in Children)
            {
                child.Update();
                ImGui.NewLine();
            }
        }
    }
}
