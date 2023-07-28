using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            Width.Value = 50.0f;
            Height.Value = 14.0f;
        }

        override public bool Update()
        {
            if ((new Random()).NextDouble() > 0.9995d && CanSetHeight)
                Height.Value += 2f;
            //    Content.Value += "a";
            //Console.WriteLine(new System.Numerics.Vector2(Position.Value.X, Position.Value.Y).ToString());
            ImGuiNET.ImGui.SetCursorPos(new System.Numerics.Vector2(Position.Value.X, Position.Value.Y) + new System.Numerics.Vector2(50, 50));
            ImGuiNET.ImGui.Text(Content.Value);

            return HasSizeChanged();
        }
    }
}
