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

        protected override void OnRender(Vector2 drawPosition)
        {
            ImGuiNET.ImGui.SetCursorPos(drawPosition);
            ImGuiNET.ImGui.Text(Content.Value);
        }

        public override void Start(IList<Component> slots)
        {
            MinWidth.Value = new Dimension(10.0f + Padding.Value.Horizontal, DimensionUnit.Pixel);
            MinHeight.Value = new Dimension(14.0f + Padding.Value.Vertical, DimensionUnit.Pixel);
        }
        
        override public bool Update()
        {
            //if ((new Random()).NextDouble() > 0.9995d)
             //   Height.Value.Scalar += 2f;


            Width.Value = Width.Value.WithScalar(ImGuiNET.ImGui.CalcTextSize(Content.Value).X + Padding.Value.Horizontal);

            return HasSizeChanged();
        }
    }
}
