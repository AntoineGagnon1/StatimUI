using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StatimUI.Components
{
    [Component("text")]
    public class TextComponent : Component
    {
        public Property<string> Content;

        public override void Render(Vector2 offset)
        {
            if (Visible)
            {
                ImGuiNET.ImGui.SetCursorPos(offset + Position.Value);
                ImGuiNET.ImGui.Text(Content.Value);
            }
        }

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

            if (CanSetWidth)
                Width = ImGuiNET.ImGui.CalcTextSize(Content.Value).X;

            return HasSizeChanged();
        }
    }
}
