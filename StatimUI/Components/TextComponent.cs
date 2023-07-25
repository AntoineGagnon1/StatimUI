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

        public override void Start(IList<Component> slots)
        {
            Console.WriteLine(Parent != null);
        }

        override public void Update()
        {
            //if ((new Random()).NextDouble() > 0.995d)
            //    Content.Value += "a";
            ImGuiNET.ImGui.Text(Content.Value);
        }
    }
}
