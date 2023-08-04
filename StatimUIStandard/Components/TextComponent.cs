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
        public Property<string> Content = new ValueProperty<string>("");

        public override void Render(Vector2 offset)
        {
            if (Visible)
            {
                ImGuiNET.ImGui.SetCursorPos(offset + DrawPosition);
                ImGuiNET.ImGui.Text(Content.Value);
            }
        }

        public override void Start(IList<Component> slots)
        {
            Console.WriteLine(Parent != null);
            MinWidth.Value = new Dimension(10.0f, DimensionUnit.Pixel);
            MinHeight.Value = new Dimension(14.0f, DimensionUnit.Pixel);
            Margin.Value = new Thickness(10, 0, 10, 10);
        }
        
        override public bool Update()
        {
            if ((new Random()).NextDouble() > 0.9995d)
                Height.Value.Scalar += 2f;


            Width.Value.Scalar = ImGuiNET.ImGui.CalcTextSize(Content.Value).X;

            /*if (CanSetWidth)
                Width.Value = ImGuiNET.ImGui.CalcTextSize(Content.Value).X;*/

            return HasSizeChanged();
        }
    }
}
