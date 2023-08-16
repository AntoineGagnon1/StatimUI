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
    [Component("text", true)]
    public class TextComponent : Component
    {
        public Property<string> Content = new ValueProperty<string>("");
        public Property<Color> TextColor = new ValueProperty<Color>(Color.White);

        private string lastContent = "";
        private RenderCommand renderCommand = new RenderCommand();

        protected override void OnRender(Vector2 drawPosition)
        {
            if (Visible)
            {
                renderCommand.Transform = Matrix4x4.CreateTranslation(drawPosition.X, drawPosition.Y, 0);
                Renderer.CurrentLayer.Commands.Add(renderCommand);
            }
        }

        public override void Start(IList<Component> slots)
        {
        }
        
        override public bool Update()
        {
            if (Content.Value != lastContent)
            {
                lastContent = Content.Value;
                renderCommand = FontManager.GetFont("arial.ttf", 14).MakeText(Content.Value, TextColor.Value, out var textSize);
                Width.Value = Width.Value.WithScalar(textSize.X);
                Height.Value = Height.Value.WithScalar(textSize.Y);
            }
            if ((new Random()).NextDouble() > 0.9995d)
               Content.Value += "a";

            return HasSizeChanged();
        }
    }
}
