using Microsoft.CodeAnalysis.CSharp.Syntax;
using StatimUI.Rendering;
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
                var startPos = offset + DrawPosition;
                Vector4 color = new Vector4(1, 0, 0, 1);
                Renderer.CurrentLayer.AddTriangle(new (startPos, color), new (startPos + new Vector2(20,0), new Vector4(1,0,1,1)), new (startPos + new Vector2(20, 10), color));
                //ImGuiNET.ImGui.SetCursorPos(offset + DrawPosition);
                //ImGuiNET.ImGui.Text(Content.Value);
            }
        }

        public override void Start(IList<Component> slots)
        {
            MinWidth.Value = new Dimension(10.0f + Padding.Value.Horizontal, DimensionUnit.Pixel);
            MinHeight.Value = new Dimension(14.0f + Padding.Value.Vertical, DimensionUnit.Pixel);
        }
        
        override public bool Update()
        {
            if ((new Random()).NextDouble() > 0.9995d)
                Height.Value.Scalar += 2f;


            //Width.Value.Scalar = ImGuiNET.ImGui.CalcTextSize(Content.Value).X + Padding.Value.Horizontal;

            return HasSizeChanged();
        }
    }
}
