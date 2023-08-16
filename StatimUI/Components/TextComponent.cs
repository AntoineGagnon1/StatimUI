﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    [Component("text", true)]
    public class TextComponent : Component
    {
        public Property<string> Content = new ValueProperty<string>("");

        protected override void OnRender(Vector2 drawPosition)
        {
            if (Visible)
            {
                // TODO : cache cmd
                var cmd = FontManager.GetFont("arial.ttf", 14).MakeText(Content.Value, Color.FromHex(0xFF5000));
                cmd.Transform = Matrix4x4.CreateTranslation(drawPosition.X, drawPosition.Y, 0);
                Renderer.CurrentLayer.Commands.Add(cmd);
            }
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


            //Width.Value.Scalar = ImGuiNET.ImGui.CalcTextSize(Content.Value).X + Padding.Value.Horizontal;

            return HasSizeChanged();
        }
    }
}
