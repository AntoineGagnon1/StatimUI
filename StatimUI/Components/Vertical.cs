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
        public override void Start(IList<Component> slots)
        {
            Console.WriteLine(Parent != null);
            Children.AddRange(slots);
        }

        public override void Update()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Update();
            }
        }
    }
}
